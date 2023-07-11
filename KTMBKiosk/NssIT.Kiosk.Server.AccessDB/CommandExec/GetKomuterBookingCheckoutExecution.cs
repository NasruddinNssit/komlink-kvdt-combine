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
    public class GetKomuterBookingCheckoutExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";
        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private KomuterBookingCheckoutCommand _command = null;
        private string _webApiUrl = @"Komuter/checkoutBooking";
        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            string methodTag = "GetKomuterBookingCheckoutExecution.Execute";
            string domainEntityTag = "Get Komuter Booking Checkout State";

            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            UIKomuterBookingCheckoutAck uiBookChkOut;

            try
            {
                // KTM Trip List XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

                _command = (KomuterBookingCheckoutCommand)commPack.Command;

                KomuterBookingCheckoutRequest param = new KomuterBookingCheckoutRequest()
                {
                    Channel = setting.PurchaseChannel, 
                    Booking_Id = _command.BookingId, 
                    //FinancePaymentMethod = _command.FinancePaymentMethod, 

                    MCounter_Id = setting.KioskId, 
                    TotalAmount = _command.TotalAmount
                };

                dynamic apiRes = _serverAccess.WebAPI.Post<KomuterBookingCheckoutResult>(param, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                bool validDataFound = false;

                if ((apiRes is KomuterBookingCheckoutResult bookChkX) && (bookChkX.Code?.Equals(WebAPIAgent.ApiCodeOK) == true))
                {
                    KomuterBookingCheckoutResult bookChk = bookChkX;

                    if ((bookChk.Data?.Error?.Equals(YesNo.No) == true)
                        && (string.IsNullOrWhiteSpace(bookChk.Data?.BookingNo) == false))
                    {
                        uiBookChkOut = new UIKomuterBookingCheckoutAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, bookChk);
                        commPack.UpSertResult(false, uiBookChkOut);
                        whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A02", uiBookChkOut);
                        validDataFound = true;
                    }
                    else
                    {
                        apiRes = new WebApiException(new Exception($@"No valid data found; (EXIT21770); Error Code : {bookChk?.Code}"));

                        Log.LogText(LogChannel, "-", bookChkX, "A03", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"No valid data found; (EXIT21770); Error Code : {bookChk?.Code}; MsgObj: KomuterBookingCheckoutResult");
                    }
                }

                if (validDataFound == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is KomuterBookingCheckoutResult errResult)
                    {
                        errCode = "99";
                        errorMsg = $@"No valid data ({domainEntityTag}) found; Code: {errCode}; (EXIT21771)";

                        Log.LogText(LogChannel, "-", errResult, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: KomuterBookingCheckoutResult");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT21772)";

                        Log.LogText(LogChannel, "-", errorMsg, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unexpected error occurred; ({domainEntityTag}); Code: {errCode}; (EXIT21773)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    KomuterBookingCheckoutResult errData = new KomuterBookingCheckoutResult() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uiBookChkOut = new UIKomuterBookingCheckoutAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT21774)" : errorMsg) };

                    commPack.UpSertResult(true, uiBookChkOut, uiBookChkOut.ErrorMessage);

                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A10", uiBookChkOut, new Exception(uiBookChkOut.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                List<string> msgs = new List<string>(new string[] { $@"{ex.Message}; (EXIT21775)" });
                KomuterBookingCheckoutResult errData = new KomuterBookingCheckoutResult() { Code = "99", Data = null, Messages = msgs, Status = false };

                uiBookChkOut = new UIKomuterBookingCheckoutAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData) { ErrorMessage = errData.MessageString() };
                commPack.UpSertResult(true, uiBookChkOut, errorMessage: uiBookChkOut.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A11", uiBookChkOut, new Exception(uiBookChkOut.ErrorMessage));
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

