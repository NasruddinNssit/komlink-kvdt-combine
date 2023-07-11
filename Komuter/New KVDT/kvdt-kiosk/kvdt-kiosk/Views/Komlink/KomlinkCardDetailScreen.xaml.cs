using Microsoft.Reporting.WinForms;
using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;
using NssIT.Kiosk.Log.DB.StatusMonitor;
using NssIT.Train.Kiosk.Common.Constants;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using kvdt_kiosk.Models.Komlink;
using kvdt_kiosk.Reports.KTMB;
using kvdt_kiosk.Reports;
using kvdt_kiosk.Services.Komlink;
using kvdt_kiosk.Views.Payment.PayWave;
using kvdt_kiosk.Views.Payment.Komlink.Ewallet;
using NssIT.Kiosk.Client;

namespace kvdt_kiosk.Views.Komlink
{
    /// <summary>
    /// Interaction logic for KomlinkCardDetailScreen.xaml
    /// </summary>
    public partial class KomlinkCardDetailScreen : UserControl
    {


        bool isPrinterError = false;
        bool isPrinterWarning = false;
        string statusDescription = null;
        string locStateDesc = null;

        bool isPrintReceipt = false;

        private LanguageCode _language = LanguageCode.English;
        CardDetail cardDetail;
        PaymentAmountOption paymentAmountOption;
        BackButton backButton;

        PaymentOption paymentOption;

        KeyPad keypad;

        ExitButton exitButton;

        TransactionDate transactionDate = new TransactionDate();

        TableTransaction tableTransaction;

        Calendar calendar;

        DateTime _startDate;
        DateTime _endDate;


        ConfirmButton confirmButton;

        kvdt_kiosk.Views.Payment.Komlink.PayWave.CardPayWaveexe cardPayWave = null;

        bool isIm30Ready = false;

        private Thread _printingThreadWorker = null;
        private Thread _endPaymentThreadWorker = null;
        private CreditCardResponse _lastCreditCardAnswer = null;

        private BigAlert bigAlert = null;

        private AlertSuccess alertSuccess = null;
        private AlertForScanAfterPayment alertScan = null;
        private AlertUnSuccess alertUnSuccess = null;
        private pgKomuter_BTnGPayment bTnGPayment = null;

        private AlertInfo _alertInfo = null;
        private bool isPaymentGateway = false;
        private string paymentMethodChoosed = null;
        private string paymentMethodTypeChoosed = null;
        private string paymentGatewayLogoUrl = null;

        ResourceDictionary langRec = null;

        private bool isAlreadyPrintForRetry = false;

        private string KomlinkTopUpReceipt
        {
            get
            {
                return "KomlinkTopUpReceiptV";
            }
        }

        private string CreditCardReceiptReportSourceName
        {
            get
            {
                return "RPTCreditCardReceipt";
            }
        }



        private static Lazy<KomlinkCardDetailScreen> komlinkCardDetailScreen = new Lazy<KomlinkCardDetailScreen>(() => new KomlinkCardDetailScreen());

        public static KomlinkCardDetailScreen GetKomlinkCardDetailScreen()
        {
            return komlinkCardDetailScreen.Value;
        }
        public KomlinkCardDetailScreen()
        {
            InitializeComponent();
            LoadLanguage();
            cardDetail = new CardDetail();
            KomlinkDetail.Children.Add(cardDetail);

            exitButton = new ExitButton();
            BackOrExitButton.Children.Add(exitButton);

            cardPayWave = kvdt_kiosk.Views.Payment.Komlink.PayWave.CardPayWaveexe.GetCreditCardPayWavePage();

            bigAlert = BigAlert.GetBigAlertPage();

            bigAlert.onSuccessToUp += BigAlert_onSuccessToUp;
            bigAlert.onFailureToUp += BigAlert_onFailureToUp;
            alertScan = AlertForScanAfterPayment.GetAlertPage();

            alertScan.ButtonScanClicked += AlertScan_ButtonScanClicked;
            _alertInfo = AlertInfo.GetAlertInfo();
            _alertInfo.closeAlert += _alertInfo_closeAlert;
            alertSuccess = AlertSuccess.GetAlertSuccess();
            alertSuccess.OnNoPrintClicked += AlertSuccess_OnNoPrintClicked;
            alertSuccess.OnYesPrintClicked += AlertSuccess_OnYesPrintClicked;
            alertUnSuccess = AlertUnSuccess.GetUnSuccesAlertPage();
            alertUnSuccess.ButtonScanAgainClicked += AlertScan_ButtonScanClicked;
            paymentAmountOption = new PaymentAmountOption();
            alertUnSuccess.ButtonNotScanAgainClicked += AlertUnSuccess_ButtonNotScanAgainClicked;

            bTnGPayment = new pgKomuter_BTnGPayment(frmSubFrame: FrmSubFrame, bdSubFrame: BdSubFrame);
        }

        private void _alertInfo_closeAlert(object sender, EventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;

            FrmSubFrame.Content = null;
            FrmSubFrame.NavigationService.RemoveBackEntry();
            FrmSubFrame.NavigationService.Content = null;

            BdSubFrame.Visibility = Visibility.Collapsed;
            System.Windows.Forms.Application.DoEvents();

            //FrmSubFrame.NavigationService.Navigate(alert);
            //BdSubFrame.Visibility = Visibility.Visible;
            //System.Windows.Forms.Application.DoEvents();
        }


        private void BigAlert_onFailureToUp(object sender, OnFailureScanEventArgs e)
        {

            if (e.retryCount <= 1) // print receipt for the first fail ettempt
            {

                //ValidatePrinterStatus();
                if (isPrinterError || isPrinterWarning)
                {

                }
                else
                {

                    bool isProceedPrintTicket = false;
                    if (UserSession.requestAddTopUpModel.komlinkCardReceiptGetModel != null)
                    {
                        isProceedPrintTicket = true;
                    }
                    if (isProceedPrintTicket)
                    {
                        UserSession.requestAddTopUpModel.komlinkCardReceiptGetModel[0].IsRetryAttempt = true;
                        UserSession.requestAddTopUpModel.komlinkCardReceiptGetModel[0].CounterId = UserSession.KioskId;
                        PrintTicket(UserSession.requestAddTopUpModel.komlinkCardReceiptGetModel);

                    }
                }
            }

            if (e.retryCount > 10)
            {
                alertUnSuccess.HideClickAgainButton();
            }

            cardPayWave.ClearEvents();

            FrmSubFrame.Content = null;
            FrmSubFrame.NavigationService.RemoveBackEntry();
            FrmSubFrame.NavigationService.Content = null;

            System.Windows.Forms.Application.DoEvents();

            FrmSubFrame.NavigationService.Navigate(alertUnSuccess);
            BdSubFrame.Visibility = Visibility.Visible;
            System.Windows.Forms.Application.DoEvents();

        }

