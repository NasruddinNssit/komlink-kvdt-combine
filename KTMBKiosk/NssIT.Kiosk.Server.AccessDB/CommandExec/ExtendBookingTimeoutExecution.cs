using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Log.DB;
using NssIT.Train.Kiosk.Common.Common;
using NssIT.Train.Kiosk.Common.Common.WebApi;
using NssIT.Train.Kiosk.Common.Constants;
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
    public class ExtendBookingTimeoutExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";
        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private ExtendBookingTimeoutCommand _command = null;
        private string _webApiUrl = @"TrainService/extendBookingTimeout";
        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            string methodTag = "ExtendBookingTimeoutExecution.Execute";
            string domainEntityTag = "Extend Booking Timeout";

            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            UISalesBookingTimeoutExtensionResult uiBkTtExtRes;

            try
            {
                // KTM Save Settlement Info XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

                _command = (ExtendBookingTimeoutCommand)commPack.Command;

                BookingTimeoutExtensionRequest param = new BookingTimeoutExtensionRequest()
                {
                    Booking_Id = _command.BookingId, 
                    Channel = setting.PurchaseChannel, 
                    MCounters_Id = setting.KioskId
                };

                dynamic apiRes = _serverAccess.WebAPI.Post<ExtendBookingSessionResult>(param, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                bool validDataFound = false;

                if ((apiRes is ExtendBookingSessionResult extBkSessResX) && (extBkSessResX.Code.Equals(WebAPIAgent.ApiCodeOK)))
                {
                    ExtendBookingSessionResult extBkSessRes = extBkSessResX;

                    if (extBkSessRes.Data?.Error?.Equals(YesNo.No) == true)                        
                    {
                        uiBkTtExtRes = new UISalesBookingTimeoutExtensionResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, extBkSessRes);
                        commPack.UpSertResult(false, uiBkTtExtRes);
                        whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A02", uiBkTtExtRes);
                        validDataFound = true;
                    }
                    else
                    {
                        apiRes = new WebApiException(new Exception($@"No valid data found; (EXIT21830); Error Code : {extBkSessResX?.Code}"));

                        Log.LogText(LogChannel, "-", extBkSessResX, "A03", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"No valid data found; (EXIT21830); Error Code : {extBkSessResX?.Code}; MsgObj: ExtendBookingSessionResult");
                    }
                }

                if (validDataFound == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is ExtendBookingSessionResult errResult)
                    {
                        errCode = "99";
                        errorMsg = $@"No valid data ({domainEntityTag}) found; Code: {errCode}; (EXIT21831)";

                        Log.LogText(LogChannel, "-", errResult, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: ExtendBookingSessionResult");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT21832)";

                        Log.LogText(LogChannel, "-", errorMsg, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unexpected error occurred; ({domainEntityTag}); Code: {errCode}; (EXIT21833)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    ExtendBookingSessionResult errData = new ExtendBookingSessionResult() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uiBkTtExtRes = new UISalesBookingTimeoutExtensionResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT21834)" : errorMsg) };

                    commPack.UpSertResult(true, uiBkTtExtRes, uiBkTtExtRes.ErrorMessage);

                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A10", uiBkTtExtRes, new Exception(uiBkTtExtRes.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                List<string> msgs = new List<string>(new string[] { $@"{ex.Message}; (EXIT21835)" });
                ExtendBookingSessionResult errData = new ExtendBookingSessionResult() { Code = "99", Data = null, Messages = msgs, Status = false };

                uiBkTtExtRes = new UISalesBookingTimeoutExtensionResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData) { ErrorMessage = errData.MessageString() };
                commPack.UpSertResult(true, uiBkTtExtRes, errorMessage: uiBkTtExtRes.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}.Execute:A11", uiBkTtExtRes, new Exception(uiBkTtExtRes.ErrorMessage));
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
