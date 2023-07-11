using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
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
    public class VerifyPromoCodeExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";

        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        private string _webApiUrl = @"TrainService/validatePromoCode";

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            string methodTag = "VerifyPromoCodeExecution.Execute";
            string domainEntityTag = "Promo Code Verification";

            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            CustPromoCodeVerifyCommand command = (CustPromoCodeVerifyCommand)_commandPack.Command;
            UICustPromoCodeVerifyAck uiPromoCode = null;
            AppDecorator.Config.Setting _setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

            methodTag = $@"{methodTag}; Promo Code.: {command.PromoCode}";
            domainEntityTag = $@"{domainEntityTag}; Promo Code : {command.PromoCode}";

            try
            {   
                // KTM Verify Promo Code XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                PromoCodeVerifyRequest apiParams = new PromoCodeVerifyRequest()
                { 
                    PromoCode = command.PromoCode, 
                    SeatLayoutModel_Id = command.SeatLayoutModelId, 
                    TicketTypes_Id = command.TicketTypesId, 
                    TrainSeatModel_Id = command.TrainSeatModelId, 
                    PassengerIC = command.PassengerIC,
                    TripBookingDetails_Id = null,
                    Channel = _setting.PurchaseChannel
                };

                dynamic apiRes = _serverAccess.WebAPI.Post<PromoCodeVerifyResult>(apiParams, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                bool validDataFound = false;

                if ((apiRes is PromoCodeVerifyResult prmVf) && (prmVf.Code.Equals(WebAPIAgent.ApiCodeOK)) && (prmVf.Data != null) && (prmVf.Data.Error.Equals(YesNo.No)))
                {
                    uiPromoCode = new UICustPromoCodeVerifyAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, prmVf);
                    commPack.UpSertResult(errorFound: false, resultData: uiPromoCode);
                    whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A02", uiPromoCode);
                    validDataFound = true;
                }
                
                if (validDataFound == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is PromoCodeVerifyResult errResult)
                    {
                        errCode = "99";

                        if (string.IsNullOrWhiteSpace(errResult.Data.ErrorMessage) == false)
                        {
                            errorMsg = $@"{errResult.Data.ErrorMessage}; Error when {domainEntityTag}; (EXIT21671)";
                        }
                        else
                            errorMsg = $@"No valid data ({domainEntityTag}) found; Code: {errCode}; (EXIT21672)";

                        Log.LogText(LogChannel, "-", errResult, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: PromoCodeVerifyResult");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT21673)";

                        Log.LogText(LogChannel, "-", wex, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: WebApiException");
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unexpected error occurred; ({domainEntityTag}); Code: {errCode};(EXIT21674)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    PromoCodeVerifyResult errData = new PromoCodeVerifyResult() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uiPromoCode = new UICustPromoCodeVerifyAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT21675)" : errorMsg) };

                    commPack.UpSertResult(true, uiPromoCode, uiPromoCode.ErrorMessage);

                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A10", uiPromoCode, new Exception(uiPromoCode.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                PromoCodeVerifyResult errData = new PromoCodeVerifyResult() { Code = "99", Data = null, Messages = new List<string>(new string[1] { $@"{ex.Message}; (EXIT21676)" }), Status = false };
                uiPromoCode = new UICustPromoCodeVerifyAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData) { ErrorMessage = $@"{ex.Message}; (EXIT21676)" };
                commPack.UpSertResult(true, uiPromoCode, errorMessage: uiPromoCode.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A15", uiPromoCode, new Exception(uiPromoCode.ErrorMessage));
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