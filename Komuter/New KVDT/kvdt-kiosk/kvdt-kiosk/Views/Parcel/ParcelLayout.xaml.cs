using kvdt_kiosk.Models;
using kvdt_kiosk.Reports;
using kvdt_kiosk.Reports.KTMB;
using kvdt_kiosk.Services;
using kvdt_kiosk.Views.Payment;
using kvdt_kiosk.Views.Payment.EWallet;
using kvdt_kiosk.Views.Payment.PayWave;
using kvdt_kiosk.Views.Printing;
using kvdt_kiosk.Views.Welcome;
using Microsoft.Reporting.WinForms;
using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;
using NssIT.Train.Kiosk.Common.Constants;
using NssIT.Train.Kiosk.Common.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Threading;

namespace kvdt_kiosk.Views.Parcel
{
    /// <summary>
    /// Interaction logic for ParcelLayout.xaml
    /// </summary>
    public partial class ParcelLayout
    {
        public bool IsReset { get; set; }

        public CardPayWaveexe cardPayWave;

        private Thread _printingThreadWorker;
        private Thread _endPaymentThreadWorker;
        private CreditCardResponse _lastCreditCardAnswer;
        public readonly PrintingTemplateScreen PrintingTemplateScreen;

        ResourceDictionary langRec = null;

        public APIServices _apiService;
        private pgKomuter_BTnGPaymentForParcel pgKomuter_BTnGPayment = null;
        private static string CreditCardReceiptReportSourceName => "RPTCreditCardReceipt";

        private AfcReceiptModel[] afcReceiptModels = new AfcReceiptModel[1];

        private static string TicketReportSourceName
        {
            get
            {
                return "RPTOnlineAFC3iNCH";
            }
        }

        private static string ReceiptReportSourceName
        {
            get
            {
                return "AFCTicketReceiptV";
            }
        }
        public ParcelLayout()
        {
            InitializeComponent();
            CurrentDate();
            CurrentTime();
            LoadParcel();
            LoadLanguage();


            cardPayWave = CardPayWaveexe.GetCreditCardPayWavePage();
            PrintingTemplateScreen = new PrintingTemplateScreen();
            PrintingTemplateScreen.OnPrintReceipt += PrintingTemplateScreen_OnPrintReceipt;
            pgKomuter_BTnGPayment = new pgKomuter_BTnGPaymentForParcel(this, FrmSubFrame, BdSubFrame, FrmPrint, PrintingTemplateScreen);
            _apiService = new APIServices(new APIURLServices(), new APISignatureServices());
        }

        private void PrintingTemplateScreen_OnPrintReceipt(object sender, EventArgs e)
        {
            PrintReceipt();
        }

        private void PrintReceipt()
        {
            Reports.RdlcRendering rpen = null;
            try
            {
                rpen = new Reports.RdlcRendering(App.ReportPDFFileMan.TicketFolderPath);

                if (afcReceiptModels[0] != null)
                {
                    LocalReport ticketRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\KTMB\KomuterTicket\{ReceiptReportSourceName}.rdlc",
                         new ReportDataSource[] { new ReportDataSource("DataSet1", new List<AfcReceiptModel>(afcReceiptModels)) });
                    ReportImageSize receiptSize = new ReportImageSize(3.2M, 8.2M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
                    Stream[] streamReceiptList = RdlcImageRendering.Export(ticketRep, receiptSize);


                    ImagePrintingTools.InitService();
                    if (streamReceiptList != null)
                        ImagePrintingTools.AddPrintDocument(streamReceiptList, "test", receiptSize);

                    if (ImagePrintingTools.ExecutePrinting("test") == false)
                    {
                        throw new Exception("Error: Printer Busy; EXP01");
                    }


                }
                UserSession.IsPrintingDone = true;
            }
            catch (Exception ex)
            {

            }

        }

        private void LoadLanguage()
        {
            App._parcelLayout = this;
            BtnConfirm.Content = App.Language == "en" ? "Confirm" : "Sahkan";
            BtnReset.Content = App.Language == "en" ? "Reset" : "Semula";
            BtnExit.Content = App.Language == "en" ? "Exit" : "Keluar";
        }

        public void timer_Tick(object sender, EventArgs e) => TxtTime.Text = DateTime.Now.ToString("HH:mm");

        private void CurrentDate()
        {
            TxtDate.Text = DateTime.Now.ToString("dd MMM yyyy");
            TxtDate.Text = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(TxtDate.Text);
        }

