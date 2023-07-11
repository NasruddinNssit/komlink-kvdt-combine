using kvdt_kiosk.Models;
using kvdt_kiosk.Reports;
using kvdt_kiosk.Reports.KTMB;
using kvdt_kiosk.Services;
using kvdt_kiosk.Views.Payment.PayWave;
using Microsoft.Reporting.WinForms;
using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;
using NssIT.Train.Kiosk.Common.Constants;
using NssIT.Train.Kiosk.Common.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace kvdt_kiosk.Views.Payment
{
    /// <summary>
    /// Interaction logic for PaymentTicketBuyInfo.xaml
    /// </summary>
    public partial class PaymentTicketBuyInfo : UserControl
    {
        private Thread _printingThreadWorker = null;
        private Thread _endPaymentThreadWorker = null;
        private CreditCardResponse _lastCreditCardAnswer = null;
        public event EventHandler StartPayWavePayment;
        //public event EventHandler StartEWalletPayment;
        public event EventHandler StartTngEwalletPayment = null;
        public event EventHandler StartBoostEwalletPayment = null;

        decimal totalTicketPrice = 0;

        private string CreditCardReceiptReportSourceName
        {
            get
            {
                return "RPTCreditCardReceipt";
            }
        }

        private string TicketReportSourceName
        {
            get
            {
                return "RPTOnlineAFC3iNCH";
            }
        }
        public PaymentTicketBuyInfo()
        {
            InitializeComponent();
            LoadLanguage();
            LoadTicketInfo();
            TicketCheckOutInfoAsync();
            ParcelCheckOutInfoAsync();
            HiddenLabelOnly();
        }

        private void LoadLanguage()
        {
            if (App.Language == "ms")
            {
                lblAttention.Text = "Sila periksa semula maklumat anda sebelum membuat pembayaran";
                lblAttention.Text = lblAttention.Text.ToUpper();
                lblPayment.Text = "MAKLUMAT PEMBAYARAN";
                lblPayment.FontSize = 32;
                lblAttention.TextWrapping = System.Windows.TextWrapping.WrapWithOverflow;
            }
        }

        private void LoadTicketInfo()
        {
            var ticketDate = DateTime.Now.ToString("ddd, dd-MM");


            if (UserSession.ToStationName == null)
            {
                lblStationInfo.Text = "";
                lblDateTypeJourneyInfo.Text = ticketDate;
            }
            else
            {
                lblStationInfo.Text = UserSession.FromStationName.ToUpper() + " => " + UserSession.ToStationName.ToUpper();
                lblDateTypeJourneyInfo.Text = ticketDate + " [ " + UserSession.JourneyType.ToUpper() + " ] ";
            }
        }

        public async void TicketCheckOutInfoAsync()
        {
            while (!UserSession.IsCheckOut)
            {
                await Task.Delay(500);
            }

            foreach (var item in UserSession.TicketOrderTypes)
            {
                totalTicketPrice += item.TotalPrice;

                TextBlock txtText = new TextBlock();
                txtText.FontWeight = System.Windows.FontWeights.Bold;
                txtText.FontSize = 15;
                txtText.TextWrapping = System.Windows.TextWrapping.Wrap;
                txtText.Foreground = System.Windows.Media.Brushes.White;

                txtText.Text = item.TicketTypeName + " ( " + item.NoOfPax + " ) ";

                GridTicket.Rows = UserSession.TicketOrderTypes.Count;

                GridTicket.Children.Add(txtText);

            }

            UserSession.TotalTicketPrice = totalTicketPrice;

            lblTotalTicketPrice.Text = "RM " + UserSession.TotalTicketPrice.ToString("0.00");
        }

        public async void ParcelCheckOutInfoAsync()
        {
            while (!UserSession.IsParcelCheckOut)
            {
                await Task.Delay(500);
            }

            if (UserSession.UserAddons != null)
            {
                foreach (var parcel in UserSession.UserAddons)
                {
                    TextBlock txtParcel = new TextBlock();
                    txtParcel.FontWeight = System.Windows.FontWeights.Bold;
                    txtParcel.FontSize = 15;
                    txtParcel.TextWrapping = System.Windows.TextWrapping.Wrap;
                    txtParcel.Foreground = System.Windows.Media.Brushes.White;

                    txtParcel.Text = "** " + parcel.AddOnName + " ( " + parcel.AddOnCount + " ) ";

                    GridParcel.Rows = UserSession.UserAddons.Count;

                    GridParcel.Children.Add(txtParcel);


                    UserSession.TotalTicketPrice = UserSession.TotalTicketPrice + (parcel.AddOnPrice * parcel.AddOnCount);

                    lblTotalTicketPrice.Text = "RM " + UserSession.TotalTicketPrice.ToString("0.00");
                }
            }
        }

        private async void HiddenLabelOnly()
        {
            while (!UserSession.isParcelHaveClicked)
            {
                await Task.Delay(50);
            }
        }

        private void BtnCardWave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UserSession.FinancePaymentMethod = FinancePaymentMethod.CreditCard;
            CheckOutBooking();
            StartPayWavePayment.Invoke(null, null);
        }

        private async void RequestPayment()
        {
            try
            {
                if (UserSession.CheckoutBookingResultModel != null)
                {
                    if (UserSession.CheckoutBookingResultModel.Error.Equals(YesNo.Yes))
                    {
                        _printingThreadWorker = new Thread(new ThreadStart(PrintErrorThreadWorking));
                        _printingThreadWorker.IsBackground = true;
                        _printingThreadWorker.Start();
                    }
                    else
                    {
                        try
                        {
                            var apiService = new APIServices(new APIURLServices(), new APISignatureServices());

                            AFCPaymentModel aFCPaymentModel = new AFCPaymentModel();
                            aFCPaymentModel.Booking_Id = UserSession.CheckoutBookingResultModel.Booking_Id;
                            aFCPaymentModel.MCurrencies_Id = UserSession.CheckoutBookingResultModel.MCurrencies_Id;
                            aFCPaymentModel.TotalAmount = UserSession.CheckoutBookingResultModel.PayableAmount;
                            aFCPaymentModel.MCounters_Id = UserSession.MCounters_Id;
                            aFCPaymentModel.Channel = "TVM";
                            aFCPaymentModel.CounterUserId = UserSession.CounterUserId;
                            aFCPaymentModel.HandheldUserId = UserSession.HandheldUserId;
                            aFCPaymentModel.PurchaseStationId = UserSession.PurchaseStationId;



                            BookingPaymentDetailModel bookingPaymentDetailModel = new BookingPaymentDetailModel();
                            bookingPaymentDetailModel.FinancePaymentMethod = UserSession.FinancePaymentMethod;
                            bookingPaymentDetailModel.Amount = UserSession.CheckoutBookingResultModel.PayableAmount;

                            aFCPaymentModel.BookingPaymentDetailModels.Add(bookingPaymentDetailModel);

                            aFCPaymentModel.CreditCardResponseModel = _lastCreditCardAnswer;

                            aFCPaymentModel.PaymentGatewaySuccessPaidModel = null;

                            var result = await apiService.RequestAFCPayment(aFCPaymentModel);

                            if (result.Status == false)
                            {
                                _printingThreadWorker = new Thread(new ThreadStart(PrintErrorThreadWorking));
                                _printingThreadWorker.IsBackground = true;
                                _printingThreadWorker.Start();
                            }
                            else
                            {
                                PrintTicketAndReceipt(result);
                            }

                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void PrintTicketAndReceipt(AFCPaymentResultModel data)
        {
            _printingThreadWorker = new Thread(new ThreadStart(PrintThreadWorking));
            _printingThreadWorker.IsBackground = true;
            _printingThreadWorker.Start();

            void PrintThreadWorking()
            {
                try
                {
                    string json = JsonConvert.SerializeObject(data.Data, Formatting.Indented);

                    CompletePaymentBookingModel paymentResponse = JsonConvert.DeserializeObject<CompletePaymentBookingModel>(json);
                    //bool isProceedToPrintTicket = false;

                    //if(paymentResponse?.KomuterBookingPaymentResult?.Error?.Equals(YesNo.No) == true)

                    //App.Log.LogText(_logChannel, (payRes?.Data?.KomuterBookingPaymentResult?.BookingNo) ?? "-", paymentStatusAck, "A01", "pgKomuter.UpdateKomuterTicketPaymentStatus",
                    //   extraMsg: "Start - UpdateKomuterTicketPaymentStatus; MsgObj: UIKomuterCompletePaymentAck");


                    bool isProceedToPrintTicket = false;
                    if ((paymentResponse?.AFCBookingPaymentResult?.Error?.Equals(YesNo.No) == true) && (paymentResponse?.AFCTicketPrintList?.Length > 0))
                    {
                        if (FixTicketQRCodeData("", paymentResponse.AFCTicketPrintList, out AFCTicketPrintModel[] resultTicketList))
                        {
                            paymentResponse.AFCTicketPrintList = resultTicketList;
                            isProceedToPrintTicket = true;
                        }
                    }

                    if (isProceedToPrintTicket)
                    {
                        PrintTicket(paymentResponse.AFCTicketPrintList);

                    }


                }
                catch (Exception ex)
                {

                }
            }

            bool FixTicketQRCodeData(string transactionNo, AFCTicketPrintModel[] ticketList, out AFCTicketPrintModel[] resultTicketList)
            {
                resultTicketList = null;
                bool retVal = false;
                int ticketInx = 0;
                string errorNote = "";

                try
                {
                    foreach (AFCTicketPrintModel tk in ticketList)
                    {
                        ticketInx++;
                        errorNote = $@"Count Inx.: {ticketInx}; Ticket No.: {tk?.TicketNo?.ToString()}";

                        if (string.IsNullOrWhiteSpace(tk.QRLink))
                            throw new Exception($@"QR-Link is BLANK");

                        Uri myUri = new Uri(tk.QRLink);

                        //CYA-TEST
                        //Uri myUri = new Uri(@"https://ktmb-live-qrcode.azurewebsites.net/QRCodeHandler.ashx?e=M&q=Two&s=4&T=2,da9e3b29-608c-41fe-90f5-aebf00bca5d1,KT220642480569,KOBAH,BUKIT MERTAJAM,25 Jun 2022,25 Jun 2022");
                        //------------------------------------------

                        string paramT = HttpUtility.ParseQueryString(myUri.Query).Get("T");
                        tk.QRTicketData = QRGen.GetQRCodeData(paramT);
                    }
                    resultTicketList = ticketList;
                    retVal = true;
                }
                catch (Exception ex)
                {
                    retVal = false;
                    string errMsg = $@"Error when printing; {ex.Message}; Ref Note: {errorNote}";

                    //App.Log.LogError(_logChannel, transactionNo, ex, "EX01", "pgKomuter.FixTicketQRCodeData",
                    //    adminMsg: errMsg);
                }
                return retVal;
            }
        }

        private void PrintTicket(AFCTicketPrintModel[] komuterPrintTickets)
        {
            Reports.RdlcRendering rpen = null;
            DSCreditCardReceipt.DSCreditCardReceiptDataTable dtPaywave = null;
            try
            {
                //Log.Information()
                rpen = new Reports.RdlcRendering(App.ReportPDFFileMan.TicketFolderPath);

                if (_lastCreditCardAnswer != null)
                {
                    var creditCardAnswer = _lastCreditCardAnswer;
                    dtPaywave = new DSCreditCardReceipt.DSCreditCardReceiptDataTable();
                    DSCreditCardReceipt.DSCreditCardReceiptRow rw = dtPaywave.NewDSCreditCardReceiptRow();
                    rw.AID = creditCardAnswer.aid;
                    rw.Approval = creditCardAnswer.apvc;
                    rw.BatchNo = creditCardAnswer.bcno;
                    rw.CardHolder_Name = creditCardAnswer.cdnm;

                    string cdno = (creditCardAnswer.cdno ?? "#").Trim();
                    if (cdno.Length >= 4)
                        rw.CardNo = $@"**** **** **** {cdno.Substring((cdno.Length - 4))}";
                    else
                        rw.CardNo = creditCardAnswer.cdno;

                    rw.CardType = creditCardAnswer.cdty;
                    rw.ExpDate = "";
                    rw.HostNo = creditCardAnswer.hsno;
                    rw.Merchant_Id = creditCardAnswer.mid;
                    rw.RRN = creditCardAnswer.rrn;
                    rw.Status = creditCardAnswer.stcd;
                    rw.TC = creditCardAnswer.trcy;
                    rw.Terminal_Id = creditCardAnswer.tid;
                    rw.TransactionType = "SALE";
                    rw.TransactionDate = creditCardAnswer.trdt.ToString("dd MMM yyyy hh:mm:ss tt");
                    rw.TransactionTrace = creditCardAnswer.ttce;
                    rw.AmountString = "MYR" + UserSession.TotalTicketPrice;
                    //rw.MachineId = uiCompltResult.MachineId;
                    //rw.RefNumber = _ktmbSalesTransactionNo;
                    dtPaywave.Rows.Add(rw);
                    dtPaywave.AcceptChanges();
                }

                //App.Log.LogText(_logChannel, _ktmbSalesTransactionNo ?? "#?#", "Debug:Print01-Start Render Data", "DBG05", classNMethodName: "pgKomuter.PrintTicket", messageType: AppDecorator.Log.MessageType.Debug);


                ReportImageSize receiptSize = new ReportImageSize(3.2M, 8.2M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
                Stream[] streamReceiptList = null;
                if (dtPaywave != null)
                {
                    LocalReport receiptRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\KTMB\{CreditCardReceiptReportSourceName}.rdlc",
                    new ReportDataSource[] { new ReportDataSource("DataSet1", (DataTable)dtPaywave) });
                    streamReceiptList = RdlcImageRendering.Export(receiptRep, receiptSize);
                }

                LocalReport ticketRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\KTMB\KomuterTicket\{TicketReportSourceName}.rdlc",
                   new ReportDataSource[] { new ReportDataSource("DataSet1", new List<AFCTicketPrintModel>(komuterPrintTickets)) });
                ReportImageSize ticketSize = new ReportImageSize(3M, 3M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
                Stream[] streamticketList = RdlcImageRendering.Export(ticketRep, ticketSize);

                //App.Log.LogText(_logChannel, _ktmbSalesTransactionNo ?? "#?#", "Debug:Print01-End Render Data", "DBG10", classNMethodName: "pgKomuter.PrintTicket", messageType: AppDecorator.Log.MessageType.Debug);
                ImagePrintingTools.InitService();
                ImagePrintingTools.AddPrintDocument(streamticketList, "test", ticketSize);
                if (streamReceiptList != null)
                    ImagePrintingTools.AddPrintDocument(streamReceiptList, "test", receiptSize);
                //App.Log.LogText(_logChannel, _ktmbSalesTransactionNo, "Start to print ticket", "A02", classNMethodName: "pgKomuter.PrintTicket",
                //   adminMsg: "Start to print ticket");

                if (ImagePrintingTools.ExecutePrinting("test") == false)
                {
                    throw new Exception("Error: Printer Busy; EXP01");
                }
            }
            catch (Exception ex)
            {
                //App.ShowDebugMsg("Error on pgKomuter.PrintTicket; EX01");
                //App.Log.LogError(_logChannel, _ktmbSalesTransactionNo, ex, "EX01", "pgKomuter.PrintTicket",
                //    adminMsg: $@"Error when printing; {ex.Message}");
            }
        }

        private async void CheckOutBooking()
        {

            try
            {
                if (Models.UserSession.UpdateAFCBookingResultModel != null)
                {

                    if (UserSession.UpdateAFCBookingResultModel.Error.Equals(YesNo.Yes))
                    {
                        _printingThreadWorker = new Thread(new ThreadStart(PrintErrorThreadWorking));
                        _printingThreadWorker.IsBackground = true;
                        _printingThreadWorker.Start();
                    }
                    else
                    {
                        try
                        {
                            var apiService = new APIServices(new APIURLServices(), new APISignatureServices());

                            AFCCheckOutModel aFCCheckOutModel = new AFCCheckOutModel();
                            aFCCheckOutModel.Booking_Id = UserSession.UpdateAFCBookingResultModel.Booking_Id;
                            aFCCheckOutModel.PNR = "";
                            aFCCheckOutModel.PassengerICNo = "";
                            aFCCheckOutModel.PassengerPassportNo = "";
                            aFCCheckOutModel.MCounters_Id = UserSession.MCounters_Id;
                            aFCCheckOutModel.CounterUserId = UserSession.CounterUserId;
                            aFCCheckOutModel.HandheldUserId = UserSession.HandheldUserId;
                            aFCCheckOutModel.PurchaseStationId = UserSession.PurchaseStationId;
                            aFCCheckOutModel.Channel = "TVM";

                            IList<PaymentOptionModel> paymentModels = new List<PaymentOptionModel>();

                            PaymentOptionModel paymentOptionModel = new PaymentOptionModel();
                            paymentOptionModel.FinancePaymentMethod = UserSession.FinancePaymentMethod;
                            paymentOptionModel.Amount = UserSession.TotalTicketPrice;

                            paymentModels.Add(paymentOptionModel);

                            aFCCheckOutModel.PaymentOptionModels = paymentModels;

                            var result = await apiService.RequestAFCCheckoutBooking(aFCCheckOutModel);

                            if (result.Error == YesNo.No)
                            {
                                UserSession.CheckoutBookingResultModel = result;
                            }

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Please make booking first");
                        }
                    }


                }
                else
                {
                    throw new Exception();
                }


            }
            catch (Exception ex)
            {

            }



        }

        private void CardPayWave_OnEndPayment(object sender, EndOfPaymentEventArgs e)
        {
            string bankRefNo = "";

            try
            {
                bankRefNo = e.BankReferenceNo;

                if (App.SysParam.PrmNoPaymentNeed)
                    bankRefNo = DateTime.Now.ToString("MMddHHmmss");
                if (_endPaymentThreadWorker != null)
                {
                    if ((_endPaymentThreadWorker.ThreadState & ThreadState.Stopped) != ThreadState.Stopped)
                    {
                        try
                        {
                            _endPaymentThreadWorker.Abort();
                            Thread.Sleep(300);
                        }
                        catch { }
                    }
                }

                _endPaymentThreadWorker = new Thread(new ThreadStart(OnEndPaymentThreadWorking));
                _endPaymentThreadWorker.IsBackground = true;
                _endPaymentThreadWorker.Start();
            }
            catch (Exception ex)
            {

            }

            return;

            void OnEndPaymentThreadWorking()
            {
                try
                {
                    if (e.ResultState == PaymentResult.Success)
                    {
                        ResponseInfo respInfo = e.CardResponseResult;

                        if (App.SysParam.PrmNoPaymentNeed)
                        {
                            _lastCreditCardAnswer = new CreditCardResponse()
                            {
                                adat = $@"TestDoc{DateTime.Now.ToString("HHmmss")}",
                                aid = "Test_AID",
                                apvc = $@"{DateTime.Now.ToString("ffffff")}",
                                bcam = 0.0M,
                                bcno = $@"{DateTime.Now.ToString("ffffff")}",
                                btct = "00001",
                                camt = UserSession.TotalTicketPrice,
                                cdnm = "",
                                cdno = "1234567812345678",
                                cdty = "01",
                                erms = "",
                                hsno = "01",
                                mcid = "",
                                mid = $@"{DateTime.Now.ToString("HHmmssff")}",
                                rmsg = "SALE",
                                rrn = $@"{DateTime.Now.ToString("HHmmss")}",
                                stcd = "01",
                                tid = "1234567",
                                trcy = $@"{DateTime.Now.ToString("HHffffff")}",
                                trdt = DateTime.Now,
                                ttce = $@"{DateTime.Now.ToString("ffffff")}",
                            };
                        }
                        else
                        {
                            _lastCreditCardAnswer = new CreditCardResponse()
                            {
                                adat = respInfo.AdditionalData,
                                aid = respInfo.AID,
                                apvc = respInfo.ApprovalCode,
                                bcam = respInfo.BatchCurrencyAmount,
                                bcno = respInfo.BatchNumber,
                                btct = respInfo.BatchCount,
                                camt = respInfo.CurrencyAmount,
                                cdnm = respInfo.CardholderName,
                                cdno = respInfo.CardNo,
                                cdty = respInfo.CardType,
                                erms = respInfo.ErrorMsg,
                                hsno = respInfo.HostNo,
                                mcid = respInfo.MachineId,
                                mid = respInfo.MID,
                                rmsg = respInfo.ResponseMsg,
                                rrn = respInfo.RRN,
                                stcd = respInfo.StatusCode,
                                tid = respInfo.TID,
                                trcy = respInfo.TC,
                                trdt = DateTime.Now,
                                ttce = respInfo.TransactionTrace
                            };
                        }

                        //complete transaction then print ticket
                        //this.Dispatcher.Invoke(new Action(() =>
                        //{

                        //}));


                        //call api to make payment
                        RequestPayment();

                    }

                    else if ((e.ResultState == PaymentResult.Cancel) || (e.ResultState == PaymentResult.Fail))
                    {
                        MessageBox.Show("Error cancel");
                    }
                    else
                    {
                        MessageBox.Show("Error ");
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                }
                finally
                {
                    _endPaymentThreadWorker = null;
                }
            }


        }

        private void PrintErrorThreadWorking()
        {
            //print ticket error;
        }

        private void BtnTNG_Click(object sender, RoutedEventArgs e)
        {
            UserSession.FinancePaymentMethod = FinancePaymentMethod.TngEWallet;
            // MessageBox.Show("This Feature Still Under Implementation", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            CheckOutBooking();
            StartTngEwalletPayment(null, null);
        }

        private void BtnBoost_Click(object sender, RoutedEventArgs e)
        {
            UserSession.FinancePaymentMethod = FinancePaymentMethod.Boost;
            CheckOutBooking();
            StartBoostEwalletPayment(null, null);
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //lblProcessing.Visibility = Visibility.Visible;

            //await Task.Delay(3000);
            //lblProcessing.Visibility = Visibility.Collapsed;
            //GridPayment.Visibility = Visibility.Visible;

            TextAnimation();

            await Task.Delay(1500);

            ProcessingTextBlock.Visibility = Visibility.Hidden;

            CurrentGrid.Visibility = Visibility.Visible;

        }

        private void TextAnimation()
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = 1;

            doubleAnimation.To = 0.1;
            doubleAnimation.AutoReverse = true;
            doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));
            ProcessingTextBlock.BeginAnimation(TextBlock.OpacityProperty, doubleAnimation);
        }
    }
}
