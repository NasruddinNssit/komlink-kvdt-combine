using kvdt_kiosk.Models;
using kvdt_kiosk.Reports;
using kvdt_kiosk.Reports.KTMB;
using kvdt_kiosk.Services;
using kvdt_kiosk.Views.Parcel;
using kvdt_kiosk.Views.Passenger;
using kvdt_kiosk.Views.Payment;
using kvdt_kiosk.Views.Payment.EWallet;
using kvdt_kiosk.Views.Payment.PayWave;
using kvdt_kiosk.Views.Printing;
using kvdt_kiosk.Views.SeatingScreen;
using kvdt_kiosk.Views.Welcome;
using LazyCache;
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Windows.Media.ColorConverter;

namespace kvdt_kiosk.Views.PurchaseTicket
{
    /// <summary>
    /// Interaction logic for PurchaseTicketScreen.xaml
    /// </summary>
    /// 

    public partial class PurchaseTicketScreen
    {
        private readonly DestinationScreen _destinationScreen = new DestinationScreen();
        private AllRouteButton _allRouteButton = new AllRouteButton();
        private StationRouteButton _stationRouteButton = new StationRouteButton();
        private JourneyTypeButton _journeyTypeButton = new JourneyTypeButton();
        private PgKomuterPax _pgKomuterPax = new PgKomuterPax();
        private readonly DispatcherTimer _checkOutTimer = new DispatcherTimer();

        public bool IsAllSelected;

        public List<string> RoutesId = new List<string>();

        public List<AFCStationDetails> ListOfAllStations = new List<AFCStationDetails>();

        public IList<AFCRouteModel> ListOfAfcRoutes = new List<AFCRouteModel>();

        public IList<Packages> ListOfAfcPackages = new List<Packages>();

        public CardPayWaveexe CardPayWave = null;
        private readonly pgKomuter_BTnGPayment pgKomuter_BTnGPayment = null;

        private Thread _printingThreadWorker = null;
        private Thread _endPaymentThreadWorker = null;
        private CreditCardResponse _lastCreditCardAnswer = null;

        private AfcReceiptModel[] afcReceiptModels = new AfcReceiptModel[1];
        private readonly PrintingTemplateScreen _printingTemplateScreen = null;

        private APIServices _services;
        private static string CreditCardReceiptReportSourceName
        {
            get
            {
                return "RPTCreditCardReceipt";
            }
        }
        ResourceDictionary langRec = null;
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

        public PurchaseTicketScreen()
        {
            InitializeComponent();

            CurrentTime();
            CurrentDate();

            GetAfcRoutesAsync();
            GetAllStations();

            GridJourney.Children.Add(_destinationScreen);

            LoadLanguage();

            CheckIfUserIsCheckOutForPayment();

            DisableKeyboardUponCheckOut();
            CardPayWave = CardPayWaveexe.GetCreditCardPayWavePage();

            _printingTemplateScreen = new PrintingTemplateScreen();
            _printingTemplateScreen.OnPrintReceipt += _printingTemplateScreen_OnPrintReceipt;
            pgKomuter_BTnGPayment = new pgKomuter_BTnGPayment(this, FrmSubFrame, BdSubFrame, FrmPrint, _printingTemplateScreen);

            _services = new APIServices(new APIURLServices(), new APISignatureServices());
            //new APIServices(new APIURLServices(), new APISignatureServices());
        }

        private void _printingTemplateScreen_OnPrintReceipt(object sender, EventArgs e)
        {
            PrintReceipt();
        }