        private void AlertUnSuccess_ButtonNotScanAgainClicked(object sender, EventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;

            cardPayWave.ClearEvents();
            FrmSubFrame.Content = null;
            FrmSubFrame.NavigationService.RemoveBackEntry();
            FrmSubFrame.NavigationService.Content = null;
            BdSubFrame.Visibility = Visibility.Collapsed;
        }

        private void AlertSuccess_OnYesPrintClicked(object sender, EventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;

            //ValidatePrinterStatus();

            cardPayWave.ClearEvents();
            FrmSubFrame.Content = null;
            FrmSubFrame.NavigationService.RemoveBackEntry();
            FrmSubFrame.NavigationService.Content = null;
            BdSubFrame.Visibility = Visibility.Collapsed;

            if (isPrinterError || isPrinterWarning)
            {

            }
            else
            {

                isPrintReceipt = true;
                _printingThreadWorker = new Thread(new ThreadStart(PrintThreadWorking));
                _printingThreadWorker.IsBackground = true;
                _printingThreadWorker.Start();

                void PrintThreadWorking()
                {
                    bool isProceedPrintTicket = false;
                    if (UserSession.requestAddTopUpModel.komlinkCardReceiptGetModel != null)
                    {
                        isProceedPrintTicket = true;
                    }

                    if (isProceedPrintTicket)
                    {
                        UserSession.requestAddTopUpModel.komlinkCardReceiptGetModel[0].IsRetryAttempt = false;
                        UserSession.requestAddTopUpModel.komlinkCardReceiptGetModel[0].TopUpAmount = decimal.Parse(UserSession.requestAddTopUpModel.komlinkCardReceiptGetModel[0].TopUpAmount.ToString("F2"));
                        UserSession.requestAddTopUpModel.komlinkCardReceiptGetModel[0].CardBalanceAmount = decimal.Parse(UserSession.requestAddTopUpModel.komlinkCardReceiptGetModel[0].CardBalanceAmount.ToString("F2"));
                        UserSession.requestAddTopUpModel.komlinkCardReceiptGetModel[0].CounterId = UserSession.KioskId;
                        PrintTicket(UserSession.requestAddTopUpModel.komlinkCardReceiptGetModel);
                    }
                }
            }

            ClearTextBoxAndUpdateTopUpAmount();

        }

        private void PrintTicket(KomlinkCardReceiptGetModel[] komuterPrintTickets)
        {
            SystemConfig.IsResetIdleTimer = true;

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
                    rw.AmountString = "MYR" + UserSession.TotalTicketPrice.ToString("F2");
                    rw.MachineId = UserSession.KioskId;
                    rw.RefNumber = UserSession.requestAddTopUpModel.TransactionNo;
                    dtPaywave.Rows.Add(rw);
                    dtPaywave.AcceptChanges();
                }
                _lastCreditCardAnswer = null;

                //App.Log.LogText(_logChannel, _ktmbSalesTransactionNo ?? "#?#", "Debug:Print01-Start Render Data", "DBG05", classNMethodName: "pgKomuter.PrintTicket", messageType: AppDecorator.Log.MessageType.Debug);


                ReportImageSize receiptSize = new ReportImageSize(3.2M, 8.2M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
                Stream[] streamReceiptList = null;
                if (dtPaywave != null)
                {
                    LocalReport receiptRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\KTMB\{CreditCardReceiptReportSourceName}.rdlc",
                    new ReportDataSource[] { new ReportDataSource("DataSet1", (DataTable)dtPaywave) });
                    streamReceiptList = RdlcImageRendering.Export(receiptRep, receiptSize);
                }


