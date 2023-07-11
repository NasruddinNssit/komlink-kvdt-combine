using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
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
using NssIT.Train.Kiosk.Common.Constants;

namespace NssIT.Kiosk.Server.AccessDB.CommandExec
{
    public class ETSCheckoutSaleExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";

        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        private string _webApiUrl = @"TrainService/etsCheckoutSale";

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            string methodTag = "ETSCheckoutSaleExecution.Execute";
            string domainEntityTag = "ETS/Intercity Sale Checkout";

            _serverAccess = serverAccess;
            _commandPack = commPack;

            AppDecorator.Config.Setting _setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

            ETSCheckoutSaleCommand axComm = null;
            bool eventSent = false;
            UIETSCheckoutSaleResult uicheckOutRes;

            try
            {
                axComm = (ETSCheckoutSaleCommand)_commandPack.Command;

                ETSCheckoutSaleRequest apiParam = new ETSCheckoutSaleRequest()
                {
                    Booking_Id = axComm.TransactionNo,
                    Channel = _setting.PurchaseChannel,
                    MCounters_Id = _setting.KioskId, 
                    PassengerSeatCount = axComm.TotalSeatCount
                };

                // KTM Submit Passenger Info XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                dynamic apiRes = _serverAccess.WebAPI.Post<ETSCheckoutSaleResult>(apiParam, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                bool validDataFound = false;

                if ((apiRes is ETSCheckoutSaleResult webCheckOutRes) && (webCheckOutRes.Code.Equals(WebAPIAgent.ApiCodeOK)))
                {
                    if ((eventSent == false)
                        && (webCheckOutRes.Data?.GetPassengerResult?.Error.Equals(YesNo.No) == true)
                        && (webCheckOutRes.Data.GetPassengerResult.PassengerDetailModels?.Length > 0)
                        && (webCheckOutRes.Data.GetPassengerResult.PassengerDetailModels.Length == axComm.TotalSeatCount)
                        && (string.IsNullOrWhiteSpace(webCheckOutRes.Data.CheckoutBookingResult?.BookingNo) == false)
                        && (string.IsNullOrWhiteSpace(webCheckOutRes.Data.CheckoutBookingResult?.Error) == false)
                        && (webCheckOutRes.Data.CheckoutBookingResult?.Error.Equals(YesNo.No) == true)
                        )
                    {
                        uicheckOutRes = new UIETSCheckoutSaleResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, webCheckOutRes);
                        commPack.UpSertResult(false, uicheckOutRes);
                        whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A02", uicheckOutRes);
                        validDataFound = true;
                    }
                    else if (eventSent == false)
                    {
                        string errMsg = "*";

                        if ((string.IsNullOrWhiteSpace(webCheckOutRes.MessageString()) == false))
                        {
                            errMsg += webCheckOutRes.MessageString();
                        }

                        if ((string.IsNullOrWhiteSpace(webCheckOutRes.Data?.GetPassengerResult?.ErrorMessage) == false))
                        {
                            errMsg += $@"; {webCheckOutRes.Data.GetPassengerResult.ErrorMessage}";
                        }

                        Log.LogText(LogChannel, "-", webCheckOutRes, "A03", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errMsg}; MsgObj: ETSCheckoutSaleResult");

                        apiRes = new WebApiException(new Exception($@"Invalid data transaction; {errMsg}; (EXIT21840)"));
                    }
                }

                if (validDataFound == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is ETSCheckoutSaleResult errResult)
                    {
                        errCode = "99";
                        errorMsg = $@"No valid data ({domainEntityTag}) found; Code: {errCode}; (EXIT21841)";

                        Log.LogText(LogChannel, "-", errResult, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: ETSCheckoutSaleResult");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT21842)";

                        Log.LogText(LogChannel, "-", errorMsg, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unexpected error occurred; ({domainEntityTag}); Code: {errCode};(EXIT21843)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    ETSCheckoutSaleResult errData = new ETSCheckoutSaleResult() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uicheckOutRes = new UIETSCheckoutSaleResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT21844)" : errorMsg) };

                    commPack.UpSertResult(true, uicheckOutRes, uicheckOutRes.ErrorMessage);

                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A10", uicheckOutRes, new Exception(uicheckOutRes.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                uicheckOutRes = new UIETSCheckoutSaleResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, null) { ErrorMessage = $@"{ex.Message}; (EXIT21845)" };
                commPack.UpSertResult(true, uicheckOutRes, errorMessage: uicheckOutRes.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A15", uicheckOutRes, new Exception(uicheckOutRes.ErrorMessage));
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