        public void CurrentTime()
        {
            TxtTime.Text = DateTime.Now.ToString("HH:mm");

            var timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 1);
            timer.Start();
        }

        private void LoadParcel()
        {
            GridParcel.Children.Clear();
            Dispatcher.BeginInvoke((Action)(() =>
            {
                APIServices aPIServices = new APIServices(new APIURLServices(), new APISignatureServices());

                var result = aPIServices.GetAFCAddOn(UserSession.AFCService).Result;

                if (result?.Data?.Count > 0)
                {

                    foreach (var item in result.Data)
                    {

                        Parcel parcel = new Parcel();

                        parcel.ParcelName.Text = item.AddOnName;
                        parcel.ParcelPrice.Text = "RM" + item.UnitPrice.ToString(CultureInfo.CurrentCulture);
                        parcel.ParcelId.Text = item.AddOnId;

                        GridParcel.Rows = result.Data.Count;

                        GridParcel.Height = GridParcel.Rows * 90;

                        GridParcel.Children.Add(parcel);
                    }
                }
            }));
        }

        //private async void BtnReset_Click(object sender, EventArgs e)
        //{
        //    UserSession.isParcelHaveClicked = false;
        //    LoadParcel();
        //}

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync(async () =>
            {
                UserSession.UserAddons = null;
                UserSession.isParcelHaveClicked = false;
                UserSession.IsParcelCheckOut = false;
                UserSession.TicketOrderTypes = null;
                UserSession.IsCheckOut = false;
                UserSession.TotalTicketPrice = 0;
                UserSession.UpdateAFCBookingResultModel = null;
                UserSession.CheckoutBookingResultModel = null;
                PassengerInfo.IcScanned.Clear();
                PassengerInfo.PassengerName = null;
                PassengerInfo.ICNumber = null;
                PassengerInfo.CurrentScanNumberForChild = 0;
                PassengerInfo.CurrentScanNumberForSenior = 0;
                PassengerInfo.IsPaxSelected = false;

                await Task.Delay(500);

                var parentWindow = Window.GetWindow(this);
                parentWindow.Content = new WelcomeScreen();

            });
        }

        private async void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (UserSession.UserAddons == null)
            {
                MessageBox.Show("Please select parcel first", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            Parcel parcel = new Parcel();

            foreach (var item in UserSession.UserAddons)
            {
                parcel.ParcelName.Text = item.AddOnName;
                parcel.ParcelPrice.Text = "RM" + item.AddOnPrice.ToString(CultureInfo.InvariantCulture);
                parcel.ParcelId.Text = item.AddOnId;
            }

            UserSession.IsParcelCheckOut = true;

            PaymentInfoScreen paymentInfoScreen = new PaymentInfoScreen();
            paymentInfoScreen.startPayWavePaymentbtn += PaymentInfoScreen_startPayWavePaymentbtn;
            paymentInfoScreen.StartBoostEwalletPaymentBtn += PaymentInfoScreen_StartBoostEwalletPaymentBtn;
            paymentInfoScreen.StartTngEwalletPaymentBtn += PaymentInfoScreen_StartTngEwalletPaymentBtn;

            GridPayment.Children.Clear();


            GridPayment.Children.Add(paymentInfoScreen);

            if (UserSession.IsParcelCheckOut)
            {
                BtnConfirm.IsEnabled = false;
                // BtnReset.IsEnabled = false;
            }

            //start booking for payment
            //var apiService = new APIServices(new APIURLServices(), new APISignatureServices());

            AFCBookingModel bookingModel = new AFCBookingModel();

            List<AFCBookingAddOnDetailModel> parcels = new List<AFCBookingAddOnDetailModel>();


            if (UserSession.IsParcelCheckOut)
            {
                bookingModel.MCounters_Id = UserSession.MCounters_Id;
                bookingModel.CounterUserId = null; // null
                bookingModel.HandheldUserId = null; //null
                bookingModel.AFCServiceHeaders_Id = UserSession.AFCService; //session
                bookingModel.Channel = "TVM";
                bookingModel.PurchaseStationId = UserSession.PurchaseStationId;
                bookingModel.PackageJourney = "1";
                bookingModel.From_MStations_Id = UserSession.FromStationId;
                bookingModel.To_MStations_Id = "18900";
                // bookingModel.To_MStations_Id = UserSession.FromStationId;

                if (UserSession.UserAddons != null)
                {
                    foreach (UserAddon userAddon in UserSession.UserAddons)
                    {
                        for (int i = 0; i < userAddon.AddOnCount; i++)
                        {
                            AFCBookingAddOnDetailModel aFCBookingAddOnDetailModel = new AFCBookingAddOnDetailModel();
                            aFCBookingAddOnDetailModel.AFCAddOns_Id = userAddon.AddOnId;
                            parcels.Add(aFCBookingAddOnDetailModel);
                        }
                    }

                    bookingModel.AFCBookingAddOnDetailModels = parcels;
                }
            }
            else
            {
                bookingModel.AFCBookingAddOnDetailModels = null;

            }

            var result = await _apiService.RequestAFCBooking(bookingModel);


            if (result.Error == YesNo.No)
            {
                UserSession.UpdateAFCBookingResultModel = result;
            }

        }

        private void PaymentInfoScreen_StartTngEwalletPaymentBtn(object sender, EventArgs e)
        {
            pgKomuter_BTnGPayment.StartBTnGPayment(UserSession.CurrencyCode,
              UserSession.TotalTicketPrice,
              UserSession.UpdateAFCBookingResultModel.BookingNo,
              "touchngo_offline",
              "afc-passenger",
              "afc-passenger",
              "1234567890",
              UserSession.UpdateAFCBookingResultModel.Booking_Id,
              "https://ktmbstorage.blob.core.windows.net/ktmb-tvm-live-file/TNG_20210723.jpg",
              "TngEwallet",
              langRec,
              LanguageCode.Malay);
        }

        private void PaymentInfoScreen_StartBoostEwalletPaymentBtn(object sender, EventArgs e)
        {
            pgKomuter_BTnGPayment.StartBTnGPayment(UserSession.CurrencyCode,
               0.1M,
               UserSession.UpdateAFCBookingResultModel.BookingNo,
               "boost",
               "afc-passenger",
               "afc-passenger",
               "1234567890",
               UserSession.UpdateAFCBookingResultModel.Booking_Id,
               "https://ktmbstorage.blob.core.windows.net/ktmb-tvm-live-file/Boost_20210723.jpg",
               "Boost",
               langRec,
               LanguageCode.English);
        }

        public void BTnGShowPaymentInfo(IKioskMsg kioskMsg)
        {
            pgKomuter_BTnGPayment.BTnGShowPaymentInfo(kioskMsg);
        }

        private void PaymentInfoScreen_startPayWavePaymentbtn(object sender, EventArgs e)
        {
            cardPayWave.ClearEvents();
            cardPayWave.InitPaymentData(UserSession.CurrencyCode, UserSession.TotalTicketPrice, UserSession.SessionId, App.PayWaveCOMPORT, null);

            cardPayWave.OnEndPayment += CardPayWave_OnEndPayment;
            FrmSubFrame.Content = null;
            FrmSubFrame.NavigationService.RemoveBackEntry();
            FrmSubFrame.NavigationService.Content = null;
            System.Windows.Forms.Application.DoEvents();


            FrmSubFrame.NavigationService.Navigate(cardPayWave);
            BdSubFrame.Visibility = Visibility.Visible;
            System.Windows.Forms.Application.DoEvents();
        }

        private void CardPayWave_OnEndPayment(object sender, EndOfPaymentEventArgs e)
        {
            try
            {
                if (App.SysParam.PrmNoPaymentNeed)
                {
                }

                if (_endPaymentThreadWorker != null)
                {
                    if ((_endPaymentThreadWorker.ThreadState & ThreadState.Stopped) != ThreadState.Stopped)
                    {
                        try
                        {
                            _endPaymentThreadWorker.Abort();
                            Thread.Sleep(300);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }

                _endPaymentThreadWorker = new Thread(OnEndPaymentThreadWorking);
                _endPaymentThreadWorker.IsBackground = true;
                _endPaymentThreadWorker.Start();
            }
            catch (Exception)
            {
                // ignored
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

                        this.Dispatcher.Invoke(() =>
                        {

                            cardPayWave.ClearEvents();
                            FrmSubFrame.Content = null;
                            FrmSubFrame.NavigationService.RemoveBackEntry();
                            FrmSubFrame.NavigationService.Content = null;
                            BdSubFrame.Visibility = Visibility.Collapsed;


                            System.Windows.Forms.Application.DoEvents();


                            //FrmSubFrame.NavigationService.Navigate(cardPayWave);
                            //BdSubFrame.Visibility = Visibility.Visible;


                            FrmPrint.Content = null;
                            FrmPrint.NavigationService.RemoveBackEntry();
                            FrmPrint.NavigationService.Navigate(PrintingTemplateScreen);
                            FrmPrint.Visibility = Visibility.Visible;
                            System.Windows.Forms.Application.DoEvents();

                            //var window = new Window
                            //{
                            //    Content = new PrintingTemplateScreen(),

                            //    WindowStyle = WindowStyle.None,
                            //   Height = 900,
                            //   Width = 1000,
                            //    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                            //    WindowState = WindowState.Normal,
                            //    Owner = Application.Current.MainWindow,
                            //    ResizeMode = ResizeMode.NoResize
                            //};

                            //window.Owner.Effect = new System.Windows.Media.Effects.BlurEffect();
                            //window.Owner.Opacity = 0.4;
                            //window.ShowDialog();
                        });


                        //call api to make payment
                        RequestPayment();

                    }

                    else if ((e.ResultState == PaymentResult.Cancel) || (e.ResultState == PaymentResult.Fail))
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            cardPayWave.ClearEvents();
                            FrmSubFrame.Content = null;
                            FrmSubFrame.NavigationService.RemoveBackEntry();
                            FrmSubFrame.NavigationService.Content = null;
                            BdSubFrame.Visibility = Visibility.Collapsed;

                        });
                    }
                    else
                    {
                        MessageBox.Show("Error ");
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception)
                {
                    // ignored
                }
                finally
                {
                    _endPaymentThreadWorker = null;
                }
            }


        }

        public async void RequestPayment()
        {
            try
            {
                if (UserSession.CheckoutBookingResultModel != null)
                {
                    if (UserSession.CheckoutBookingResultModel.Error.Equals(YesNo.Yes))
                    {
                        _printingThreadWorker = new Thread(PrintErrorThreadWorking);
                        _printingThreadWorker.IsBackground = true;
                        _printingThreadWorker.Start();
                    }
                    else
                    {
                        try
                        {
                            //var apiService = new APIServices(new APIURLServices(), new APISignatureServices());

                            var aFCPaymentModel = new AFCPaymentModel();
                            aFCPaymentModel.Booking_Id = UserSession.CheckoutBookingResultModel.Booking_Id;
                            aFCPaymentModel.MCurrencies_Id = UserSession.CheckoutBookingResultModel.MCurrencies_Id;
                            aFCPaymentModel.TotalAmount = UserSession.CheckoutBookingResultModel.PayableAmount;
                            aFCPaymentModel.MCounters_Id = UserSession.MCounters_Id;
                            aFCPaymentModel.Channel = "TVM";
                            aFCPaymentModel.CounterUserId = UserSession.CounterUserId;
                            aFCPaymentModel.HandheldUserId = UserSession.HandheldUserId;
                            aFCPaymentModel.PurchaseStationId = UserSession.PurchaseStationId;

                            var bookingPaymentDetailModel = new BookingPaymentDetailModel();
                            bookingPaymentDetailModel.FinancePaymentMethod = UserSession.FinancePaymentMethod;
                            bookingPaymentDetailModel.Amount = UserSession.CheckoutBookingResultModel.PayableAmount;

                            aFCPaymentModel.BookingPaymentDetailModels.Add(bookingPaymentDetailModel);

                            aFCPaymentModel.CreditCardResponseModel = _lastCreditCardAnswer;

                            aFCPaymentModel.PaymentGatewaySuccessPaidModel = null;

                            var result = await _apiService.RequestAFCPayment(aFCPaymentModel);

                            if (result.Status == false)
                            {
                                _printingThreadWorker = new Thread(PrintErrorThreadWorking);
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
                            // ignored
                        }
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void PrintTicketAndReceipt(AFCPaymentResultModel data)
        {
            _printingThreadWorker = new Thread(PrintThreadWorking);
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

                    if (paymentResponse.AFCTicketPrintList[0] != null)
                    {
                        AfcReceiptModel afcReceiptModel = new AfcReceiptModel()
                        {
                            CompanyAddress = paymentResponse.AFCTicketPrintList[0].CompanyAddress,
                            CompanyRegNo = paymentResponse.AFCTicketPrintList[0].CompanyRegNo,
                            TicketMessage = paymentResponse.AFCTicketPrintList[0].TicketMessage,
                            DepartureStationDescription = paymentResponse.AFCTicketPrintList[0].From_MStations_Id,
                            ArrivalStationDescription = paymentResponse.AFCTicketPrintList[0].To_MStations_Id,
                            TicketEffectiveFromDate = paymentResponse.AFCTicketPrintList[0].TicketEffectiveFromDate,
                            BookingNo = paymentResponse.AFCTicketPrintList[0].BookingNo,
                            NumberOfTicket = paymentResponse.AFCTicketPrintList[0].NumberOfTicket,
                            TicketId = string.Join(" ", paymentResponse.AFCTicketPrintList.Select(x => x.TicketNo)),
                            PaymentMethod = paymentResponse.AFCTicketPrintList[0].PaymentMethod,
                            TotalAmount = paymentResponse.AFCTicketPrintList[0].TotalAmount.ToString("F2"),
                            CreationID = paymentResponse.AFCTicketPrintList[0].CreationId,
                            CreationDate = paymentResponse.AFCTicketPrintList[0].CreationDateTime
                        };

                        afcReceiptModels[0] = afcReceiptModel;
                    }
                    bool isProceedToPrintTicket = false;
                    if ((paymentResponse?.AFCBookingPaymentResult?.Error?.Equals(YesNo.No) == true) && (paymentResponse.AFCTicketPrintList?.Length > 0))
                    {
                        if (FixTicketQRCodeData(paymentResponse.AFCTicketPrintList, out AFCTicketPrintModel[] resultTicketList))
                        {
                            paymentResponse.AFCTicketPrintList = resultTicketList;
                            isProceedToPrintTicket = true;
                        }
                    }

                    if (isProceedToPrintTicket)
                    {
                        PrintTicket(paymentResponse.AFCTicketPrintList);
                        UserSession.IsPrintingDone = true;
                    }


                }
                catch (Exception)
                {
                    // ignored
                }
            }


            bool FixTicketQRCodeData(AFCTicketPrintModel[] ticketList, out AFCTicketPrintModel[] resultTicketList)
            {
                resultTicketList = null;
                bool retVal;
                string errorNote = "";
                if (errorNote == null) throw new ArgumentNullException(nameof(errorNote));

                try
                {
                    foreach (AFCTicketPrintModel tk in ticketList)
                    {
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
                catch (Exception)
                {
                    retVal = false;

                    //App.Log.LogError(_logChannel, transactionNo, ex, "EX01", "pgKomuter.FixTicketQRCodeData",
                    //    adminMsg: errMsg);
                }
                return retVal;
            }
        }

        private void PrintTicket(IEnumerable<AFCTicketPrintModel> komuterPrintTickets)
        {
            DSCreditCardReceipt.DSCreditCardReceiptDataTable dtPaywave = null;
            try
            {
                //Log.Information()
                var rdlcRendering = new RdlcRendering(App.ReportPDFFileMan.TicketFolderPath);
                if (rdlcRendering == null) throw new ArgumentNullException(nameof(rdlcRendering));

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
                    new[] { new ReportDataSource("DataSet1", (DataTable)dtPaywave) });
                    streamReceiptList = RdlcImageRendering.Export(receiptRep, receiptSize);
                }

                LocalReport ticketRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\KTMB\KomuterTicket\{TicketReportSourceName}.rdlc",
                   new[] { new ReportDataSource("DataSet1", new List<AFCTicketPrintModel>(komuterPrintTickets)) });
                ReportImageSize ticketSize = new ReportImageSize(3.2M, 8.2M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
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
            catch (Exception)
            {
                //App.ShowDebugMsg("Error on pgKomuter.PrintTicket; EX01");
                //App.Log.LogError(_logChannel, _ktmbSalesTransactionNo, ex, "EX01", "pgKomuter.PrintTicket",
                //    adminMsg: $@"Error when printing; {ex.Message}");
            }
        }

        private static void PrintErrorThreadWorking()
        {
            //print ticket error;
        }

        private async void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            BtnReset.Content = "Please Wait...";
            BtnReset.IsEnabled = false;

            await Task.Delay(100);

            SystemConfig.IsResetIdleTimer = true;
            UserSession.TotalTicketPrice = 0;
            UserSession.UserAddons = null;
            UserSession.isParcelHaveClicked = false;
            UserSession.IsParcelCheckOut = false;
            LoadParcel();

            BtnConfirm.IsEnabled = true;

            GridPayment.Children.Clear();

            BtnReset.Content = "RESET";
            BtnReset.IsEnabled = true;
        }

        private void GridPurchaseTicket_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;
        }
    }
}