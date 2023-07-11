using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Constant.BTnG;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Sqlite.DB;
using NssIT.Train.Kiosk.Common.Common;
using NssIT.Train.Kiosk.Common.Common.WebApi;
using NssIT.Train.Kiosk.Common.Constants;
using NssIT.Train.Kiosk.Common.Data;
using NssIT.Train.Kiosk.Common.Data.PostRequestParam;
using NssIT.Train.Kiosk.Common.Data.Response;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.AccessDB.CommandExec
{
    /// <summary>
    /// ClassCode:EXIT25.05
    /// </summary>
    public class KomuterCompletePaymentExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";
        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private KomuterCompletePaymentCommand _command = null;
        private string _webApiUrl = @"Komuter/doMakePayment";
        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            string methodTag = "KomuterCompletePaymentExecution.Execute";
            string domainEntityTag = "Komuter Complete Payment";

            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            UIKomuterCompletePaymentAck uiCompltPay;

            try
            {
                // KTM Trip List XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

                _command = (KomuterCompletePaymentCommand)commPack.Command;

                CompleteKomuterPaymentRequest param = new CompleteKomuterPaymentRequest()
                {
                    Channel = setting.PurchaseChannel,
                    Booking_Id = _command.BookingId,
                    MCounters_Id = setting.KioskId,
                    Amount = _command.Amount, 
                    MCurrencies_Id = _command.CurrencyId
                };

                if (_command.TypeOfPayment == PaymentType.PaymentGateway)
                {
                    param.FinancePaymentMethod = _command.FinancePaymentMethod;
                    param.ReferenceNo = _command.BTnGSaleTransactionNo;
                    param.PaymentGatewaySuccessPaidModel = new BTnGSuccessPaidModel()
                    {
                        SalesTransactionNo = _command.BTnGSaleTransactionNo,
                        HeaderStatus = ((int)BTnGHeaderStatus.SUCCESS).ToString(),
                        DetailStatus = ((int)BTnGDetailStatus.w_paid_ack).ToString(),

                        LineTag = "Kiosk_Komuter_Transaction_A01",
                        User = setting.KioskId,
                        Remark = "Transaction successful done in Kiosk"
                    };
                }
                else
                {
                    param.ReferenceNo = _command.CreditCardAnswer.rrn ?? "";
                    param.FinancePaymentMethod = FinancePaymentMethod.CreditCard;
                    param.CreditCardResponse = new CreditCardResponseModel()
                    {
                        adat = _command.CreditCardAnswer.adat ?? "",
                        aid = _command.CreditCardAnswer.aid ?? "",
                        apvc = _command.CreditCardAnswer.apvc ?? "",
                        bcam = _command.CreditCardAnswer.bcam,
                        bcno = _command.CreditCardAnswer.bcno ?? "",
                        btct = _command.CreditCardAnswer.btct ?? "",
                        camt = _command.CreditCardAnswer.camt,
                        cdnm = _command.CreditCardAnswer.cdnm ?? "",
                        cdno = _command.CreditCardAnswer.cdno ?? "",
                        cdty = _command.CreditCardAnswer.cdty ?? "",
                        erms = _command.CreditCardAnswer.erms ?? "",
                        hsno = _command.CreditCardAnswer.hsno ?? "",
                        mcid = setting.KioskId ?? "*#*",
                        mid = _command.CreditCardAnswer.mid ?? "",
                        rmsg = _command.CreditCardAnswer.rmsg ?? "",
                        rrn = _command.CreditCardAnswer.rrn ?? "",
                        stcd = _command.CreditCardAnswer.stcd ?? "",
                        tid = _command.CreditCardAnswer.tid ?? "",
                        trcy = _command.CreditCardAnswer.trcy ?? "",
                        trdt = _command.CreditCardAnswer.trdt,
                        ttce = _command.CreditCardAnswer.ttce ?? "",
                        stmStcd = "",
                        stmTrid = -1
                    };
                }

                dynamic apiRes = _serverAccess.WebAPI.Post<CompleteKomuterPaymentResult>(param, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                bool validDataFound = false;

                if ((apiRes is CompleteKomuterPaymentResult payResX) && (payResX.Code?.Equals(WebAPIAgent.ApiCodeOK) == true))
                {
                    CompleteKomuterPaymentResult payRes = payResX;

                    if ((payRes.Data?.KomuterBookingPaymentResult?.Error?.Equals(YesNo.No) == true) 
                        && (string.IsNullOrWhiteSpace(payRes.Data?.KomuterBookingPaymentResult?.BookingNo) == false)
                        && (payRes.Data?.KomuterTicketPrintList?.Length > 0)
                        )
                    {
                        uiCompltPay = new UIKomuterCompletePaymentAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, payRes, setting.KioskId);
                        commPack.UpSertResult(false, uiCompltPay);
                        whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A02", uiCompltPay);
                        validDataFound = true;

                        if (_command.TypeOfPayment == AppDecorator.Common.AppService.Sales.PaymentType.PaymentGateway)
                            UpdateLocalTransactionDB(_command.BTnGSaleTransactionNo, BTnGHeaderStatus.SUCCESS, BTnGDetailStatus.w_paid_ack);
                    }
                    else
                    {
                        apiRes = new WebApiException(new Exception($@"No valid data found; (EXIT21780); Error Code : {payRes?.Code}"));

                        Log.LogText(LogChannel, "-", payResX, "A03", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"No valid data found; (EXIT21780); Error Code : {payRes?.Code}; MsgObj: CompleteKomuterPaymentResult");

                        if (_command.TypeOfPayment == AppDecorator.Common.AppService.Sales.PaymentType.PaymentGateway)
                            UpdateLocalTransactionDB(_command.BTnGSaleTransactionNo, BTnGHeaderStatus.PENDING, BTnGDetailStatus.w_web_api_error);
                    }
                }
                else
                {
                    UpdateLocalTransactionDB(_command.BTnGSaleTransactionNo, BTnGHeaderStatus.PENDING, BTnGDetailStatus.w_web_api_error);
                }

                if (validDataFound == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is CompleteKomuterPaymentResult errResult)
                    {
                        errCode = "99";
                        errorMsg = $@"No valid data ({domainEntityTag}) found; Code: {errCode}; (EXIT21781)";

                        Log.LogText(LogChannel, "-", errResult, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: CompleteKomuterPaymentResult");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT21782)";

                        Log.LogText(LogChannel, "-", errorMsg, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unexpected error occurred; ({domainEntityTag}); Code: {errCode}; (EXIT21783)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    CompleteKomuterPaymentResult errData = new CompleteKomuterPaymentResult() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uiCompltPay = new UIKomuterCompletePaymentAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData, setting.KioskId)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT21784)" : errorMsg) };

                    commPack.UpSertResult(true, uiCompltPay, uiCompltPay.ErrorMessage);

                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A10", uiCompltPay, new Exception(uiCompltPay.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                List<string> msgs = new List<string>(new string[] { $@"{ex.Message}; (EXIT21785)" });
                CompleteKomuterPaymentResult errData = new CompleteKomuterPaymentResult() { Code = "99", Data = null, Messages = msgs, Status = false };

                uiCompltPay = new UIKomuterCompletePaymentAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData, "*#*") { ErrorMessage = errData.MessageString() };
                commPack.UpSertResult(true, uiCompltPay, errorMessage: uiCompltPay.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A11", uiCompltPay, new Exception(uiCompltPay.ErrorMessage));
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

            /// <summary>
            /// FuncCode:EXIT25.058B
            /// </summary>
            void UpdateLocalTransactionDB(string bTnGSaleTransNo, BTnGHeaderStatus headerStatus, BTnGDetailStatus detailStatus)
            {
                SQLiteConnection conn = null;
                SQLiteTransaction trn = null;

                try
                {
                    conn = new SQLiteConnection(DBManager.DBMan.ConnectionString);
                    conn.Open();
                    trn = conn.BeginTransaction();

                    using (BTnGTransactionDBTrans dbTrans = new BTnGTransactionDBTrans())
                    {
                        // Get Header Status
                        dbTrans.QueryHeaderStatus(bTnGSaleTransNo, conn, trn,
                            out AppDecorator.DomainLibs.Sqlite.DB.Constant.BTnG.BTnGHeaderStatus? resHeaderStatus, out bool isFound);

                        if (isFound == false)
                            throw new Exception("BTnG sale transaction record not found; (EXIT25.058B.X01)");

                        else if (resHeaderStatus.HasValue == false)
                            throw new Exception("BTnG sale transaction header's status not found; (EXIT25.058B.X02)");

                        //else if ((resHeaderStatus.Value == BTnGHeaderStatus.FAIL) ||
                        //    (resHeaderStatus.Value == BTnGHeaderStatus.SUCCESS) ||
                        //    (resHeaderStatus.Value == BTnGHeaderStatus.CANCEL)
                        //    )
                        //{
                        //    /*By-pass*/
                        //}

                        else
                        {
                            Log.LogText(LogChannel, bTnGSaleTransNo, $@"Last HeaderStatus: {resHeaderStatus.Value}; Curr.Header Status : {headerStatus}; Curr.Detail Status : {detailStatus}; BTnG-{bTnGSaleTransNo}", "BTNG_B02", "KomuterCompletePaymentExecution.UpdateLocalTransactionDB");
                            dbTrans.UpdatePaymentStatus(bTnGSaleTransNo, headerStatus, detailStatus, conn, trn);
                        }

                    }

                    trn.Commit();
                }
                catch (Exception ex)
                {
                    if (trn != null)
                    {
                        try
                        {
                            trn.Rollback();
                        }
                        catch { }
                    }

                    Log.LogError(LogChannel, $@"BTnG-{bTnGSaleTransNo}", ex, "BTNG_EX01", "KomuterCompletePaymentExecution.UpdateLocalTransactionDB");
                }
                finally
                {
                    trn = null;
                    if (conn != null)
                    {
                        try
                        {
                            conn.Close();
                        }
                        catch { }
                        try
                        {
                            conn.Dispose();
                        }
                        catch { }
                    }
                }
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