        //private void CardPayWave_OnEndPayment(object sender, EndOfPaymentEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        private void CardPayWave_OnEndPayment(object sender, EndOfPaymentEventArgs e)
        {
            var bankRefNo = "";

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

                _endPaymentThreadWorker = new Thread(new ThreadStart(OnEndPaymentThreadWorking))
                {
                    IsBackground = true
                };
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
                    if (e.ResultState != PaymentResult.Success)
                    {
                        if ((e.ResultState == PaymentResult.Cancel) || (e.ResultState == PaymentResult.Fail))
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                CardPayWave.ClearEvents();
                                FrmSubFrame.Content = null;
                                FrmSubFrame.NavigationService.RemoveBackEntry();
                                FrmSubFrame.NavigationService.Content = null;
                                BdSubFrame.Visibility = Visibility.Collapsed;
                            }));
                        }
                        else
                        {
                            MessageBox.Show("Error ");
                        }
                    }
                    else
                    {
                        ResponseInfo respInfo = e.CardResponseResult;

                        if (App.SysParam.PrmNoPaymentNeed)
                        {
                            _lastCreditCardAnswer = new CreditCardResponse()
                            {
                                adat = $@"TestDoc{DateTime.Now:HHmmss}",
                                aid = "Test_AID",
                                apvc = $@"{DateTime.Now:ffffff}",
                                bcam = 0.0M,
                                bcno = $@"{DateTime.Now:ffffff}",
                                btct = "00001",
                                camt = UserSession.TotalTicketPrice,
                                cdnm = "",
                                cdno = "1234567812345678",
                                cdty = "01",
                                erms = "",
                                hsno = "01",
                                mcid = "",
                                mid = $@"{DateTime.Now:HHmmssff}",
                                rmsg = "SALE",
                                rrn = $@"{DateTime.Now:HHmmss}",
                                stcd = "01",
                                tid = "1234567",
                                trcy = $@"{DateTime.Now:HHffffff}",
                                trdt = DateTime.Now,
                                ttce = $@"{DateTime.Now:ffffff}",
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

                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            CardPayWave.ClearEvents();
                            FrmSubFrame.Content = null;
                            FrmSubFrame.NavigationService.RemoveBackEntry();
                            FrmSubFrame.NavigationService.Content = null;
                            BdSubFrame.Visibility = Visibility.Collapsed;


                            System.Windows.Forms.Application.DoEvents();


                            //FrmSubFrame.NavigationService.Navigate(cardPayWave);
                            //BdSubFrame.Visibility = Visibility.Visible;


                            _printingTemplateScreen.UpdatePaymentComplete(UserSession.UpdateAFCBookingResultModel.BookingNo);

                            FrmPrint.Content = null;
                            FrmPrint.NavigationService.RemoveBackEntry();

                            FrmPrint.NavigationService.Navigate(_printingTemplateScreen);
                            FrmPrint.Visibility = Visibility.Visible;
                            System.Windows.Forms.Application.DoEvents();
                        }));


                        //call api to make payment
                        RequestPayment();
                    }
                }
                catch (ThreadAbortException) { }
                catch
                {
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
                        _printingThreadWorker = new Thread(new ThreadStart(PrintErrorThreadWorking));

                        _printingThreadWorker.IsBackground = true;
                        _printingThreadWorker.Start();
                    }
                    else
                    {
                        try
                        {
                            //var apiService = new APIServices(new APIURLServices(), new APISignatureServices());

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

                            var result = await _services.RequestAFCPayment(aFCPaymentModel);

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
                            _printingThreadWorker = new Thread(new ThreadStart(PrintErrorThreadWorking));

                            _printingThreadWorker.IsBackground = true;
                            _printingThreadWorker.Start();
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
                var retVal = false;
                var ticketInx = 0;
                var errorNote = "";

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

                UserSession.IsPrintingDone = true;
                return retVal;
            }
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

                    var cdno = (creditCardAnswer.cdno ?? "#").Trim();
                    rw.CardNo = cdno.Length >= 4 ? $@"**** **** **** {cdno.Substring((cdno.Length - 4))}" : creditCardAnswer.cdno;

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

                UserSession.IsPrintingDone = true;
            }
            catch (Exception ex)
            {
                //App.ShowDebugMsg("Error on pgKomuter.PrintTicket; EX01");
                //App.Log.LogError(_logChannel, _ktmbSalesTransactionNo, ex, "EX01", "pgKomuter.PrintTicket",
                //    adminMsg: $@"Error when printing; {ex.Message}");
            }
        }

        private void PrintErrorThreadWorking()
        {
            //print ticket error;
        }

        public void timer_Tick(object sender, EventArgs e)

        {
            TxtTime.Text = DateTime.Now.ToString("HH:mm");
        }

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

        private void LoadLanguage()
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (App.Language == "ms")
                {
                    lblJourneyType.Text = "JENIS PERJALANAN";
                    lblJourneyType.FontSize = 35;
                    BtnReset.Content = "SEMULA";

                    BtnExit.Content = "KELUAR";
                    BtnViewMap.Content = "PAPAR PETA";

                    lblKeyboardDestination.Text = "DESTINASI";
                }
            });
        }

        private async void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            BtnReset.Content = "Please Wait...";
            BtnReset.IsEnabled = false;

            await Task.Delay(50);

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

            _checkOutTimer.Start();

            var parentWindow = Window.GetWindow(this);
            parentWindow.Content = null;
            parentWindow.Content = new PurchaseTicketScreen();

            //PurchaseTicketScreen purchaseTicketScreen = new PurchaseTicketScreen ();
            //parentWindow.Content = purchaseTicketScreen;
            //App._purchaseTicketScreen = purchaseTicketScreen;

            //GridStation.Children.Clear();
            //GridPayment.Children.Clear();

            GetAfcRoutesAsync();
            GetAllStations();

            BtnReset.IsEnabled = true;
            BtnReset.Content = "RESET";

            SystemConfig.IsResetIdleTimer = true;
        }

        private async void BtnExit_Click(object sender, RoutedEventArgs e)
        {

            //IAppCache cache = new CachingService();
            //cache.Remove("AFCRoutesCache");
            //cache.Remove("AFCStationsCache");
            //cache.Remove("AFCPackagesCache");
            //cache.Remove("UserSession.ToStationId");
            //cache.Remove("colorCache");

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
        }

        public void GetAfcRoutesAsync()
        {
            IAppCache afcRoutesCache = new CachingService();
            ListOfAfcRoutes = afcRoutesCache.GetAsync<IList<AFCRouteModel>>("AFCRoutesCache").Result;

            try
            {
                if (ListOfAfcRoutes.Count > 0)
                {
                    GridRoutesModel.Children.Clear();

                    foreach (var item in ListOfAfcRoutes)
                    {
                        if (item.IsInterchange == "1")
                        {
                            _allRouteButton = new AllRouteButton
                            {
                                Margin = new Thickness(5, 3, 5, 3),
                                Padding = new Thickness(5, 3, 5, 3),
                                lblAll =
                                {
                                    Text = item.Description
                                }
                            };

                            GridRoutesModel.Children.Add(_allRouteButton);
                        }
                        else
                        {
                            _stationRouteButton = new StationRouteButton
                            {
                                Margin = new Thickness(5, 3, 5, 3),
                                Padding = new Thickness(5, 3, 5, 3),

                                TxtDesription =
                                {
                                    Text = item.Description
                                },
                                TxtId =
                                {
                                    Text = item.Id
                                },
                                BorderColor =
                                {
                                    Background =
                                        new SolidColorBrush((Color)ConvertFromString(item.ColorCode))
                                }

                            };

                            GridRoutesModel.Children.Add(_stationRouteButton);

                            _stationRouteButton.BtnRoute.Click += (s, e) =>
                            {
                                IsAllSelected = false;
                                foreach (var route in ListOfAfcRoutes)
                                {
                                    route.IsButtonClicked = route.Id == item.Id;

                                    var itemTest = _stationRouteButton.Content;
                                    if (route.IsInterchange != "1")
                                    {

                                        //_stationRouteButton.BtnRoute.Style = (Style)FindResource("BtnSelected");

                                        foreach (var routeItem in ListOfAfcRoutes)
                                        {
                                            if (routeItem.IsButtonClicked)
                                            {
                                                //_allRouteButton.BtnAll.Style = (Style)FindResource("BtnDefaultAll");

                                                foreach (var itemRoute in GridRoutesModel.Children)
                                                {
                                                    if (itemRoute is StationRouteButton stationRouteButton)
                                                    {
                                                        if (stationRouteButton.TxtId.Text == routeItem.Id)
                                                        {
                                                            stationRouteButton.BtnRoute.Style =
                                                                (Style)FindResource("BtnSelected");
                                                        }
                                                        else
                                                        {
                                                            stationRouteButton.BtnRoute.Style =
                                                                (Style)FindResource("BtnDefaultAll");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _stationRouteButton.BtnRoute.Style = (Style)FindResource("BtnDefaultAll");
                                    }
                                }

                                RoutesId = item.Id.Split(',').ToList();

                                _destinationScreen.lblDestination.TextWrapping = TextWrapping.WrapWithOverflow;
                                _destinationScreen.lblToStation.Text = "";

                                GetStations(item.Id);
                            };

                        }
                    }
                }

                _allRouteButton.BtnAll.Click += (s, e) =>
                {
                    IsAllSelected = true;
                    GetAllStations();

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _allRouteButton.BtnAll.Style = (Style)FindResource("BtnSelected");

                        foreach (var item in GridRoutesModel.Children)
                        {
                            if (item is StationRouteButton stationRouteButton)
                            {
                                stationRouteButton.BtnRoute.Style = (Style)FindResource("BtnDefaultAll");
                            }
                        }
                    }));
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void GetAllStations()
        {
            IsAllSelected = true;
            TxtTextBox.Text = "";
            _destinationScreen.lblToStation.Text = "[SELECT DESTINATION]";
            GridJourneyButton.Visibility = Visibility.Hidden;

            if (App.Language == "ms")
            {
                _destinationScreen.lblToStation.Text = "[PILIH DESTINASI]";
            }

            IAppCache cache = new CachingService();
            ListOfAllStations = cache.GetAsync<List<AFCStationDetails>>("AFCStationsCache").Result;

            if (ListOfAllStations == null) return;
            GridStation.Children.Clear();

            foreach (var item in ListOfAfcRoutes)
            {
                if (item.IsInterchange != "1") continue;
                foreach (var station in ListOfAllStations)
                {
                    var genericStation = new GenericStationButton()
                    {
                        LblStationName =
                        {
                            Text = station.Station
                        },
                        LblStationId =
                        {
                            Text = station.Id
                        },
                        StationColorCode =
                        {
                            Background =
                                new SolidColorBrush((Color)ConvertFromString(station.ColorCode))
                        },
                        Margin = new Thickness(5, 3, 5, 3),
                        Padding = new Thickness(5, 3, 5, 3),
                        Height = 60,
                    };


                    GridStation.Children.Add(genericStation);

                    genericStation.BtnGenericStation.MouseLeftButtonDown += (e, s) =>
                    {
                        _destinationScreen.lblToStation.Text = genericStation.LblStationName.Text;

                        UserSession.ToStationName = genericStation.LblStationName.Text;

                        foreach (var btnStation in GridStation.Children)
                        {
                            if (!(btnStation is GenericStationButton btn)) continue;
                            var btnBg = btn.BtnGenericStation.Background;
                            if (btnBg?.ToString() == "#FFFBD012")
                            {
                                btn.BtnGenericStation.Background =
                                    new SolidColorBrush((Color)ConvertFromString("#FFFFFFFF"));
                            }
                        }

                        UserSession.ToStationId = station.Id;

                        genericStation.BtnGenericStation.Background =
                            new SolidColorBrush((Color)ConvertFromString("#FFFBD012"));

                        GetAfcPackage();

                        GridJourneyButton.Visibility = Visibility.Visible;

                    };
                }
            }
        }

        private void GetStations(string routeId)
        {
            _destinationScreen.lblToStation.Text = "[SELECT DESTINATION]";
            if (App.Language == "ms")
            {
                _destinationScreen.lblToStation.Text = "[PILIH DESTINASI]";
            }

            GridJourneyButton.Visibility = Visibility.Hidden;

            IAppCache cache = new CachingService();
            ListOfAllStations = cache.GetAsync<List<AFCStationDetails>>("AFCStationsCache").Result;

            if (ListOfAllStations == null) return;
            GridStation.Children.Clear();
            TxtTextBox.Text = string.Empty;

            foreach (var station in ListOfAllStations)
            {
                if (!station.RouteId[0].Equals(routeId) && !station.IsInterchange) continue;
                var genericStation = new GenericStationButton()
                {
                    LblStationName =
                    {
                        Text = station.Station
                    },
                    LblStationId =
                    {
                        Text = station.Id
                    },
                    StationColorCode =
                    {
                        Background = new SolidColorBrush((Color)ConvertFromString(station.ColorCode))
                    },
                    Margin = new Thickness(5, 3, 5, 3),
                    Padding = new Thickness(5, 3, 5, 3),
                    Height = 60,
                };

                IAppCache routeCache = new CachingService();
                routeCache.Add("RouteIdCache", routeId);

                GridStation.Children.Add(genericStation);

                genericStation.BtnGenericStation.MouseLeftButtonDown += (e, s) =>
                {
                    _destinationScreen.lblToStation.Text = genericStation.LblStationName.Text;

                    UserSession.ToStationName = genericStation.LblStationName.Text;

                    foreach (var btnStation in GridStation.Children)
                    {
                        if (!(btnStation is GenericStationButton btn)) continue;
                        var btnBg = btn.BtnGenericStation.Background;
                        if (btnBg?.ToString() == "#FFFBD012")
                        {
                            btn.BtnGenericStation.Background =
                                new SolidColorBrush((Color)ConvertFromString("#FFFFFFFF"));
                        }
                    }

                    UserSession.ToStationId = station.Id;

                    genericStation.BtnGenericStation.Background =
                        new SolidColorBrush((Color)ConvertFromString("#FFFBD012"));

                    GetAfcPackage();

                    GridJourneyButton.Visibility = Visibility.Visible;
                };


                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _allRouteButton.BtnAll.Style = (Style)FindResource("BtnDefaultAll");
                }));
            }
        }

        private void CheckIfUserIsCheckOutForPayment()
        {

            _checkOutTimer.Interval = TimeSpan.FromMilliseconds(350);

            _checkOutTimer.Tick += async (sender, args) =>
            {
                if (UserSession.IsCheckOut)
                {
                    _checkOutTimer.Stop();
                    TxtDate.Text = DateTime.Now.ToString("dd MMM yyyy");
                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        GridJourneyButton1.Children.Clear();
                        GridJourneyButton1.Rows = 0;

                        JourneyTypeButton journeyTypeButton = new JourneyTypeButton
                        {
                            lblJourney =
                            {
                                Text = UserSession.JourneyType
                            },
                            lblJourneyDate =
                            {
                                Text = DateTime.Now.ToString("ddd, dd-MM")

                            },
                            IsEnabled = false,
                            BtnJourney =
                            {
                                Style = (Style)FindResource("BtnSelectedJourney")
                            }
                        };

                        PaymentInfoScreen paymentInfoScreen = new PaymentInfoScreen();
                        paymentInfoScreen.startPayWavePaymentbtn += StartPayWavePayment;
                        paymentInfoScreen.StartBoostEwalletPaymentBtn += StartBoostEwalletPayment;
                        paymentInfoScreen.StartTngEwalletPaymentBtn += StartTngEwalletPayment;

                        GridPayment.Children.Clear();

                        GridPayment.Children.Add(paymentInfoScreen);

                        GridJourneyButton1.Children.Add(journeyTypeButton);
                    }));
                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        GridJourneyButton1.Children.Clear();
                        _checkOutTimer.Stop();
                        GridJourneyButton1.Rows = 0;

                        JourneyTypeButton journeyTypeButton = new JourneyTypeButton
                        {
                            lblJourney =
                            {
                                Text = UserSession.JourneyType
                            },
                            lblJourneyDate =
                            {
                                Text = DateTime.Now.ToString("ddd, dd-MM")
                            },
                            IsEnabled = false,
                            BtnJourney =
                            {
                                Style = (Style)FindResource("BtnSelectedJourney")
                            }
                        };

                        PaymentInfoScreen paymentInfoScreen = new PaymentInfoScreen();
                        paymentInfoScreen.startPayWavePaymentbtn += StartPayWavePayment;
                        paymentInfoScreen.StartBoostEwalletPaymentBtn += StartBoostEwalletPayment;
                        paymentInfoScreen.StartTngEwalletPaymentBtn += StartTngEwalletPayment;

                        GridPayment.Children.Clear();

                        GridPayment.Children.Add(paymentInfoScreen);
                        GridJourneyButton1.Children.Add(journeyTypeButton);
                    }));
                }
            };

            _checkOutTimer.Start();
        }

        private void StartTngEwalletPayment(object sender, EventArgs e)
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

        public void BTnGShowPaymentInfo(IKioskMsg kioskMsg)
        {
            pgKomuter_BTnGPayment.BTnGShowPaymentInfo(kioskMsg);
        }

        private void StartPayWavePayment(object sender, EventArgs e)
        {

            CardPayWave.ClearEvents();
            CardPayWave.InitPaymentData(UserSession.CurrencyCode, UserSession.TotalTicketPrice, UserSession.SessionId, App.PayWaveCOMPORT, null);

            CardPayWave.OnEndPayment += CardPayWave_OnEndPayment;
            FrmSubFrame.Content = null;
            FrmSubFrame.NavigationService.RemoveBackEntry();
            FrmSubFrame.NavigationService.Content = null;
            System.Windows.Forms.Application.DoEvents();

            FrmSubFrame.NavigationService.Navigate(CardPayWave);
            BdSubFrame.Visibility = Visibility.Visible;
            System.Windows.Forms.Application.DoEvents();

        }

        private void StartBoostEwalletPayment(object sender, EventArgs e)
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

        private void GetAfcPackage()
        {
            GridJourneyButton.Visibility = Visibility.Visible;
            GridJourneyButton1.Children.Clear();

            IAppCache cache = new CachingService();
            ListOfAfcPackages = cache.GetAsync<List<Packages>>("AFCPackagesCache").Result;

            if (ListOfAfcPackages.Count <= 0) return;
            foreach (var item in ListOfAfcPackages)
            {
                _journeyTypeButton = new JourneyTypeButton
                {
                    Margin = new Thickness(5, 3, 5, 3),
                    Padding = new Thickness(5, 3, 5, 3),
                    lblJourneyId =
                    {
                        Text = item.PackageJourney
                    },
                    lblJourney =
                    {
                        Text = item.PackageName
                    },
                    lblJourneyDate =
                    {
                        Text = DateTime.Now.ToString("ddd, dd-MM")
                    },
                };

                GridJourneyButton1.Children.Add(_journeyTypeButton);
            }

            for (int i = 0; i < GridJourneyButton1.Children.Count; i++)
            {
                if (!(GridJourneyButton1.Children[i] is JourneyTypeButton btn)) continue;
                btn.BtnJourney.Click += (sender, args) =>
                {
                    var id = btn.lblJourneyId.Text;
                    btn.BtnJourney.Style = (Style)FindResource("BtnSelectedJourney");

                    FrmSubFrame.Content = null;
                    FrmSubFrame.NavigationService.RemoveBackEntry();
                    FrmSubFrame.NavigationService.Content = null;
                    FrmSubFrame.NavigationService.Navigate(_pgKomuterPax);
                    BdSubFrame.Visibility = Visibility.Visible;

                    for (int j = 0; j < GridJourneyButton1.Children.Count; j++)
                    {
                        if (!(GridJourneyButton1.Children[j] is JourneyTypeButton btn1)) continue;
                        if (btn1.lblJourneyId.Text == id) continue;
                        btn1.BtnJourney.Style = (Style)FindResource("BtnDefaultJourney");
                    }
                };
            }

            _pgKomuterPax.BtnCancel.Click += (sender, args) =>
            {
                FrmSubFrame.Content = null;
                FrmSubFrame.NavigationService.RemoveBackEntry();
                FrmSubFrame.NavigationService.Content = null;
                System.Windows.Forms.Application.DoEvents();

                BdSubFrame.Visibility = Visibility.Hidden;
                System.Windows.Forms.Application.DoEvents();

                foreach (var journeyButton in GridJourneyButton1.Children)
                {
                    if (!(journeyButton is JourneyTypeButton btn)) continue;
                    btn.BtnJourney.Style = (Style)FindResource("BtnDefaultJourney");
                }
            };

            _pgKomuterPax.BtnOk.Click += async (sender, args) =>
            {
                PassengerTemplate passengerTemplate = new PassengerTemplate();
                ParcelScreen parcelScreen = new ParcelScreen();
                ParcelActionButton parcelActionButton = new ParcelActionButton();

                if (UserSession.ChildSeat != 0 || UserSession.SeniorSeat != 0)
                {
                    FrmSubFrame.Content = null;
                    FrmSubFrame.NavigationService.RemoveBackEntry();
                    FrmSubFrame.NavigationService.Content = null;
                    FrmSubFrame.NavigationService.Navigate(passengerTemplate);
                    FrmSubFrame.Width = 1024;
                    BdSubFrame.Visibility = Visibility.Visible;
                }
                else
                {
                    FrmSubFrame.Content = null;
                    FrmSubFrame.NavigationService.RemoveBackEntry();
                    FrmSubFrame.NavigationService.Content = null;
                    FrmSubFrame.NavigationService.Navigate(parcelScreen);
                    FrmSubFrame.Width = 1024;
                    BdSubFrame.Visibility = Visibility.Visible;

                    parcelScreen.BtnSkip.Click += (sender1, args1) =>
                    {
                        FrmSubFrame.Content = null;
                        FrmSubFrame.NavigationService.RemoveBackEntry();
                        FrmSubFrame.NavigationService.Content = null;
                        BdSubFrame.Visibility = Visibility.Collapsed;
                    };

                    parcelScreen.BtnOk.Click += (sender1, args1) =>
                    {
                        FrmSubFrame.Content = null;
                        FrmSubFrame.NavigationService.RemoveBackEntry();
                        FrmSubFrame.NavigationService.Content = null;
                        BdSubFrame.Visibility = Visibility.Collapsed;
                    };
                }

                passengerTemplate.BtnCancel.Click += (sender1, args1) =>
                {
                    FrmSubFrame.Content = null;
                    FrmSubFrame.NavigationService.RemoveBackEntry();
                    FrmSubFrame.NavigationService.Content = null;

                    FrmSubFrame.Content = null;
                    FrmSubFrame.NavigationService.RemoveBackEntry();
                    FrmSubFrame.NavigationService.Content = null;
                    FrmSubFrame.NavigationService.Navigate(_pgKomuterPax);
                    System.Windows.Forms.Application.DoEvents();

                    UserSession.ChildSeat = 0;
                    UserSession.SeniorSeat = 0;
                    UserSession.TempDataForChildSeat = 0;
                    UserSession.TempDataForSeniorSeat = 0;
                    UserSession.IsVerifyAgeAFCRequiredForSenior = false;
                    UserSession.IsVerifyAgeAFCRequiredForChild = false;
                    UserSession.TicketOrderTypes = null;

                    foreach (var journeyButton in GridJourneyButton1.Children)
                    {
                        if (!(journeyButton is JourneyTypeButton btn)) continue;
                        btn.BtnJourney.Style = (Style)FindResource("BtnDefaultJourney");
                    }
                };

                passengerTemplate.BtnOk.Click += (sender1, args1) =>
                {
                    if (UserSession.SeniorSeat == 0 && UserSession.ChildSeat == 0)
                    {
                        PassengerInfo.IcScanned.Clear();
                        PassengerInfo.PassengerName = null;

                        FrmSubFrame.Content = null;
                        FrmSubFrame.NavigationService.RemoveBackEntry();
                        FrmSubFrame.NavigationService.Content = null;
                        FrmSubFrame.NavigationService.Navigate(parcelScreen);
                        System.Windows.Forms.Application.DoEvents();

                        parcelScreen.BtnSkip.Click += (sender2, args2) =>
                        {
                            FrmSubFrame.Content = null;
                            FrmSubFrame.NavigationService.RemoveBackEntry();
                            FrmSubFrame.NavigationService.Content = null;
                            BdSubFrame.Visibility = Visibility.Collapsed;
                        };

                        parcelScreen.BtnOk.Click += (sender2, args2) =>
                        {
                            FrmSubFrame.Content = null;
                            FrmSubFrame.NavigationService.RemoveBackEntry();
                            FrmSubFrame.NavigationService.Content = null;
                            BdSubFrame.Visibility = Visibility.Collapsed;
                        };
                    }

                };
            };
        }

        public void FilterStationByKeyboardInput(bool isAllSelection, string afcRouteModelId, string inputFromTextBox)
        {
            TxtTextBox.Text = inputFromTextBox;
            IAppCache cache = new CachingService();
            ListOfAllStations = cache.GetAsync<List<AFCStationDetails>>("AFCStationsCache").Result;

            if (ListOfAllStations == null) return;

            GridStation.Children.Clear();

            switch (isAllSelection)
            {
                case true:
                    {
                        var filteredStations =
                            ListOfAllStations.Where(x => x.Station.StartsWith(inputFromTextBox)).ToList();

                        foreach (var station in filteredStations)
                        {
                            var genericStation = new GenericStationButton()
                            {
                                LblStationName =
                            {
                                Text = station.Station
                            },
                                LblStationId =
                            {
                                Text = station.Id
                            },
                                StationColorCode =
                            {
                                Background =
                                    new SolidColorBrush((Color)ConvertFromString(station.ColorCode))
                            },
                                Margin = new Thickness(5, 3, 5, 3),
                                Padding = new Thickness(5, 3, 5, 3),
                                Height = 60,
                            };

                            UserSession.ToStationId = station.Id;

                            GridStation.Children.Add(genericStation);

                            genericStation.BtnGenericStation.MouseLeftButtonDown += (e, s) =>
                            {
                                _destinationScreen.lblToStation.Text = genericStation.LblStationName.Text;
                                UserSession.ToStationName = genericStation.LblStationName.Text;

                                foreach (var btnStation in GridStation.Children)
                                {
                                    if (!(btnStation is GenericStationButton btn)) continue;
                                    var btnBg = btn.BtnGenericStation.Background;
                                    if (btnBg?.ToString() == "#FFFBD012")
                                    {
                                        btn.BtnGenericStation.Background =
                                            new SolidColorBrush((Color)ConvertFromString("#FFFFFFFF"));
                                    }
                                }

                                genericStation.BtnGenericStation.Background =
                                    new SolidColorBrush((Color)ConvertFromString("#FFFBD012"));

                                GetAfcPackage();

                                GridJourneyButton.Visibility = Visibility.Visible;
                            };
                        }

                        break;
                    }
                case false:
                    {
                        var filteredStations = ListOfAllStations.Where(x =>
                            x.RouteId[0].Equals(afcRouteModelId) && x.Station.StartsWith(inputFromTextBox)).ToList();
                        var filteredInterchangeStations = ListOfAllStations
                            .Where(x => x.IsInterchange && x.Station.StartsWith(inputFromTextBox)).ToList();

                        filteredStations.AddRange(filteredInterchangeStations);

                        foreach (var station in filteredStations)
                        {
                            var genericStation = new GenericStationButton()
                            {
                                LblStationName =
                            {
                                Text = station.Station
                            },
                                LblStationId =
                            {
                                Text = station.Id
                            },
                                StationColorCode =
                            {
                                Background =
                                    new SolidColorBrush((Color)ConvertFromString(station.ColorCode))
                            },
                                Margin = new Thickness(5, 3, 5, 3),
                                Padding = new Thickness(5, 3, 5, 3),
                                Height = 60,
                            };

                            UserSession.ToStationId = station.Id;

                            GridStation.Children.Add(genericStation);

                            genericStation.BtnGenericStation.MouseLeftButtonDown += (e, s) =>
                            {
                                _destinationScreen.lblToStation.Text = genericStation.LblStationName.Text;

                                foreach (var btnStation in GridStation.Children)
                                {
                                    if (!(btnStation is GenericStationButton btn)) continue;
                                    var btnBg = btn.BtnGenericStation.Background;
                                    if (btnBg?.ToString() == "#FFFBD012")
                                    {
                                        btn.BtnGenericStation.Background =
                                            new SolidColorBrush((Color)ConvertFromString("#FFFFFFFF"));
                                    }
                                }

                                UserSession.ToStationId = station.Id;

                                genericStation.BtnGenericStation.Background =
                                    new SolidColorBrush((Color)ConvertFromString("#FFFBD012"));

                                GetAfcPackage();

                                GridJourneyButton.Visibility = Visibility.Visible;
                            };
                        }

                        break;
                    }
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (isAllSelection == true)
                {
                    _allRouteButton.BtnAll.Style = (Style)FindResource("BtnSelected");
                }
                else
                {
                    _allRouteButton.BtnAll.Style = (Style)FindResource("BtnDefaultAll");
                }

            }));
        }

        private void BtnViewMap_Click(object sender, RoutedEventArgs e)
        {
            var viewMapScreen = new ViewMapScreen();

            var window = new Window
            {
                Title = "KTM ETS Map",
                Content = viewMapScreen,
                Width = 1000,
                Height = 1750,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                Owner = Application.Current.MainWindow
            };
            if (window.Owner != null)
            {
                window.Owner.Effect = new System.Windows.Media.Effects.BlurEffect();
                window.Owner.Opacity = 0.5;
            }

            window.ShowDialog();
        }

        private void BtnA_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "A";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnS_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "S";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnD_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "D";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnF_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "F";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnG_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "G";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnH_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "H";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnJ_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "J";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnK_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "K";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnL_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "L";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnZ_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "Z";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnX_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "X";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnC_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "C";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnV_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "V";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnB_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "B";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnN_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "N";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnM_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "M";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnQ_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        private void BtnQ_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnQ.Background = Brushes.White;
        }

        private void BtnW_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnW.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnW_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnW.Background = Brushes.White;
        }

        private void BtnE_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnE.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnE_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnE.Background = Brushes.White;
        }

        private void BtnR_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnR.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnR_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnR.Background = Brushes.White;
        }

        private void BtnT_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnT.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnT_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnT.Background = Brushes.White;
        }

        private void BtnY_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnY.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnY_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnY.Background = Brushes.White;
        }

        private void BtnU_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnU.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnU_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnU.Background = Brushes.White;
        }

        private void BtnI_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnI.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnI_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnI.Background = Brushes.White;
        }

        private void BtnO_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnO.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnO_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnO.Background = Brushes.White;
        }

        private void BtnP_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnP.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnP_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnP.Background = Brushes.White;
        }

        private void BtnA_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnA.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnA_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnA.Background = Brushes.White;
        }

        private void BtnS_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnS.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnS_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnS.Background = Brushes.White;
        }

        private void BtnD_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnD.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnD_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnD.Background = Brushes.White;
        }

        private void BtnF_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnF.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnF_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnF.Background = Brushes.White;
        }

        private void BtnG_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnG.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnG_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnG.Background = Brushes.White;
        }

        private void BtnH_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnH.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnH_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnH.Background = Brushes.White;
        }

        private void BtnJ_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnJ.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnJ_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnJ.Background = Brushes.White;
        }

        private void BtnK_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnK.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnK_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnK.Background = Brushes.White;
        }

        private void BtnL_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnL.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnL_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnL.Background = Brushes.White;
        }

        private void BtnZ_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnZ.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnZ_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnZ.Background = Brushes.White;
        }

        private void BtnX_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnX.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnX_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnX.Background = Brushes.White;
        }

        private void BtnC_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnC.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnC_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnC.Background = Brushes.White;
        }

        private void BtnV_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnV.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnV_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnV.Background = Brushes.White;
        }

        private void BtnB_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnB.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnB_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnB.Background = Brushes.White;
        }

        private void BtnN_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnN.Background = Brushes.White;
        }

        private void BtnM_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnM.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnM_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnM.Background = Brushes.White;
        }

        private void BtnClear_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnClear.Background = (Brush)new BrushConverter().ConvertFromString("#FBD012");
        }

        private void BtnClear_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnClear.Background = Brushes.White;
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            GridStation.Children.Clear();
            GetAllStations();
            TxtTextBox.Text = string.Empty;
        }

        private void BtnQ_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "Q";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnW_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "W";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnE_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "E";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnR_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "R";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnT_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "T";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnY_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "Y";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnU_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "U";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnI_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "I";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnO_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "O";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void BtnP_Click(object sender, RoutedEventArgs e)
        {
            TxtTextBox.Text += "P";

            IAppCache appCache = new CachingService();
            var routeId = appCache.Get<string>("RouteIdCache");
            FilterStationByKeyboardInput(IsAllSelected, routeId, TxtTextBox.Text);
        }

        private void DisableKeyboardUponCheckOut()
        {
            //while (!UserSession.IsCheckOut || !UserSession.IsParcelCheckOut)
            //{
            //    await Task.Delay(500);
            //

            _checkOutTimer.Interval = TimeSpan.FromMilliseconds(350);

            _checkOutTimer.Tick += async (sender, args) =>
            {
                if (!UserSession.IsCheckOut && !UserSession.IsParcelCheckOut) return;
                _checkOutTimer.Stop();

                BtnA.IsEnabled = false;
                BtnS.IsEnabled = false;
                BtnD.IsEnabled = false;
                BtnF.IsEnabled = false;
                BtnG.IsEnabled = false;
                BtnH.IsEnabled = false;
                BtnJ.IsEnabled = false;
                BtnK.IsEnabled = false;
                BtnL.IsEnabled = false;
                BtnZ.IsEnabled = false;
                BtnX.IsEnabled = false;
                BtnC.IsEnabled = false;
                BtnV.IsEnabled = false;
                BtnB.IsEnabled = false;
                BtnN.IsEnabled = false;
                BtnM.IsEnabled = false;
                BtnQ.IsEnabled = false;
                BtnW.IsEnabled = false;
                BtnE.IsEnabled = false;
                BtnR.IsEnabled = false;
                BtnT.IsEnabled = false;
                BtnY.IsEnabled = false;
                BtnU.IsEnabled = false;
                BtnI.IsEnabled = false;
                BtnO.IsEnabled = false;
                BtnP.IsEnabled = false;
                BtnClear.IsEnabled = false;
                // BtnReset.IsEnabled = false;
                TxtTextBox.IsReadOnly = true;
            };

            _checkOutTimer.Start();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //GetAfcRoutes();
            // GetAllStations();


        }

    }
}