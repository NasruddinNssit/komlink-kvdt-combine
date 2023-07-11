using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
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

//CompleteTransactionElseReleaseSeatCommand//UICompleteTransactionResult//uiCompleteResult

namespace NssIT.Kiosk.Server.AccessDB.CommandExec
{
    /// <summary>
    /// ClassCode:EXIT25.03
    /// </summary>
    public class CompleteTransactionElseReleaseSeatExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";
        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private CompleteTransactionElseReleaseSeatCommand _command = null;
        //private string _webApiUrl = @"TrainService/completeTicketPayment";
        private string _webApiUrl = @"TrainService/doTicketPayment";
        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        /// <summary>
        /// FuncCode:EXIT25.0302
        /// </summary>
        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            string methodTag = "CompleteTransactionElseReleaseSeatExecution.Execute";
            string domainEntityTag = "Complete Ticket Payment Transaction";

            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            UICompleteTransactionResult uiTicket;

            try
            {
                // KTM Trip List XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

                _command = (CompleteTransactionElseReleaseSeatCommand)commPack.Command;

                //IPostRequestParam param = new UpdatePaymentRequest()
                //{
                //    Booking_Id = _command.TransactionNo,
                //    Channel = setting.PurchaseChannel,
                //    MCounters_Id = setting.KioskId, 
                //    MCurrencies_Id = _command.TradeCurrency, 
                //    BookingPaymentDetailModels = new BookingPaymentDetailModel[] 
                //    { new BookingPaymentDetailModel() 
                //        { 
                //            Amount = _command.TotalAmount,
                //            FinancePaymentMethod = FinancePaymentMethod.CreditCard,
                //            ReferenceNo = _command.BankReferenceNo
                //        } 
                //    }
                //};

                //JSonStringParameter jSonStrParam = new JSonStringParameter() { JSonString = JsonConvert.SerializeObject(param)};
                CompletePaymentRequest completePayReq = new CompletePaymentRequest()
                {
                    Amount = _command.TotalAmount,
                    Booking_Id = _command.TransactionNo,
                    Channel = setting.PurchaseChannel,
                    MCounters_Id = setting.KioskId,
                    MCurrencies_Id = _command.TradeCurrency,
                };

                if (_command.TypeOfPayment == AppDecorator.Common.AppService.Sales.PaymentType.PaymentGateway)
                {
                    completePayReq.FinancePaymentMethod = _command.PaymentMethod;
                    completePayReq.ReferenceNo = _command.BTnGSaleTransactionNo;
                    completePayReq.PaymentGatewaySuccessPaidModel = new BTnGSuccessPaidModel()
                    {
                        SalesTransactionNo = _command.BTnGSaleTransactionNo,
                        HeaderStatus = ((int)BTnGHeaderStatus.SUCCESS).ToString(),
                        DetailStatus = ((int)BTnGDetailStatus.w_paid_ack).ToString(),

                        LineTag = "Kiosk_ETS_Transaction_A01",
                        User = setting.KioskId,
                        Remark = "Transaction successful done in Kiosk"
                    };
                }

                // Default refer to Credit Card payment
                else
                {
                    completePayReq.FinancePaymentMethod = FinancePaymentMethod.CreditCard;
                    completePayReq.ReferenceNo = _command.BankReferenceNo;
                    completePayReq.CreditCardResponse = new CreditCardResponseModel()
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

                //PaymentSubmissionResult
                dynamic apiRes = _serverAccess.WebAPI.Post<PaymentSubmissionResult>(completePayReq, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                bool validDataFound = false;

                if ((apiRes is PaymentSubmissionResult tickX) && (tickX.Code.Equals(WebAPIAgent.ApiCodeOK)))
                {
                    PaymentSubmissionResult tick = tickX;

                    if ((tick.Data?.BookingPaymentResult?.Error?.Equals(YesNo.No) == true) 
                        && (string.IsNullOrWhiteSpace(tick.Data.BookingPaymentResult.BookingNo) == false)
                        && (tick.Data.IntercityETSTicketListResult?.Length > 0)
                        )
                    {
                        uiTicket = new UICompleteTransactionResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, tick, AppDecorator.Common.AppService.Sales.ProcessResult.Success, setting.KioskId);
                        commPack.UpSertResult(false, uiTicket);
                        whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A02", uiTicket);
                        validDataFound = true;

                        if (_command.TypeOfPayment == AppDecorator.Common.AppService.Sales.PaymentType.PaymentGateway)
                            UpdateLocalTransactionDB(_command.BTnGSaleTransactionNo, BTnGHeaderStatus.SUCCESS, BTnGDetailStatus.w_paid_ack);
                    }
                    else
                    {
                        apiRes = new WebApiException(new Exception($@"No valid data found; (EXIT21750); Error Code : {tick.Code}"));

                        Log.LogText(LogChannel, "-", tickX, "A03", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"No valid data found; (EXIT21750); Error Code : {tick.Code}; MsgObj: UICompleteTransactionResult");

                        if (_command.TypeOfPayment == AppDecorator.Common.AppService.Sales.PaymentType.PaymentGateway)
                            UpdateLocalTransactionDB(_command.BTnGSaleTransactionNo, BTnGHeaderStatus.PENDING, BTnGDetailStatus.w_web_api_error);
                    }
                }
                else
                {
                    if (_command.TypeOfPayment == AppDecorator.Common.AppService.Sales.PaymentType.PaymentGateway)
                        UpdateLocalTransactionDB(_command.BTnGSaleTransactionNo, BTnGHeaderStatus.PENDING, BTnGDetailStatus.w_web_api_error);
                }

                if (validDataFound == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is PaymentSubmissionResult errResult)
                    {
                        errCode = "99";
                        errorMsg = $@"No valid data ({domainEntityTag}) found; Code: {errCode}; (EXIT21751)";

                        Log.LogText(LogChannel, "-", errResult, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: PaymentSubmissionResult");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT21752)";

                        Log.LogText(LogChannel, "-", errorMsg, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unexpected error occurred; ({domainEntityTag}); Code: {errCode}; (EXIT21753)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    PaymentSubmissionResult errData = new PaymentSubmissionResult() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uiTicket = new UICompleteTransactionResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData, AppDecorator.Common.AppService.Sales.ProcessResult.Fail, setting.KioskId)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT21754)" : errorMsg) };

                    commPack.UpSertResult(true, uiTicket, uiTicket.ErrorMessage);

                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A10", uiTicket, new Exception(uiTicket.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                List<string> msgs = new List<string>(new string[] { $@"{ex.Message}; (EXIT21755)" });
                PaymentSubmissionResult errData = new PaymentSubmissionResult() { Code = "99", Data = null, Messages = msgs, Status = false };

                uiTicket = new UICompleteTransactionResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData, AppDecorator.Common.AppService.Sales.ProcessResult.Fail, "**") { ErrorMessage = errData.MessageString() };
                commPack.UpSertResult(true, uiTicket, errorMessage: uiTicket.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}.Execute:A11", uiTicket, new Exception(uiTicket.ErrorMessage));

                Log.LogError(LogChannel, _command.ProcessId, ex, "EX01", methodTag);
            }

            return commPack;
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

            /// <summary>
            /// FuncCode:EXIT25.038A
            /// </summary>
            void whenCompletedSendEvent(ResultStatus resultState, Guid? netProcessId, string processId, string lineTag,
                IKioskMsg uiKioskMsg, Exception error = null)
            {
                if (eventSent)
                    return;
                _serverAccess.RaiseOnSendMessage(resultState, netProcessId, processId, lineTag, uiKioskMsg, error);
                eventSent = true;
            }

            /// <summary>
            /// FuncCode:EXIT25.038B
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
                            throw new Exception("BTnG sale transaction record not found; (EXIT25.038B.X01)");

                        else if (resHeaderStatus.HasValue == false)
                            throw new Exception("BTnG sale transaction header's status not found; (EXIT25.038B.X02)");

                        //else if ((resHeaderStatus.Value == BTnGHeaderStatus.FAIL) ||
                        //    (resHeaderStatus.Value == BTnGHeaderStatus.SUCCESS) ||
                        //    (resHeaderStatus.Value == BTnGHeaderStatus.CANCEL)
                        //    )
                        //{
                        //    /*By-pass*/
                        //}

                        else 
                        {
                            Log.LogText(LogChannel, bTnGSaleTransNo, $@"Last HeaderStatus: {resHeaderStatus.Value}; Curr.Header Status : {headerStatus}; Curr.Detail Status : {detailStatus}; BTnG-{bTnGSaleTransNo}", "BTNG_B02", "CompleteTransactionElseReleaseSeatExecution.UpdateLocalTransactionDB");
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

                    Log.LogError(LogChannel, $@"BTnG-{bTnGSaleTransNo}", ex, "BTNG_EX01", "CompleteTransactionElseReleaseSeatExecution.UpdateLocalTransactionDB");
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