                LocalReport ticketRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\KTMB\KomuterTicket\{KomlinkTopUpReceipt}.rdlc",
                   new ReportDataSource[] { new ReportDataSource("DataSet1", new List<KomlinkCardReceiptGetModel>(komuterPrintTickets)) });
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
            catch (Exception ex)
            {
                //App.ShowDebugMsg("Error on pgKomuter.PrintTicket; EX01");
                //App.Log.LogError(_logChannel, _ktmbSalesTransactionNo, ex, "EX01", "pgKomuter.PrintTicket",
                //    adminMsg: $@"Error when printing; {ex.Message}");
            }
        }


        private void AlertSuccess_OnNoPrintClicked(object sender, EventArgs e)
        {

            SystemConfig.IsResetIdleTimer = true;

            cardPayWave.ClearEvents();

            FrmSubFrame.Content = null;
            FrmSubFrame.NavigationService.RemoveBackEntry();
            FrmSubFrame.NavigationService.Content = null;

            System.Windows.Forms.Application.DoEvents();

            BdSubFrame.Visibility = Visibility.Collapsed;
            System.Windows.Forms.Application.DoEvents();


            ClearTextBoxAndUpdateTopUpAmount();
        }


        private void ClearTextBoxAndUpdateTopUpAmount()
        {
            paymentAmountOption.ClearText();

            paymentAmountOption.ResetSelectedButton();
            cardDetail.UpdateAmountAfterTopUP();


        }



        private void BigAlert_onSuccessToUp(object sender, EventArgs e)
        {

            RequestUpdateWriteCardStatus();
            cardPayWave.ClearEvents();

            FrmSubFrame.Content = null;
            FrmSubFrame.NavigationService.RemoveBackEntry();
            FrmSubFrame.NavigationService.Content = null;

            System.Windows.Forms.Application.DoEvents();

            FrmSubFrame.NavigationService.Navigate(alertSuccess);
            BdSubFrame.Visibility = Visibility.Visible;
            System.Windows.Forms.Application.DoEvents();

            bigAlert.SetRetryScanToZerro();



        }

        private void AlertScan_ButtonScanClicked(object sender, ScanEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;

            if (e.isRetryScan)
            {
                bigAlert.CheckIsRetry(true);
                bigAlert.scanFor("TopUp");

                bigAlert.IncrementRetryScan(e.retryCount);
            }
            else
            {
                bigAlert.scanFor("TopUp");
            }

            cardPayWave.ClearEvents();

            FrmSubFrame.Content = null;
            FrmSubFrame.NavigationService.RemoveBackEntry();
            FrmSubFrame.NavigationService.Content = null;

            System.Windows.Forms.Application.DoEvents();

            FrmSubFrame.NavigationService.Navigate(bigAlert);
            BdSubFrame.Visibility = Visibility.Visible;
            System.Windows.Forms.Application.DoEvents();
        }

        private void LoadLanguage()
        {
            try
            {
                if (App.Language == "ms")
                {
                    KomlinkCardText.Text = "Komlink Kad";
                    TopUpBtnText.Content = "Tambah Nilai";
                    ViewTransactionText.Content = "Papar Transaksi";
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in KomlinkCardDetailScreen.xaml.cs");

            }
        }

        private void Button_TopUp_Click(object sender, RoutedEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;

            ConfirmButton.Children.Clear();
            KomlinkToUpSection.Children.Clear();
           
            KomlinkToUpSection.Children.Add(paymentAmountOption);

            paymentAmountOption.TextBoxClicked += TextBox_Clicked;

            PaymentOptionOrKeypad.Children.Clear();

            paymentOption = new PaymentOption();
            paymentOption.ButtonPayWaveClicked += ButtonPayWaveClicked;

            paymentOption.ButtonBTngClicked += ButtonBTnGClickded;
            paymentOption.ButtonTngClicked += ButtonTngClicked;
            PaymentOptionOrKeypad.Children.Add(paymentOption);

            BackOrExitButton.Children.Clear();

            backButton = new BackButton();

            backButton.BackButtonClicked += BackButton_Clicked;

            BackOrExitButton.Children.Add(backButton);
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;

            ConfirmButton.Children.Clear();
            KomlinkToUpSection.Children?.Clear();

            PaymentOptionOrKeypad.Children?.Clear();

            BackOrExitButton.Children.Clear();

            BackOrExitButton.Children.Add(exitButton);
        }

        private void TextBox_Clicked(object sender, EventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;

            PaymentOptionOrKeypad.Children.Clear();

            keypad = new KeyPad();
            keypad.KeyPadClicked += KeyPad_Clicked;
            keypad.KeyPadDeletedClicked += KeyPadDelete_Clicked;
            keypad.KeyPadEnterClicked += KeyPadEnter_Clicked;

            keypad.keyPadCancelClicked += KeyPadCancel_Clicked;
            PaymentOptionOrKeypad.Children.Add(keypad);

            BackOrExitButton.Children?.Clear();

            //paymentAmountOption.SetText(0);

            paymentAmountOption.SetAllButtonUnSelected();
        }

        private void KeyPad_Clicked(object sender, EventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;

            Button button = (Button)sender;
            if (button != null)
            {
                string amountText = button.Content.ToString();

                int amount = Convert.ToInt32(amountText);

                paymentAmountOption.SetText(amount);
            }

            UpdateEnterButton();
        }

        private void KeyPadEnter_Clicked(object sender, EventArgs e)
        {
            ConfirmButton.Children.Clear();
            PaymentOptionOrKeypad.Children.Clear();
            PaymentOptionOrKeypad.Children.Add(paymentOption);
            BackOrExitButton?.Children?.Clear();
            BackOrExitButton?.Children.Add(backButton);
        }

        private void KeyPadCancel_Clicked(object sender, EventArgs e)
        {


            ConfirmButton.Children.Clear();
            PaymentOptionOrKeypad.Children.Clear();
            PaymentOptionOrKeypad.Children.Add(paymentOption);
            BackOrExitButton?.Children?.Clear();
            BackOrExitButton?.Children.Add(backButton);


            paymentAmountOption.cancelKey();

        }

        private void KeyPadDelete_Clicked(object sender, EventArgs e)
        {
            string text = paymentAmountOption.GetText();

            if (string.IsNullOrEmpty(text))
            {
                return; // Exit the method if the text is empty or null
            }

            if (text.Length == 1)
            {
                paymentAmountOption.SetTextAfterDelete(0); // Set a default value when the text has only one character
                keypad.DisableEnterButton();
                return;
            }

            int amount;
            if (int.TryParse(text.Remove(text.Length - 1), out amount))
            {
                paymentAmountOption.SetTextAfterDelete(amount);
            }
            else
            {
                // Handle the case when the text cannot be parsed to an integer
                // You can display an error message or perform alternative actions
            }
        }


        private void UpdateEnterButton()
        {
            keypad.UpdateEnterButton();
        }

        private async void Button_Transaction_Clicked(object sender, RoutedEventArgs e)
        {
            ConfirmButton.Children.Clear();
            KomlinkToUpSection.Children?.Clear();
            PaymentOptionOrKeypad.Children?.Clear();
            BackOrExitButton?.Children?.Clear();


            transactionDate.DateRangeClicked += DateRangeClicked;
            transactionDate.Past1WeekClicked += BtnPast1WeekClicked;
            transactionDate.Past30DayClicked += BtnPast30DayClicked;
            transactionDate.TodayClicked += BtnTodayClicked;

            KomlinkToUpSection?.Children?.Add(transactionDate);

            tableTransaction = new TableTransaction();


            //List<KomlinkTransactionDetail> komlinkTransactions = getTransactionData().Result;

            //tableTransaction.AddData(komlinkTransactions);
            PaymentOptionOrKeypad.Children?.Add(tableTransaction);

            exitButton = new ExitButton();
            BackOrExitButton?.Children?.Add(exitButton);

            ViewTransactionToday();
            //transactionDate.Btn_TodayClicked();

        }

        private async void ViewTransactionToday()
        {

            DateTime startDate = DateTime.UtcNow.Date;
            DateTime endDate = startDate.AddDays(1).AddTicks(-1);
            transactionDate.setStartDateText(startDate.ToString("dd MMM yyyy"));
            transactionDate.setEndDateText(startDate.ToString("dd MMM yyyy"));


            _startDate = startDate;
            _endDate = endDate.AddDays(1).AddTicks(-1);

            List<KomlinkTransactionDetail> komlinkTransactions = await getTransactionData(_startDate, _endDate);
            List<KomlinkTransactionDetailItem> komlinkTransactionDetailItems = new List<KomlinkTransactionDetailItem>();

            foreach (KomlinkTransactionDetail item in komlinkTransactions)
            {
                KomlinkTransactionDetailItem kTdI = new KomlinkTransactionDetailItem();



                switch (item.TransactionType.ToString())
                {
                    case "1":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Top Up";
                        }
                        else
                        {
                            kTdI.TransactionType = "Top Up";
                        }
                        break;
                    case "2":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Pengambilan";
                        }
                        else
                        {
                            kTdI.TransactionType = "Refund";
                        }
                        break;
                    case "3":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Pengunaan";
                        }
                        else
                        {
                            kTdI.TransactionType = "Usage";
                        }
                        break;
                    case "4":
                        break;
                    case "5":
                        break;

                    case "6":
                        break;
                    default:
                        break;


                }

                kTdI.Date = item.TransactionDateTime.Date.ToString("dd/MM/yyyy");
                kTdI.Time = item.TransactionDateTime.TimeOfDay.ToString(@"hh\:mm");
                kTdI.Station = item.Station;
                kTdI.TicketType = item.TicketType;
                kTdI.TotalAmount = item.TotalAmount.ToString("C2").Replace("$", "RM");

                komlinkTransactionDetailItems.Add(kTdI);
            }

            tableTransaction.AddData(komlinkTransactionDetailItems);

            PaymentOptionOrKeypad.Children.Clear();
            PaymentOptionOrKeypad.Children?.Add(tableTransaction);
        }



        private void DateRangeClicked(object sender, EventArgs e)
        {
            PaymentOptionOrKeypad.Children.Clear();
            calendar = new Calendar();
            calendar.SelectedStartDateChanged += setStartDateChange;
            calendar.SelectedEndDateChanged += setEndDateChange;
            PaymentOptionOrKeypad?.Children?.Add(calendar);

            ConfirmButton.Children.Clear();
            confirmButton = new ConfirmButton();


            confirmButton.ButtonConfirmClicked += ButtonConfirmClicked;
            ConfirmButton?.Children?.Add(confirmButton);
        }


        private DateTime startDateToFilter;
        private DateTime endDateToFilter;

        private async void ButtonConfirmClicked(object sender, EventArgs e)
        {


            if (endDateToFilter > startDateToFilter)
            {
                PaymentOptionOrKeypad.Children.Clear();
                ConfirmButton.Children.Clear();
                PaymentOptionOrKeypad.Children?.Add(tableTransaction);


                List<KomlinkTransactionDetail> komlinkTransactions = await getTransactionData(_startDate, _endDate);

                List<KomlinkTransactionDetailItem> komlinkTransactionDetailItems = new List<KomlinkTransactionDetailItem>();


                foreach (KomlinkTransactionDetail item in komlinkTransactions)
                {
                    KomlinkTransactionDetailItem kTdI = new KomlinkTransactionDetailItem();



                    switch (item.TransactionType.ToString())
                    {
                        case "1":
                            if (App.Language == "ms")
                            {
                                kTdI.TransactionType = "Top Up";
                            }
                            else
                            {
                                kTdI.TransactionType = "Top Up";
                            }
                            break;
                        case "2":
                            if (App.Language == "ms")
                            {
                                kTdI.TransactionType = "Pengambilan";
                            }
                            else
                            {
                                kTdI.TransactionType = "Refund";
                            }
                            break;
                        case "3":
                            if (App.Language == "ms")
                            {
                                kTdI.TransactionType = "Pengunaan";
                            }
                            else
                            {
                                kTdI.TransactionType = "Usage";
                            }
                            break;
                        case "4":
                            break;
                        case "5":
                            break;

                        case "6":
                            break;
                        default:
                            break;


                    }

                    kTdI.Date = item.TransactionDateTime.Date.ToString("dd/MM/yyyy");
                    kTdI.Time = item.TransactionDateTime.TimeOfDay.ToString(@"hh\:mm");
                    kTdI.Station = item.Station;
                    kTdI.TicketType = item.TicketType;
                    kTdI.TotalAmount = item.TotalAmount.ToString("C2").Replace("$", "RM");

                    komlinkTransactionDetailItems.Add(kTdI);
                }

                tableTransaction.AddData(komlinkTransactionDetailItems);
            }
            else
            {
                if (App.Language == "ms")
                {
                    _alertInfo.SetTextAlert("Jangka masa tidak sah");

                }
                else
                {
                    _alertInfo.SetTextAlert("Invalid date range");
                }

                FrmSubFrame.Content = null;
                FrmSubFrame.NavigationService.RemoveBackEntry();
                FrmSubFrame.NavigationService.Content = null;


                System.Windows.Forms.Application.DoEvents();

                FrmSubFrame.NavigationService.Navigate(_alertInfo);
                BdSubFrame.Visibility = Visibility.Visible;
                System.Windows.Forms.Application.DoEvents();
            }

        }

        private void setStartDateChange(object sender, EventArgs e)
        {

            string date = sender.ToString();
            DateTime startDate = DateTime.Parse(date);

            startDateToFilter = startDate;
            string startDateString = startDate.ToString("dd MMM yyyy");
            transactionDate.setStartDateText(startDateString);

            _startDate = startDate;
        }

        private void setEndDateChange(object sender, EventArgs e)
        {
            string date = sender.ToString();
            DateTime endDate = DateTime.Parse(date);

            endDateToFilter = endDate;
            string endDateString = endDate.ToString("dd MMM yyyy");
            transactionDate.setEndDateText(endDateString);

            _endDate = endDate.AddDays(1).AddTicks(-1);
        }
        private async void BtnPast30DayClicked(object sender, EventArgs e)
        {



            ConfirmButton.Children.Clear();
            PaymentOptionOrKeypad.Children.Clear();
            DateTime endDate = DateTime.Now;
            DateTime startDate = endDate.AddDays(-30);

            transactionDate.setStartDateText(startDate.ToString("dd MMM yyyy"));
            transactionDate.setEndDateText(endDate.ToString("dd MMM yyyy"));

            //tableTransaction = new TableTransaction();


            _startDate = startDate;
            _endDate = endDate.AddDays(1).AddTicks(-1);

            List<KomlinkTransactionDetail> komlinkTransactions = await getTransactionData(_startDate, _endDate);



            List<KomlinkTransactionDetailItem> komlinkTransactionDetailItems = new List<KomlinkTransactionDetailItem>();
            foreach (KomlinkTransactionDetail item in komlinkTransactions)
            {
                KomlinkTransactionDetailItem kTdI = new KomlinkTransactionDetailItem();



                switch (item.TransactionType.ToString())
                {
                    case "1":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Top Up";
                        }
                        else
                        {
                            kTdI.TransactionType = "Top Up";
                        }
                        break;
                    case "2":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Pengambilan";
                        }
                        else
                        {
                            kTdI.TransactionType = "Refund";
                        }
                        break;
                    case "3":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Pengunaan";
                        }
                        else
                        {
                            kTdI.TransactionType = "Usage";
                        }
                        break;
                    case "4":
                        break;
                    case "5":
                        break;

                    case "6":
                        break;
                    default:
                        break;


                }

                kTdI.Date = item.TransactionDateTime.Date.ToString("dd/MM/yyyy");
                kTdI.Time = item.TransactionDateTime.TimeOfDay.ToString(@"hh\:mm");
                kTdI.Station = item.Station;
                kTdI.TicketType = item.TicketType;
                kTdI.TotalAmount = item.TotalAmount.ToString("C2").Replace("$", "RM");

                komlinkTransactionDetailItems.Add(kTdI);
            }
            tableTransaction.AddData(komlinkTransactionDetailItems);
            PaymentOptionOrKeypad.Children.Clear();
            PaymentOptionOrKeypad.Children?.Add(tableTransaction);
        }

        private async void BtnPast1WeekClicked(object sender, EventArgs e)
        {
            ConfirmButton.Children.Clear();
            PaymentOptionOrKeypad.Children.Clear();

            DateTime endDate = DateTime.Now;
            DateTime startDate = endDate.AddDays(-7);

            transactionDate.setStartDateText(startDate.ToString("dd MMM yyyy"));
            transactionDate.setEndDateText(endDate.ToString("dd MMM yyyy"));

            //tableTransaction = new TableTransaction();

            _startDate = startDate;
            _endDate = endDate.AddDays(1).AddTicks(-1);

            List<KomlinkTransactionDetail> komlinkTransactions = await getTransactionData(_startDate, _endDate);



            List<KomlinkTransactionDetailItem> komlinkTransactionDetailItems = new List<KomlinkTransactionDetailItem>();
            foreach (KomlinkTransactionDetail item in komlinkTransactions)
            {
                KomlinkTransactionDetailItem kTdI = new KomlinkTransactionDetailItem();



                switch (item.TransactionType.ToString())
                {
                    case "1":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Top Up";
                        }
                        else
                        {
                            kTdI.TransactionType = "Top Up";
                        }
                        break;
                    case "2":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Pengambilan";
                        }
                        else
                        {
                            kTdI.TransactionType = "Refund";
                        }
                        break;
                    case "3":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Pengunaan";
                        }
                        else
                        {
                            kTdI.TransactionType = "Usage";
                        }
                        break;
                    case "4":
                        break;
                    case "5":
                        break;

                    case "6":
                        break;
                    default:
                        break;


                }

                kTdI.Date = item.TransactionDateTime.Date.ToString("dd/MM/yyyy");
                kTdI.Time = item.TransactionDateTime.TimeOfDay.ToString(@"hh\:mm");
                kTdI.Station = item.Station;
                kTdI.TicketType = item.TicketType;
                kTdI.TotalAmount = item.TotalAmount.ToString("C2").Replace("$", "RM"); ;

                komlinkTransactionDetailItems.Add(kTdI);
            }
            tableTransaction.AddData(komlinkTransactionDetailItems);
            PaymentOptionOrKeypad.Children.Clear();
            PaymentOptionOrKeypad.Children?.Add(tableTransaction);
        }
        private async void BtnTodayClicked(object sender, EventArgs e)
        {
            ConfirmButton.Children.Clear();
            PaymentOptionOrKeypad.Children.Clear();
            DateTime startDate = DateTime.UtcNow.Date;
            DateTime endDate = startDate.AddDays(1).AddTicks(-1);

            transactionDate.setStartDateText(startDate.ToString("dd MMM yyyy"));
            transactionDate.setEndDateText(startDate.ToString("dd MMM yyyy"));


            //tableTransaction = new TableTransaction();

            _startDate = startDate;
            _endDate = endDate.AddDays(1).AddTicks(-1);

            List<KomlinkTransactionDetail> komlinkTransactions = await getTransactionData(_startDate, _endDate);



            List<KomlinkTransactionDetailItem> komlinkTransactionDetailItems = new List<KomlinkTransactionDetailItem>();
            foreach (KomlinkTransactionDetail item in komlinkTransactions)
            {
                KomlinkTransactionDetailItem kTdI = new KomlinkTransactionDetailItem();



                switch (item.TransactionType.ToString())
                {
                    case "1":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Top Up";
                        }
                        else
                        {
                            kTdI.TransactionType = "Top Up";
                        }
                        break;
                    case "2":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Pengambilan";
                        }
                        else
                        {
                            kTdI.TransactionType = "Refund";
                        }
                        break;
                    case "3":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Pengunaan";
                        }
                        else
                        {
                            kTdI.TransactionType = "Usage";
                        }
                        break;
                    case "4":
                        break;
                    case "5":
                        break;

                    case "6":
                        break;
                    default:
                        break;


                }

                kTdI.Date = item.TransactionDateTime.Date.ToString("dd/MM/yyyy");
                kTdI.Time = item.TransactionDateTime.TimeOfDay.ToString(@"hh\:mm");
                kTdI.Station = item.Station;
                kTdI.TicketType = item.TicketType;
                kTdI.TotalAmount = item.TotalAmount.ToString("C2").Replace("$", "RM");

                komlinkTransactionDetailItems.Add(kTdI);
            }



            tableTransaction.AddData(komlinkTransactionDetailItems);

            PaymentOptionOrKeypad.Children.Clear();
            PaymentOptionOrKeypad.Children?.Add(tableTransaction);
        }
        private async Task<List<KomlinkTransactionDetail>> getTransactionData(DateTime startDate, DateTime endDate)
        {

            //KomlinkCardDetailModel data = ScreenNavigation.KomlinkCardDetails;

            //APIServices aPIServices = new APIServices(new APIURLServices(), new APISignatureServices());
            //var result = aPIServices.GetKomlinkTransactionDetail().Result;



            //return result?.Data;

            List<KomlinkTransactionDetail> resultModel = null;

            try
            {

                string komlinkCard = UserSession.CurrentUserSession.CardData;

                //string info = "{\"CSN\":\"04097902DB\",\"KVDTCardNo\":227182759798068397,\"IsCardCancelled\":false,\"IsCardBlacklisted\":false,\"IsSP1Available\":true,\"IsSP2Available\":true,\"IsSP3Available\":false,\"IsSP4Available\":false,\"IsSP5Available\":false,\"IsSP6Available\":false,\"IsSP7Available\":false,\"IsSP8Available\":false,\"ChkInGateNo\":\"\",\"ChkInDatetime\":\"2001-01-01T00:00:00\",\"ChkInStationNo\":\"\",\"ChkOutGateNo\":\"\",\"ChkOutDatetime\":\"2001-01-01T00:00:00\",\"ChkOutStationNo\":\"\",\"MainPurse\":21639268,\"MainTransNo\":65000,\"IssuerSAMId\":\"0CA51022F7CC92DD\",\"Gender\":\"M\",\"CardIssuedDate\":\"2043-06-05T00:00:00\",\"CardExpireDate\":\"2001-01-01T00:00:00\",\"PNR\":\"PNR-E7B07F86\",\"CardType\":\"oku\",\"CardTypeExpireDate\":\"2023-07-28T00:00:00\",\"IsMalaysian\":false,\"DOB\":\"2001-06-22T00:00:00\",\"LRCKey\":182,\"IDType\":\"PassportNo\",\"IDNo\":\"980521595019345\",\"PassengerName\":\"LEONG WAI LIEMmm\",\"BLKStartDatetime\":\"2099-12-31T00:00:00\",\"RefillSAMId\":\"0CA51022F7CC92DD\",\"RefillDatetime\":\"2023-06-05T13:47:26\",\"LastTransDatetime\":\"2023-06-08T09:07:20\",\"BackupPurse\":21639268,\"BackupTransNo\":65000,\"BLKSAMId\":\"0000000000000000\",\"BLKDatetime\":\"2001-01-01T00:00:00\",\"KomLinkSAMId\":\"0CA51022F7CC92DD\",\"MerchantNameAddr\":null,\"TransactionDateTime\":null,\"KomLinkSeasonPassDatas\":[{\"SPNo\":1,\"SPSaleDate\":\"2023-06-07T00:00:00\",\"SPMaxTravelAmtPDayPTrip\":0.1,\"SPIssuerSAMId\":\"0CA51022F7CC92DD\",\"SPStartDate\":\"2023-06-14T00:00:00\",\"SPEndDate\":\"2023-06-20T00:00:00\",\"SPSaleDocNo\":\"KCT230608353297 \",\"SPServiceCode\":\"KV \",\"SPPackageCode\":\"KVAD\",\"SPType\":\"KVWP\",\"SPMaxTripCountPDay\":10,\"SPIsAvoidChecking\":false,\"SPIsAvoidTripDurationCheck\":false,\"SPOriginStationNo\":\"50600 \",\"SPDestinationStationNo\":\"54500 \",\"SPLastTravelDate\":\"2001-01-01T00:00:00\",\"SPDailyTravelTripCount\":0},{\"SPNo\":2,\"SPSaleDate\":\"2023-06-03T00:00:00\",\"SPMaxTravelAmtPDayPTrip\":1,\"SPIssuerSAMId\":\"0CA51022F7CC92DD\",\"SPStartDate\":\"2023-06-03T00:00:00\",\"SPEndDate\":\"2023-09-13T00:00:00\",\"SPSaleDocNo\":\"SP-DOC-111156789\",\"SPServiceCode\":\"KVDT\",\"SPPackageCode\":\"PK01\",\"SPType\":\"WEEK\",\"SPMaxTripCountPDay\":8,\"SPIsAvoidChecking\":false,\"SPIsAvoidTripDurationCheck\":false,\"SPOriginStationNo\":\"19000 \",\"SPDestinationStationNo\":\"19700 \",\"SPLastTravelDate\":\"2023-06-06T00:00:00\",\"SPDailyTravelTripCount\":1}]}";


                KomlinkCardTransactionRequesModel komlinkCardTransactionRequesModel = new KomlinkCardTransactionRequesModel();
                komlinkCardTransactionRequesModel.DateFrom = startDate;
                komlinkCardTransactionRequesModel.DateTo = endDate;



                komlinkCardTransactionRequesModel.KomlinkCardDetails = komlinkCard;

                APIServices aPIServices = new APIServices(new APIURLServices(), new APISignatureServices());

                List<KomlinkTransactionDetail> result = await aPIServices.GetKomlinkCardTransaction(komlinkCardTransactionRequesModel);



                resultModel = result;

            }
            catch (Exception ex)
            {

            }

            return resultModel;

        }
        private void ButtonTngClicked(object sender, EventArgs e)
        {
            if (CheckTextBox())
            {
                UserSession.TotalTicketPrice = decimal.Parse(paymentAmountOption.GetText());
                UserSession.PaymentMethod = FinancePaymentMethod.TngEWallet;
                RequestToUpBooking();
                isPaymentGateway = true;
                paymentMethodChoosed = "touchngo_offline";
                paymentMethodTypeChoosed = "TngEwallet";
                paymentGatewayLogoUrl = "https://ktmbstorage.blob.core.windows.net/ktmb-tvm-live-file/TNG_20210723.jpg";
            }
        }

        private bool CheckTextBox()
        {
            if (paymentAmountOption.GetText() == "")
            {

                if (App.Language == "ms")
                {
                    _alertInfo.SetTextAlert("Sila masukkan jumlah tambah nilai");
                }
                else
                {
                    _alertInfo.SetTextAlert("Please enter amount to top up");
                }


                cardPayWave.ClearEvents();

                FrmSubFrame.Content = null;
                FrmSubFrame.NavigationService.RemoveBackEntry();
                FrmSubFrame.NavigationService.Content = null;

                System.Windows.Forms.Application.DoEvents();

                FrmSubFrame.NavigationService.Navigate(_alertInfo);
                BdSubFrame.Visibility = Visibility.Visible;
                System.Windows.Forms.Application.DoEvents();


                return false;
            }
            else
                return true;
        }
        private void ButtonBTnGClickded(object sender, EventArgs e)
        {
            if (CheckTextBox())
            {
                UserSession.TotalTicketPrice = decimal.Parse(paymentAmountOption.GetText());

                UserSession.PaymentMethod = FinancePaymentMethod.Boost;
                RequestToUpBooking();
                isPaymentGateway = true;
                paymentMethodChoosed = "boost";
                paymentMethodTypeChoosed = "Boost";
                paymentGatewayLogoUrl = "https://ktmbstorage.blob.core.windows.net/ktmb-tvm-live-file/Boost_20210723.jpg";
            }
        }

        private void StartPaymentGateWay()
        {
            bTnGPayment.StartBTnGPayment(UserSession.MCurrenciesId,
                UserSession.TotalTicketPrice,
                UserSession.requestAddTopUpModel.TransactionNo,
                paymentMethodChoosed,
                "komlink-card",
                "komlink-card",
                "1234567890",
                UserSession.requestAddTopUpModel.KomlnkPurchaseHeaders_Id,
                paymentGatewayLogoUrl,
                paymentMethodTypeChoosed,
                langRec,
                LanguageCode.English);
        }
        private void ButtonPayWaveClicked(object sender, EventArgs e)
        {

            if (CheckTextBox())
            {
                UserSession.PaymentMethod = FinancePaymentMethod.CreditCard;
                isPaymentGateway = false;

                UserSession.TotalTicketPrice = decimal.Parse(paymentAmountOption.GetText());
                if (App.isBypassPayment)
                {

                    RequestToUpBooking();
                }
                else
                {


                    RequestToUpBooking();
                }
            }

        }


        private void startCardPayWave()
        {
            cardPayWave.ClearEvents();
            cardPayWave.InitPaymentData(UserSession.MCurrenciesId, UserSession.TotalTicketPrice, UserSession.SessionId, App.PayWaveCOMPORT, null);
            cardPayWave.OnEndPayment += CardPayWave_OnEndPayment;
            FrmSubFrame.Content = null;
            FrmSubFrame.NavigationService.RemoveBackEntry();
            FrmSubFrame.NavigationService.Content = null;
            System.Windows.Forms.Application.DoEvents();

            FrmSubFrame.NavigationService.Navigate(cardPayWave);
            BdSubFrame.Visibility = Visibility.Visible;
            System.Windows.Forms.Application.DoEvents();
        }



        private async void RequestToUpBooking()
        {
            try
            {
                var apiService = new APIServices(new APIURLServices(), new APISignatureServices());

                KomlinkCardAddTopUpRequestModel komlinkRequestModel = new KomlinkCardAddTopUpRequestModel();
                komlinkRequestModel.KomlinKCardNo = UserSession.CurrentUserSession.CardNo;
                komlinkRequestModel.PurchaseCounterId = UserSession.MCounters_Id;
                komlinkRequestModel.MCurrenciesId = UserSession.MCurrenciesId;
                komlinkRequestModel.PurchaseStationId = UserSession.PurchaseStationId;
                komlinkRequestModel.Amount = UserSession.TotalTicketPrice;

                var result = await apiService.AddTopUp(komlinkRequestModel);

                if (result != null)
                {
                    UserSession.requestAddTopUpModel = result;

                    //if (isPaymentGateway)
                    //{
                    //    StartPaymentGateWay();
                    //}
                    //else
                    //{
                    //    startCardPayWave();
                    //}
                    if (isPaymentGateway)
                    {
                        StartPaymentGateWay();
                    }
                    else
                    {
                        startCardPayWave();
                    }
                   
                }
            }
            catch (Exception ex)
            {

            }

            //if (UserSession.PaymentMethod == FinancePaymentMethod.CreditCard)
            //{
            //    //if (App.isBypassPayment)
            //    //{
            //    //    CheckOutTopUp();
            //    //}
            //}


        }

        private void CardPayWave_OnEndPayment(object sender, kvdt_kiosk.Views.Payment.Komlink.PayWave.EndOfPaymentEventArgs e)
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

                        this.Dispatcher.Invoke(new Action(() =>
                        {

                        }));


                        //call api to make payment
                        CheckOutTopUp();

                    }

                    else if ((e.ResultState == PaymentResult.Cancel) || (e.ResultState == PaymentResult.Fail))
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            cardPayWave.ClearEvents();
                            FrmSubFrame.Content = null;
                            FrmSubFrame.NavigationService.RemoveBackEntry();
                            FrmSubFrame.NavigationService.Content = null;
                            BdSubFrame.Visibility = Visibility.Collapsed;

                            RequestCancelTopUp();

                        }));
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

        private void ValidatePrinterStatus()
        {
            try
            {

                try
                {
                    App.AppHelp.PrinterApp.GetPrinterStatus(out isPrinterError, out isPrinterWarning, out statusDescription, out locStateDesc);
                }
                catch (Exception ex)
                {
                    Log.Error("TVM-Komlink", "-", ex, "EX02", "pgKomuter.StartCreditCardPayWavePayment");
                    throw new Exception($@"{ex.Message}; (EXIT10000921); {AppHelper.PrinterErrorTag}");
                }

                if (isPrinterError || isPrinterWarning)
                {
                    StatusHub.GetStatusHub().zNewStatus_IsPrinterStandBy(NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, statusDescription);

                    if (string.IsNullOrWhiteSpace(statusDescription))
                        statusDescription = "Printer Error (X01); (EXIT10000922)";

                    Log.Information("TVM-Komlink", "-", $@"Error; (EXIT10000923); {statusDescription}", "A11", "pgKomuter.StartCreditCardPayWavePayment", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
                    throw new Exception($@"{statusDescription}; (EXIT10000923); {AppHelper.PrinterErrorTag}");
                }
                else
                {
                    StatusHub.GetStatusHub().zNewStatus_IsPrinterStandBy(NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Printer Standing By");
                }

                if (NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting().DisablePrinterTracking == false)
                {
                    if (string.IsNullOrWhiteSpace(App.SysParam.FinalizedPrinterName) == true)
                    {
                        Log.Information("TVM-Komlink", "-", $@"Error; (EXIT10000924); Default (or correct) printer not found", "A11", "pgKomuter.StartCreditCardPayWavePayment", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
                        throw new Exception($@"{statusDescription}; (EXIT10000924); Default (or correct) printer not found; {AppHelper.PrinterErrorTag}");
                    }
                }
            }
            catch
            {

            }
        }

        public async void CheckOutTopUp()
        {
            try
            {
                if (UserSession.requestAddTopUpModel != null)
                {
                    var apiService = new APIServices(new APIURLServices(), new APISignatureServices());

                    KomlinkCardCheckoutTopupRequestModel checkOutModel = new KomlinkCardCheckoutTopupRequestModel();
                    checkOutModel.KomlinkPurchaseHeaders_Id = UserSession.requestAddTopUpModel.KomlnkPurchaseHeaders_Id;
                    checkOutModel.Currency_Id = UserSession.MCurrenciesId;
                    checkOutModel.CardId = UserSession.CurrentUserSession.CardNo;
                    checkOutModel.MCounters_Id = UserSession.MCounters_Id;
                    checkOutModel.TotalAmount = UserSession.TotalTicketPrice;
                    checkOutModel.Channel = "TVM";
                    checkOutModel.KomlinkCardNo = UserSession.CurrentUserSession.CardNo;
                    checkOutModel.CounterUserId = UserSession.MCounters_Id;
                    checkOutModel.PurchaseStationId = UserSession.PurchaseStationId;
                    checkOutModel.FinancePaymentMethod = UserSession.PaymentMethod;
                    checkOutModel.ReferenceNo = "";
                    checkOutModel.InvoiceNo = "";
                    checkOutModel.HostNo = "";

                    if (_lastCreditCardAnswer != null)
                    {
                        checkOutModel.CreditCardResponseModel = _lastCreditCardAnswer;
                    }
                    else
                    {
                        checkOutModel.CreditCardResponseModel = null;
                    }

                    if (UserSession.PaymentMethod.Equals(FinancePaymentMethod.EWallet))
                    {
                        PaymentGatewaySuccessPaidModel ewalletPayment = new PaymentGatewaySuccessPaidModel();
                        ewalletPayment.SalesTransactionNo = UserSession.PaymentGateWaySalesTransactionNo;

                        checkOutModel.PaymentGatewaySuccessPaidModel = ewalletPayment;
                    }
                    else
                    {
                        checkOutModel.PaymentGatewaySuccessPaidModel = null;

                    }


                    var result = await apiService.CompleteTopUp(checkOutModel);

                    if (result != null)
                    {
                        if (result.Status == false)
                        {
                            _printingThreadWorker = new Thread(new ThreadStart(PrintErrorThreadWorking));
                            _printingThreadWorker.IsBackground = true;
                            _printingThreadWorker.Start();
                        }
                        else
                        {
                            //write to the card

                            string json = JsonConvert.SerializeObject(result.Data, Formatting.Indented);
                            CompleteTopUpKomlinkCardCCompiledResultModel res = JsonConvert.DeserializeObject<CompleteTopUpKomlinkCardCCompiledResultModel>(json);
                            UserSession.requestAddTopUpModel.komlinkCardReceiptGetModel = res.komlinkCardReceiptGetModel;




                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                //cardPayWave.ClearEvents();
                                //FrmSubFrame.Content = null;
                                //FrmSubFrame.NavigationService.RemoveBackEntry();
                                //FrmSubFrame.NavigationService.Content = null;
                                //BdSubFrame.Visibility = Visibility.Collapsed;


                                //UserSession.TotalTicketPrice = decimal.Parse(paymentAmountOption.GetText());

                                cardPayWave.ClearEvents();

                                FrmSubFrame.Content = null;
                                FrmSubFrame.NavigationService.RemoveBackEntry();
                                FrmSubFrame.NavigationService.Content = null;

                                System.Windows.Forms.Application.DoEvents();

                                FrmSubFrame.NavigationService.Navigate(alertScan);
                                BdSubFrame.Visibility = Visibility.Visible;
                                System.Windows.Forms.Application.DoEvents();
                            }));

                        }
                    }

                }
            }
            catch (Exception ex)
            {

            }
        }


        void PrintErrorThreadWorking()
        {
           
        }


        private async void RequestUpdateWriteCardStatus()
        {
            try
            {
                if (UserSession.requestAddTopUpModel != null)
                {
                    var apiService = new APIServices(new APIURLServices(), new APISignatureServices());

                    KomlinkCardCancelTopupRequestModel writeStatus = new KomlinkCardCancelTopupRequestModel();
                    writeStatus.KomlinkPurchaseHeaders_Id = UserSession.requestAddTopUpModel.KomlnkPurchaseHeaders_Id;
                    writeStatus.CounterUserId = UserSession.MCounters_Id;

                    await apiService.UpdateWriteStatus(writeStatus);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public async void RequestCancelTopUp()
        {
            try
            {
                if (UserSession.requestAddTopUpModel != null)
                {
                    var apiService = new APIServices(new APIURLServices(), new APISignatureServices());

                    KomlinkCardCancelTopupRequestModel cancel = new KomlinkCardCancelTopupRequestModel();
                    cancel.KomlinkPurchaseHeaders_Id = UserSession.requestAddTopUpModel.KomlnkPurchaseHeaders_Id;
                    cancel.CounterUserId = UserSession.MCounters_Id;

                    await apiService.CancelTopUp(cancel);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void BTnGShowPaymentInfo(IKioskMsg kioskMsg)
        {
            bTnGPayment.BTnGShowPaymentInfo(kioskMsg);
        }

        //private void UserControl_Loaded(object sender, RoutedEventArgs e)
        //{
        //    while (!isIm30Ready)
        //    {
        //        isIm30Ready = cardPayWave.GetIm30Status();
        //    }

        //}
    }
}
