using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Log.DB;
using NssIT.Train.Kiosk.Common.Common;
using NssIT.Train.Kiosk.Common.Common.WebApi;
using NssIT.Train.Kiosk.Common.Data;
using NssIT.Train.Kiosk.Common.Data.PostRequestParam;
using NssIT.Train.Kiosk.Common.Data.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.AccessDB.CommandExec
{
    public class GetETSInterCityTicketExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";
        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private ETSIntercityTicketRequestCommand _command = null;
        private string _webApiUrl = @"Ticket/getETSIntercityTicketData";
        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            string methodTag = "GetETSInterCityTicketExecution.Execute";
            string domainEntityTag = "Get ETS InterCity Ticket";

            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            UIETSIntercityTicketAck uiTicket;

            try
            {
                // KTM Trip List XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

                _command = (ETSIntercityTicketRequestCommand)commPack.Command;

                IPostRequestParam param = new TicketETSIntercityRequest()
                {
                    BookingNo = _command.BookingNo
                };

                dynamic apiRes = _serverAccess.WebAPI.Post<IntercityETSTicketListResult>(param, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                bool validDataFound = false;

                if ((apiRes is IntercityETSTicketListResult tickX) && (tickX.Code.Equals(WebAPIAgent.ApiCodeOK)))
                {
                    IntercityETSTicketListResult tick = tickX;

                    if ((tick?.Data.Length > 0))
                    {
                        uiTicket = new UIETSIntercityTicketAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, tick);
                        commPack.UpSertResult(false, uiTicket);
                        whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A02", uiTicket);
                        validDataFound = true;
                    }
                    else
                    {
                        apiRes = new WebApiException(new Exception($@"No valid data found; (EXIT21740); Error Code : {tick?.Code}"));

                        Log.LogText(LogChannel, "-", tickX, "A03", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"No valid data found; (EXIT21740); Error Code : {tick?.Code}; MsgObj: IntercityETSTicketListResult");
                    }
                }

                if (validDataFound == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is IntercityETSTicketListResult errResult)
                    {
                        errCode = "99";
                        errorMsg = $@"No valid data ({domainEntityTag}) found; Code: {errCode}; (EXIT21741)";

                        Log.LogText(LogChannel, "-", errResult, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: IntercityETSTicketListResult");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT21742)";

                        Log.LogText(LogChannel, "-", errorMsg, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unexpected error occurred; ({domainEntityTag}); Code: {errCode}; (EXIT21743)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    IntercityETSTicketListResult errData = new IntercityETSTicketListResult() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uiTicket = new UIETSIntercityTicketAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT21744)" : errorMsg) };

                    commPack.UpSertResult(true, uiTicket, uiTicket.ErrorMessage);

                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A10", uiTicket, new Exception(uiTicket.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                List<string> msgs = new List<string>(new string[] { $@"{ex.Message}; (EXIT21745)" });
                IntercityETSTicketListResult errData = new IntercityETSTicketListResult() { Code = "99", Data = null, Messages = msgs, Status = false };

                uiTicket = new UIETSIntercityTicketAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData) { ErrorMessage = errData.MessageString() };
                commPack.UpSertResult(true, uiTicket, errorMessage: uiTicket.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}.Execute:A11", uiTicket, new Exception(uiTicket.ErrorMessage));
            }

            return commPack;
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            void whenCompletedSendEvent(ResultStatus resultState, Guid? netProcessId, string processId, string lineTag,
                IKioskMsg uiKioskMsg, Exception error = null)
            {
                if (eventSent)
                    return;
                _serverAccess.RaiseOnSendMessage(resultState, netProcessId, processId, lineTag, uiKioskMsg, error);
                eventSent = true;
            }
        }

        public void Dispose()
        {
            _commandPack = null;
            _log = null;
            _serverAccess = null;
            _command = null;
        }
    }
}
