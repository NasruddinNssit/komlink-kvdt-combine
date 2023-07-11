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
    public class GetKomuterTicketTypePackage : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";
        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private KomuterTicketTypePackageCommand _command = null;
        private string _webApiUrl = @"Komuter/getPackage";
        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            string methodTag = "GetKomuterTicketTypePackage.Execute";
            string domainEntityTag = "Get Komuter Ticket Type Package";

            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            UIKomuterTicketTypePackageAck uiTickTypePack;

            try
            {
                // KTM Trip List XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

                _command = (KomuterTicketTypePackageCommand)commPack.Command;

                IPostRequestParam param = new GetKomuterPackageRequest()
                {
                    CounterId = setting.KioskId, 
                    OriginStationId = _command.OriginStationId, 
                    DestinationStationId = _command.DestinationStationId
                };

                dynamic apiRes = _serverAccess.WebAPI.Post<KomuterTicketTypePackageResult>(param, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                bool validDataFound = false;
                if ((apiRes is KomuterTicketTypePackageResult tickTypPackX) && (tickTypPackX.Code.Equals(WebAPIAgent.ApiCodeOK)))
                {
                    KomuterTicketTypePackageResult tickTypPack = tickTypPackX;

                    if ((tickTypPack?.Data?.KomuterPackages?.Length > 0))
                    {
                        //testMsg = $@"Success; Record Cound : {sttr.data?.Count().ToString() ?? "--"}";
                        uiTickTypePack = new UIKomuterTicketTypePackageAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, tickTypPack);
                        commPack.UpSertResult(false, uiTickTypePack);
                        whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A02", uiTickTypePack);
                        validDataFound = true;
                    }
                    else
                    {
                        apiRes = new WebApiException(new Exception($@"No valid data found; (EXIT21686); Ticket Type Count : 0"));
                        Log.LogText(LogChannel, "-", tickTypPackX, "A03", 
                            methodTag,
                            AppDecorator.Log.MessageType.Error, 
                            netProcessId: commPack.NetProcessId, 
                            extraMsg: "(EXIT21686); MsgObj: KomuterTicketTypePackageResult");
                    }
                }

                if (validDataFound == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is KomuterTicketTypePackageResult errResult)
                    {
                        errCode = "99";
                        errorMsg = $@"No valid data ({domainEntityTag}) found; Code: {errCode}; (EXIT21687)";

                        Log.LogText(LogChannel, "-", errResult, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: KomuterTicketTypePackageResult");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT21688)";

                        Log.LogText(LogChannel, "-", errorMsg, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unexpected error occurred; ({domainEntityTag}); Code: {errCode}; (EXIT21689)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    KomuterTicketTypePackageResult errData = new KomuterTicketTypePackageResult() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uiTickTypePack = new UIKomuterTicketTypePackageAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT21690)" : errorMsg) };

                    commPack.UpSertResult(true, uiTickTypePack, uiTickTypePack.ErrorMessage);

                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A10", uiTickTypePack, new Exception(uiTickTypePack.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                List<string> msgs = new List<string>(new string[] { $@"{ex.Message}; (EXIT21690A)" });
                KomuterTicketTypePackageResult errData = new KomuterTicketTypePackageResult() { Code = "99", Data = null, Messages = msgs, Status = false };

                uiTickTypePack = new UIKomuterTicketTypePackageAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData) { ErrorMessage = errData.MessageString() };
                commPack.UpSertResult(true, uiTickTypePack, errorMessage: uiTickTypePack.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}.Execute:A11", uiTickTypePack, new Exception(uiTickTypePack.ErrorMessage));
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
