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
    public class GetCustInfoPrerequisiteExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";

        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        private string _webApiUrl = @"TrainService/getTicketType";

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            string methodTag = "GetCustInfoPrerequisiteExecution.Execute";
            string domainEntityTag = "Get Custumer Info Prerequisite Data";
            AppDecorator.Config.Setting _setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

            _serverAccess = serverAccess;
            _commandPack = commPack;

            bool eventSent = false;
            UICustInfoPrerequisiteAck uiCustPreqData = null;

            try
            {
                CustInfoPrerequisiteRequestCommand custInfoPreqComm = (CustInfoPrerequisiteRequestCommand)_commandPack.Command;

                // KTM Get Custumer Info Prerequisite Data - Ticket Type XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                GetTicketTypeRequest apiParams = new GetTicketTypeRequest()
                {
                    Channel = _setting.PurchaseChannel,
                    MCounters_Id = _setting.KioskId,
                    TrainService = custInfoPreqComm.TrainService
                };

                dynamic apiRes = _serverAccess.WebAPI.Post<GetTicketTypeResult>(apiParams, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                bool validDataFound = false;

                if ((apiRes is GetTicketTypeResult ttr) && (ttr.Code.Equals(WebAPIAgent.ApiCodeOK)))
                {
                    if ((ttr.Data?.Length > 0))
                    {
                        uiCustPreqData = new UICustInfoPrerequisiteAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, ttr);
                        commPack.UpSertResult(false, uiCustPreqData);
                        whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A02", uiCustPreqData);
                        validDataFound = true;
                    }
                    else
                        apiRes = new WebApiException(new Exception("No valid Ticket Type found; (EXIT21711)"));
                }

                if (validDataFound == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is GetTicketTypeResult errResult)
                    {
                        errCode = "99";
                        errorMsg = $@"No valid data ({domainEntityTag}) found; Code: {errCode}; (EXIT21712)";

                        Log.LogText(LogChannel, "-", errResult, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: GetTicketTypeResult");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT21713)";

                        Log.LogText(LogChannel, "-", errorMsg, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unexpected error occurred; ({domainEntityTag}); Code: {errCode};(EXIT21714)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    GetTicketTypeResult errData = new GetTicketTypeResult() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uiCustPreqData = new UICustInfoPrerequisiteAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT21715)" : errorMsg) };

                    commPack.UpSertResult(true, uiCustPreqData, uiCustPreqData.ErrorMessage);

                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A10", uiCustPreqData, new Exception(uiCustPreqData.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                uiCustPreqData = new UICustInfoPrerequisiteAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, null) { ErrorMessage = $@"{ex.Message}; (EXIT21716)" };
                commPack.UpSertResult(true, uiCustPreqData, errorMessage: uiCustPreqData.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A15", uiCustPreqData, new Exception(uiCustPreqData.ErrorMessage));
            }

            return commPack;
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            void whenCompletedSendEvent(ResultStatus resultState, Guid? netProcessId, string processId, string lineTag, IKioskMsg uiKioskMsg,
                Exception error = null)
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
        }
    }
}

