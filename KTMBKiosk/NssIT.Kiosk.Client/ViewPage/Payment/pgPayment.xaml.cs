using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using System;
using System.Collections.Generic;
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
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.Client.Reports;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.Client.Base;
using NssIT.Train.Kiosk.Common.Data.Response;  
using NssIT.Train.Kiosk.Common.Data;
using NssIT.Kiosk.Client.Reports.KTMB;
using System.IO;
using System.Data;
using Microsoft.Reporting.WinForms;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.Tools.ThreadMonitor;
using System.Web;
using NssIT.Train.Kiosk.Common.Constants;
using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;

namespace NssIT.Kiosk.Client.ViewPage.Payment
{
    public delegate void ResetCurrentPaymentTypeDelg();
    public delegate bool SetCurrentPaymentTypeDelg(PaymentType paymentType);

    /// <summary>
    /// Interaction logic for pgPayment.xaml
    /// </summary>
    public partial class pgPayment : Page, IPayment
    {
        private string _logChannel = "Payment";
        public const string CreditCardPayment = "CREDIT_CARD";

        //private string _terminalLogoPath = @"file://C:\dev\RND\MyRnD\WFmRdlcReport5\bin\Debug\Resource\MelSenLogo.jpg";
        //private string _terminalVerticalLogoPath = @"file://C:\dev\RND\MyRnD\WFmRdlcReport5\bin\Debug\Resource\MelSenLogo_Vertical.jpg";

        //private pgPaymentInfo _paymentInfoPage = null;
        private pgPaymentInfo2 _paymentInfoPage = null;

        //pgCreditCardPayWave
        private pgCreditCardPayWaveV2 _cardPayPage = null;
        private pgPayment_BTnGPayment _bTnGPaymentStaff = null;

        private pgPrintTicket2 _printTicketPage = null;

        //private decimal _departTotalPricePerTicket = 0M;
        private string _currency = "RM";
        private decimal _totalAmount = 0M;
        private string _seatBookingId = "";
        private string _ktmbSalesTransactionNo = "";

        private string _firstName = "";
        private string _lastName = "";
        private string _contactNo = "";

        private CreditCardResponse _lastCreditCardAnswer = null;
        private bool _alreadyExitPaymentFlag = false;
        private object _alreadyExitPaymentLock = new object();

        private Thread _printingThreadWorker = null;

        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        private pgPaymentInfo2.StartCreditCardPayWavePaymentDelg _startCreditCardPayWavePaymentDelHandle = null;
        private uscPaymentGateway.StartBTngPaymentDelg _startBTngPaymentDelHandle = null;
        private ResetCurrentPaymentTypeDelg _resetCurrentPaymentTypeDelgHandle = null;

        private PaymentType _lastSelectedTypeOfPayment = PaymentType.Unknown;
        private string _currentSelectedPaymentMethod = null;
        private object _selectedPaymentMethodLock = new object();

        public pgPayment()
        {
            InitializeComponent();

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\Payment\rosPaymentMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\Payment\rosPaymentEnglish.xaml");

            _printTicketPage = new pgPrintTicket2();
            _paymentInfoPage = new pgPaymentInfo2();

            _startCreditCardPayWavePaymentDelHandle = new pgPaymentInfo2.StartCreditCardPayWavePaymentDelg(StartCreditCardPayWavePayment);
            _startBTngPaymentDelHandle = new uscPaymentGateway.StartBTngPaymentDelg(StartBTnGPayment);
            _resetCurrentPaymentTypeDelgHandle = new ResetCurrentPaymentTypeDelg(DoResetCurrentPaymentType);

            _cardPayPage = pgCreditCardPayWaveV2.GetCreditCardPayWavePage();

            _bTnGPaymentStaff = new pgPayment_BTnGPayment(this, FrmGoPay, BdPay, FrmPayInfo, _printTicketPage, _resetCurrentPaymentTypeDelgHandle);

            _printTicketPage.OnDoneClick += _printTicketPage_OnDoneClick;
            _printTicketPage.OnPauseClick += _printTicketPage_OnPauseClick;
        }

        private string TerminalLogoPath
        {
            get => $@"file://{App.ExecutionFolderPath}\Resources\MelSenLogo.jpg";
        }

        private string BCImagePathPath
        {
            get => $@"file://{App.ExecutionFolderPath}\Resources\BC.gif";
        }

        private string TerminalVerticalLogoPath
        {
            get => $@"file://{App.ExecutionFolderPath}\Resources\KTMB_Logo01.png";
        }

        private string TicketReportSourceName
        {
            get
            {
                return "RPTIntercityETS";
            }
        }

        private string CreditCardReceiptReportSourceName
        {
            get
            {
                return "RPTCreditCardReceipt";
            }
        }

        private string TicketErrorReportSourceName
        {
            get
            {
                return "TicketKTMBErrorMessage";
            }
        }

        private void _printTicketPage_OnDoneClick(object sender, EventArgs e)
        {
            try
            {
                // ClearPrintingThread();
                //App.MainScreenControl.ShowWelcome();

                Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
                    try
                    {
                        lock (_alreadyExitPaymentLock)
                        {
                            if (_alreadyExitPaymentFlag == false)
                            {
                                //App.NetClientSvc.SalesService.NavigateToPage(PageNavigateDirection.Exit);
                                App.MainScreenControl.ShowWelcome();
                                _alreadyExitPaymentFlag = true;
                                App.ShowDebugMsg("pgPayment._printTicketPage_OnDoneClick");
                            }                            
                        }                        
                    }
                    catch (Exception ex)
                    {
                        App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000915)");
                        App.Log.LogError(_logChannel, "", new Exception("(EXIT10000915)", ex), "EX01", "pgPayment._printTicketPage_OnDoneClick");
                    }
                })));
                submitWorker.IsBackground = true;
                submitWorker.Start();

            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgPayment._printTicketPage_OnDoneClick");
            }
        }

        private bool _isPauseOnPrinting = false;
        private void _printTicketPage_OnPauseClick(object sender, EventArgs e)
        {
            try
            {
                _isPauseOnPrinting = true;
                //App.NetClientSvc.SalesService.PauseCountDown(out bool isServerResponded);
                SubmitPause();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgPayment._printTicketPage_OnPauseClick");
            }
        }

        private void SubmitPause()
        {
            System.Windows.Forms.Application.DoEvents();

            Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
                try
                {
                    App.NetClientSvc.SalesService.PauseCountDown(out bool isServerResponded);

                    //if (isServerResponded == false)
                    //    App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000209)");
                }
                catch (Exception ex)
                {
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000910)");
                    App.Log.LogError(_logChannel, "", new Exception("(EXIT10000910)", ex), "EX01", "pgPayment._printTicketPage_OnPauseClick");
                }
            })));
            submitWorker.IsBackground = true;
            submitWorker.Start();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _isPauseOnPrinting = false;

                ClearPrintingThread();

                this.Resources.MergedDictionaries.Clear();

                if (_language == LanguageCode.Malay)
                    this.Resources.MergedDictionaries.Add(_langMal);
                else
                    this.Resources.MergedDictionaries.Add(_langEng);

                BdPay.Visibility = Visibility.Collapsed;

                FrmPayInfo.Content = null;
                FrmPayInfo.NavigationService.RemoveBackEntry();
                FrmGoPay.Content = null;
                FrmGoPay.NavigationService.RemoveBackEntry();
                FrmGoPay.NavigationService.Content = null;
                BdPay.Visibility = Visibility.Collapsed;

                ///// Mandatory Reset to _currentSelectedTypeOfPayment
                _currentSelectedPaymentMethod = null;
                /////-------------------------------------------------

                System.Windows.Forms.Application.DoEvents();

                FrmPayInfo.NavigationService.Navigate(_paymentInfoPage);

                System.Windows.Forms.Application.DoEvents();

                _pageLoaded = true;
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgPayment.Page_Loaded");
            }
        }

        private bool _pageLoaded = false;
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _pageLoaded = false;

                BdPay.Visibility = Visibility.Collapsed;

                _cardPayPage.ClearEvents();
                _bTnGPaymentStaff.PageUnloaded();
                FrmPayInfo.Content = null;
                FrmPayInfo.NavigationService.RemoveBackEntry();
                FrmPayInfo.NavigationService.Content = null;

                FrmGoPay.Content = null;
                FrmGoPay.NavigationService.RemoveBackEntry();
                FrmGoPay.NavigationService.Content = null;
                ///// Mandatory Reset to _currentSelectedTypeOfPayment
                _currentSelectedPaymentMethod = null;
                /////-------------------------------------------------

                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgPayment.Page_Unloaded");
            }
        }


        private Thread _endPaymentThreadWorker = null;
        private void _creditCardPayWavePage_OnEndPayment(object sender, EndOfPaymentEventArgs e)
        {
            //App.MainScreenControl.MainFormDispatcher.Invoke(new Action(() => {
            //    App.MainScreenControl.ExecMenu.HideMiniNavigator();
            //}));

            string bankRefNo = e.BankReferenceNo;

            //CYA-DEMO
            if (App.SysParam.PrmNoPaymentNeed)
                bankRefNo = DateTime.Now.ToString("MMddHHmmss");

            try
            {
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
                App.Log.LogError(_logChannel, "-", ex, "EX02", "pgPayment._cashPaymentPage_OnEndPayment");
            }

            void OnEndPaymentThreadWorking()
            {
                try
                {
                    App.ShowDebugMsg("_cashPaymentPage_OnEndPayment.. go to ticket printing");

                    if (e.ResultState == AppDecorator.Common.PaymentResult.Success)
                    {
                        App.BookingTimeoutMan.ResetCounter();

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
                                camt = _totalAmount,
                                cdnm = "",
                                cdno = "1234567812345678",
                                cdty = "01",
                                erms = "",
                                hsno = "01",
                                mcid = App.AppHelp.MachineID ?? "",
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
                        

                        // Complete Transaction Then Print Ticket
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            FrmPayInfo.Content = null;
                            FrmPayInfo.NavigationService.RemoveBackEntry();
                            System.Windows.Forms.Application.DoEvents();

                            _printTicketPage.InitSuccessPaymentCompleted(_ktmbSalesTransactionNo, _language);

                            _cardPayPage.ClearEvents();
                            //FrmGoPay.NavigationService.Navigate(_printTicketPage);
                            FrmGoPay.Content = null;
                            FrmGoPay.NavigationService.RemoveBackEntry();
                            FrmGoPay.NavigationService.Content = null;
                            
                            BdPay.Visibility = Visibility.Collapsed;

                            FrmPayInfo.NavigationService.Navigate(_printTicketPage);

                            //App.MainScreenControl.ExecMenu.HideMiniNavigator();
                            System.Windows.Forms.Application.DoEvents();
                        }));

                        DoResetCurrentPaymentType();
                        /////CYA-TEST -------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                        //_lastCreditCardAnswer = null;
                        //_lastSelectedTypeOfPayment = PaymentType.PaymentGateway;
                        //App.NetClientSvc.SalesService.SubmitSalesPayment(_seatBookingId, _totalAmount, _currency, $@"TestBTnGNo_{DateTime.Now.ToString("HHmmss")}", out bool isServerResponded);
                        /////-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                        App.NetClientSvc.SalesService.SubmitSalesPayment(_seatBookingId, _totalAmount, _currency, bankRefNo, _lastCreditCardAnswer, out bool isServerResponded);

                        //DEBUG-Testing .. isServerResponded = false;

                        if (isServerResponded == false) 
                        {
                            _printTicketPage.UpdateCompleteTransactionState(isTransactionSuccess: false, language: _language);

                            string probMsg = "Local Server not responding (EXIT10000914)";
                            probMsg = $@"{probMsg}; Transaction No.:{_ktmbSalesTransactionNo}";

                            App.Log.LogError(_logChannel, _ktmbSalesTransactionNo, new Exception(probMsg), "EX01", "pgPayment.OnEndPaymentThreadWorking");

                            _printingThreadWorker = new Thread(new ThreadStart(PrintErrorThreadWorking));
                            _printingThreadWorker.IsBackground = true;
                            _printingThreadWorker.Start();
                            //PrintTicketError(_transactionNo);
                            //App.ShowDebugMsg("Print Sales Receipt on Fail Completed Transaction ..; pgPayment.OnEndPaymentThreadWorking");
                            //App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000912)");
                        }
                    }
                    else if ((e.ResultState == AppDecorator.Common.PaymentResult.Cancel) || (e.ResultState == AppDecorator.Common.PaymentResult.Fail))
                    {
                        App.TimeoutManager.ResetTimeout();

                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            _cardPayPage.ClearEvents();
                            FrmGoPay.Content = null;
                            FrmGoPay.NavigationService.RemoveBackEntry();
                            FrmGoPay.NavigationService.Content = null;
                            DoResetCurrentPaymentType();
                            BdPay.Visibility = Visibility.Collapsed;

                            System.Windows.Forms.Application.DoEvents();
                        }));
                    }
                    else
                    {
                        App.BookingTimeoutMan.ResetCounter();

                        // Below used to handle result like ..
                        //------------------------------------------
                        // AppDecorator.Common.PaymentResult.Fail
                        // AppDecorator.Common.PaymentResult.Timeout
                        // AppDecorator.Common.PaymentResult.Unknown

                        App.NetClientSvc.SalesService.RequestSeatRelease(_seatBookingId);
                        //if (isServerResponded == false)
                        //    App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000913)");

                        //if (_isPauseOnPrinting == false)
                        App.MainScreenControl.ShowWelcome();
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; EX02;pgPayment.OnEndPaymentThreadWorking");
                    App.Log.LogError(_logChannel, "-", ex, "EX02", "pgPayment.OnEndPaymentThreadWorking");
                }
                finally
                {
                    _endPaymentThreadWorker = null;
                }
            }

            void PrintErrorThreadWorking()
            {
                try
                {
                    PrintTicketError2(_ktmbSalesTransactionNo);
                    App.ShowDebugMsg("Print Sales Receipt on Fail Completed Transaction ..; pgPayment.OnEndPaymentThreadWorking");

                    // App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000912)");
                }
                catch (ThreadAbortException) 
                {
                    //PDFTools.KillAdobe("AcroRd32");
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; EX02;pgPayment.PrintErrorThreadWorking");
                    App.Log.LogError(_logChannel, "-", ex, "EX03", "pgPayment.PrintErrorThreadWorking");
                }
            }
        }

        public void InitPayment(UserSession session)
        {
            try
            {
                ClearPrintingThread();

                _language = session.Language;

                _currency = string.IsNullOrWhiteSpace(session.TradeCurrency) ? "RM" : session.TradeCurrency.Trim();
                
                _alreadyExitPaymentFlag = false;
                _lastCreditCardAnswer = null;
                _totalAmount = session.GrossTotal;
                _seatBookingId = session.SeatBookingId;
                _ktmbSalesTransactionNo = session.KtmbSalesTransactionNo;

                _firstName = session.DepartPassengerSeatDetailList[0].CustName;
                _lastName = session.DepartPassengerSeatDetailList[0].CustName;
                _contactNo = session.DepartPassengerSeatDetailList[0].Contact;

                _firstName = string.IsNullOrWhiteSpace(_firstName) ? "-First Name-" : _firstName;
                _lastName = string.IsNullOrWhiteSpace(_lastName) ? "-First Name-" : _lastName;
                _contactNo = string.IsNullOrWhiteSpace(_contactNo) ? "-First Name-" : _contactNo;
                
                _paymentInfoPage.InitPaymentInfo(session, _startCreditCardPayWavePaymentDelHandle, _startBTngPaymentDelHandle);
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgPayment.InitPayment");
            }
        }

        private void StartCreditCardPayWavePayment()
        {
            try
            {
                if ((DoSetCurrentPaymentType(CreditCardPayment) == false) 
                    || (_currentSelectedPaymentMethod is null) 
                    || (_currentSelectedPaymentMethod?.ToString().Equals(CreditCardPayment, StringComparison.InvariantCultureIgnoreCase) == false))
                {
                    return;
                }

                ResourceDictionary langRec = (_language == LanguageCode.Malay) ? _langMal : _langEng;

                _lastSelectedTypeOfPayment = PaymentType.CreditCard;
                //InitCashPaymentPage
                _cardPayPage.ClearEvents();
                _cardPayPage.InitPaymentData(_currency, _totalAmount, _ktmbSalesTransactionNo, App.SysParam.PrmPayWaveCOM, langRec);
                _cardPayPage.OnEndPayment += _creditCardPayWavePage_OnEndPayment;
                FrmGoPay.Content = null;
                FrmGoPay.NavigationService.RemoveBackEntry();
                FrmGoPay.NavigationService.Content = null;
                System.Windows.Forms.Application.DoEvents();

                App.CurrentTransStage = TicketTransactionStage.ETS;
                FrmGoPay.NavigationService.Navigate(_cardPayPage);
                BdPay.Visibility = Visibility.Visible;
                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgPayment.StartCreditCardPayWavePayment");
            }
        }

        private void StartBTnGPayment(string paymentGateWay, string paymentGatewayLogoUrl, string paymentMethod)
        {
            try
            {
                if ((DoSetCurrentPaymentType(paymentMethod) == false) 
                    || (_currentSelectedPaymentMethod is null)
                    || (_currentSelectedPaymentMethod?.ToString().Equals(paymentMethod, StringComparison.InvariantCultureIgnoreCase) == false))
                {
                    return;
                }

                _lastSelectedTypeOfPayment = PaymentType.PaymentGateway;

                ResourceDictionary langRec = (_language == LanguageCode.Malay) ? _langMal : _langEng;

                _bTnGPaymentStaff.StartBTnGPayment(_currency, _totalAmount, _ktmbSalesTransactionNo, paymentGateWay, _firstName, _lastName, _contactNo, 
                    _seatBookingId, paymentGatewayLogoUrl, paymentMethod,
                    langRec, _language);
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgPayment.StartCreditCardPayWavePayment");
            }
        }

        private void DoResetCurrentPaymentType()
        {
            string hisPayMet = _currentSelectedPaymentMethod;
            RunThreadMan tMan = new RunThreadMan(new Action(() => 
            {
                lock (_selectedPaymentMethodLock)
                {
                    if ((hisPayMet is null) && (_currentSelectedPaymentMethod is null))
                    {  /* bypass */  }
                    else if ((string.IsNullOrWhiteSpace(hisPayMet) == false) && (string.IsNullOrWhiteSpace(_currentSelectedPaymentMethod) == false))
                    {
                        try
                        {
                            if (hisPayMet?.ToString().Equals(_currentSelectedPaymentMethod, StringComparison.InvariantCultureIgnoreCase) == true)
                            {
                                _currentSelectedPaymentMethod = null;
                            }
                        }
                        catch { }
                    }
                }
            }), "pgPayment.DoResetCurrentPaymentType", 3, _logChannel, ThreadPriority.AboveNormal);
            tMan.WaitUntilCompleted();
        }

        private bool DoSetCurrentPaymentType(string paymentMethod)
        {
            bool retVal = false;
            RunThreadMan tMan = new RunThreadMan(new Action(() =>
            {
                lock (_selectedPaymentMethodLock)
                {
                    if (string.IsNullOrWhiteSpace(_currentSelectedPaymentMethod) && (_pageLoaded))
                    {
                        _currentSelectedPaymentMethod = paymentMethod;
                        retVal = true;
                    }
                }
            }), "pgPayment.DoResetCurrentPaymentType", 3, _logChannel, ThreadPriority.AboveNormal);
        
            tMan.WaitUntilCompleted();

            return retVal;
        }

        private void ClearPrintingThread()
        {
            if ((_printingThreadWorker != null) && ((_printingThreadWorker.ThreadState & ThreadState.Stopped) != ThreadState.Stopped))
            {
                try
                {
                    _printingThreadWorker.Abort();
                }
                catch { }
                Task.Delay(300).Wait();
                _printingThreadWorker = null;
            }
        }
                
        public void UpdateTransCompleteStatus(UICompleteTransactionResult uiCompltResult)
        {
            ///// 20201201 - Remark to continue print ticket.
            //if (_pageLoaded == false)
            //    return;

            //////////App.MainScreenControl.MainFormDispatcher.Invoke(new Action(() => {
            //////////    //App.MainScreenControl.ExecMenu.HideMiniNavigator();
            //////////}));

            string machineId = uiCompltResult.MachineId ?? "*#*";

            _printingThreadWorker = new Thread(new ThreadStart(PrintThreadWorking));
            _printingThreadWorker.IsBackground = true;
            _printingThreadWorker.Start();

            void PrintThreadWorking()
            {
                try
                {
                    App.Log.LogText(_logChannel, (uiCompltResult?.Session?.SeatBookingId) ?? "-", uiCompltResult, "A01", "pgPayment.UpdateTransCompleteStatus",
                        extraMsg: "Start - UpdateTransCompleteStatus; MsgObj: UICompleteTransactionResult");

                    bool isProceedToPrintTicket = false;

                    //Fix QR Code
                    if (uiCompltResult.ProcessState == ProcessResult.Success)
                    {
                        if (((PaymentSubmissionResult)uiCompltResult.MessageData)?.Data?.IntercityETSTicketListResult?.Length > 0)
                        {
                            string trnNo = uiCompltResult.Session?.KtmbSalesTransactionNo;
                            if (FixTicketData(trnNo, ((PaymentSubmissionResult)uiCompltResult.MessageData).Data.IntercityETSTicketListResult, out IntercityETSTicketModel[] resultTicketList))
                            {
                                ((PaymentSubmissionResult)uiCompltResult.MessageData).Data.IntercityETSTicketListResult = resultTicketList;
                                isProceedToPrintTicket = true;
                            }
                        }
                    }

                    // Print Ticket
                    if (isProceedToPrintTicket)
                    {
                        _printTicketPage.UpdateCompleteTransactionState(isTransactionSuccess: true, language: _language);

                        //CYA-TEST
                        //PaymentSubmissionResult stt = ReportDataQuery.GetTicketTestData01();
                        //UICompleteTransactionResult uiCompltResult2 = new UICompleteTransactionResult(uiCompltResult.RefNetProcessId, uiCompltResult.ProcessId, DateTime.Now, stt, ProcessResult.Success);
                        //uiCompltResult2.UpdateSession(uiCompltResult.Session);
                        //uiCompltResult = uiCompltResult2;
                        //----------------------------------------------------------------------------------------

                        System.Windows.Forms.Application.DoEvents();
                        App.ShowDebugMsg("Printing Ticket ..; pgPayment.UpdateTransCompleteStatus");

                        App.Log.LogText(_logChannel, _ktmbSalesTransactionNo, "....enter sequence of printing", "A05", classNMethodName: "pgPayment.PrintThreadWorking", messageType: AppDecorator.Log.MessageType.Debug);
                        
                        if (_lastSelectedTypeOfPayment == PaymentType.PaymentGateway)
                            PrintTicket(uiCompltResult, null, _bTnGPaymentStaff.LastBTnGSaleTransNo);
                        else
                            PrintTicket(uiCompltResult, _lastCreditCardAnswer, null);

                        App.Log.LogText(_logChannel, _ktmbSalesTransactionNo, "....exit sequence of printing", "A10", classNMethodName: "pgPayment.PrintThreadWorking", messageType: AppDecorator.Log.MessageType.Debug);
                        
                        ImagePrintingTools.WaitForPrinting(60 * 2);

                        lock (_alreadyExitPaymentLock)
                        {
                            if ((_isPauseOnPrinting == false) && (_alreadyExitPaymentFlag == false))
                            {
                                App.Log.LogText(_logChannel, _ktmbSalesTransactionNo, $@"isPauseOnPrinting : {_isPauseOnPrinting}; alreadyExitPaymentFlag: {_alreadyExitPaymentFlag}", "A15", classNMethodName: "pgPayment.PrintThreadWorking", messageType: AppDecorator.Log.MessageType.Debug);

                                App.MainScreenControl.ShowWelcome();
                                _alreadyExitPaymentFlag = true;
                            }
                        }
                        
                    }
                    else
                    {
                        _printTicketPage.UpdateCompleteTransactionState(isTransactionSuccess: false, language: _language);

                        System.Windows.Forms.Application.DoEvents();

                        string probMsg = (string.IsNullOrWhiteSpace(uiCompltResult.ErrorMessage) == false) ? uiCompltResult.ErrorMessage : "Fail Complete Payment Transaction";
                        probMsg = $@"{probMsg}; Transation No.:{_ktmbSalesTransactionNo}";

                        App.Log.LogError(_logChannel, _ktmbSalesTransactionNo, new Exception(probMsg), "EX01", "pgPayment.UpdateTransCompleteStatus", adminMsg: "Problem ;" + probMsg);

                        PrintTicketError1(uiCompltResult);
                        App.ShowDebugMsg("Print Sales Receipt on Fail Completed Transaction ..; pgPayment.UpdateTransCompleteStatus");

                        lock (_alreadyExitPaymentLock)
                        {
                            if ((_isPauseOnPrinting == false) && (_alreadyExitPaymentFlag == false))
                            {
                                App.MainScreenControl.ShowWelcome();
                                _alreadyExitPaymentFlag = true;
                            }
                        }
                    }
                }
                catch (ThreadAbortException) 
                {
                    //PDFTools.KillAdobe("AcroRd32");
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"Error: {ex.Message}; pgPayment.PrintThreadWorking");
                    App.Log.LogError(_logChannel, "-", ex, "EX02", "pgPayment.PrintThreadWorking", 
                        adminMsg: $@"Error when printing; Error {ex.Message}");
                    //App.MainScreenControl.Alert(detailMsg: $@"Unable to read Transaction Status; (EXIT10000911)");
                }
            }

            bool FixTicketData(string transactionNo, IntercityETSTicketModel[] ticketList, out IntercityETSTicketModel[] resultTicketList)
            {
                resultTicketList = null;
                bool retVal = false;
                int ticketInx = 0;
                string errorNote = "";

                try
                {
                    foreach (IntercityETSTicketModel tk in ticketList)
                    {
                        ticketInx++;
                        errorNote = $@"Count Inx.: {ticketInx}; Ticket No.: {tk?.TicketNo?.ToString()}";

                        if (string.IsNullOrWhiteSpace(tk.QRLink))
                            throw new Exception($@"QR-Link is BLANK");

                        // Generate QR Code
                        Uri myUri = new Uri(tk.QRLink);
                        string paramT = HttpUtility.ParseQueryString(myUri.Query).Get("T");
                        tk.QRTicketData = QRGen.GetQRCodeData(paramT);
                        //---------------------------------
                        // Check Insurance
                        tk.IsInsuranceAvailable = (tk.IsInsurance?.Equals(YesNo.Yes) == true) ? true : false;
                        //---------------------------------
                    }
                    resultTicketList = ticketList;
                    retVal = true;
                }
                catch (Exception ex)
                {
                    retVal = false;
                    string errMsg = $@"Error when printing; {ex.Message}; Ref Note: {errorNote}";

                    App.Log.LogError(_logChannel, transactionNo, ex, "EX01", "pgPayment.FixTicketQRCodeData",
                        adminMsg: errMsg);
                }
                return retVal;
            }
        }

        public void BTnGShowPaymentInfo(IKioskMsg kioskMsg)
        {
            _bTnGPaymentStaff.BTnGShowPaymentInfo(kioskMsg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uiCompltResult"></param>
        /// <param name="creditCardAnswer">This parameter only used when Credit Card Payment else NULL</param>
        /// <param name="bTnGSaleTransNo">This parameter only used when eWallet Payment else NULL</param>
        private void PrintTicket(UICompleteTransactionResult uiCompltResult, CreditCardResponse creditCardAnswer, string bTnGSaleTransNo)
        {
            UserSession session = uiCompltResult.Session;
            
            Reports.RdlcRendering rpRen = null;

            try
            {
                App.Log.LogText(_logChannel, _ktmbSalesTransactionNo ?? "#?#", "Debug:Print01-Start", "DBG01", classNMethodName: "pgPayment.PrintTicket", messageType: AppDecorator.Log.MessageType.Debug);

                PaymentSubmissionResult transCompStt = (PaymentSubmissionResult)uiCompltResult.MessageData;

                rpRen = new Reports.RdlcRendering(App.ReportPDFFileMan.TicketFolderPath);

                //file://C:\\dev\\RND\\MyRnD\\WFmRdlcReport5\\bin\\Debug\\Resource\\MelSenLogo.jpg
                //file://C:\dev\RND\MyRnD\WFmRdlcReport5\bin\Debug\Resource\MelSenLogo.jpg
                ////TerminalVerticalLogoPath//. TerminalLogoPath
                //DsMelakaCentralTicket ds = ReportDataQuery.ReadToTicketDataSet(transactionNo, transCompStt, TerminalLogoPath, BCImagePathPath, out bool errorFound);

                //if (errorFound == false)
                //{

                DSCreditCardReceipt.DSCreditCardReceiptDataTable dtPaywave = null;

                if (creditCardAnswer != null)
                {
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
                    rw.AmountString = $@"{_currency} {_totalAmount:#,###.00}";
                    rw.MachineId = uiCompltResult.MachineId;
                    rw.RefNumber = _ktmbSalesTransactionNo;
                    dtPaywave.Rows.Add(rw);
                    dtPaywave.AcceptChanges();
                }

                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                ///// Phase 1 Print Solution
                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                //Thread createPDFThreadWorker = new Thread(new ThreadStart(new Action(() => { 
                //    try
                //    {
                //        //// Create Receipt
                //        string resultFileName1 = rpRen.RenderReportFile(Reports.RdlcRendering.RdlcOutputFormat.PotraitTicketPDF1, App.ExecutionFolderPath + @"\Reports\KTMB",
                //            CreditCardReceiptReportSourceName, _salesTransactionNo, "DataSet1", dt, null);

                //        //// Create Ticket
                //        string resultFileName2 = rpRen.RenderReportFile(Reports.RdlcRendering.RdlcOutputFormat.PotraitTicketPDF1, App.ExecutionFolderPath + @"\Reports\KTMB\IntercityETSTicket",
                //            TicketReportSourceName, _salesTransactionNo, "DataSet1", new List<IntercityETSTicketModel>(transCompStt.Data.IntercityETSTicketListResult), null);

                //        App.Log.LogText(_logChannel, _salesTransactionNo, "Start to print ticket", "A02", classNMethodName: "pgPayment.PrintTicket",
                //            adminMsg: "Start to print receipt");

                //        /////string[] pdfFileList = new string[] { resultFileName1, resultFileName2 };
                //        /////PDFTools.PrintPDFs_2(pdfFileList, _salesTransactionNo, App.MainScreenControl.MainFormDispatcher);
                //    }
                //    catch (Exception ex)
                //    {
                //        App.ShowDebugMsg("Error on pgPayment.PrintTicket->createPDFThreadWorker; EX11");
                //        App.Log.LogError(_logChannel, _salesTransactionNo, ex, "EX11", "pgPayment.PrintTicket->createPDFThreadWorker",
                //            adminMsg: $@"Error when printing; {ex.Message}");
                //    }
                //})));
                //createPDFThreadWorker.IsBackground = true;
                //createPDFThreadWorker.Start();

                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                ///// Phase 2 Print Solution
                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                Stream[] streamPaywaveReceiptList = null;
                ReportImageSize paywaveReceiptSize = null;

                App.ShowDebugMsg("pgPayment.PrintTicket ---------- ---------- ----------- ---------- ----------");

                if (dtPaywave != null)
                {
                    App.Log.LogText(_logChannel, _ktmbSalesTransactionNo ?? "#?#", "Debug:Print01-Start Render Data", "DBG05", classNMethodName: "pgPayment.PrintTicket", messageType: AppDecorator.Log.MessageType.Debug);

                    LocalReport receiptRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\KTMB\{CreditCardReceiptReportSourceName}.rdlc",
                        new ReportDataSource[] { new ReportDataSource("DataSet1", (DataTable)dtPaywave) });
                    paywaveReceiptSize = new ReportImageSize(3.2M, 8.2M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
                    streamPaywaveReceiptList = RdlcImageRendering.Export(receiptRep, paywaveReceiptSize);
                }

                ////////////----------------------------------------------
                ////////////----------------------------------------------
                //////////// CYA-PENDING-CODE -- Complain & Follow-up 
                //////////// Complain : (2021-03-19) No ticket printed, but Receipt is printed.
                //////////// Note : The checking block of code (or Complain & Follow-up) should be removed if no problem found anymore. And Leave only "ticks" variable.
                //////////App.Log.LogText(_logChannel, "*", transCompStt?.Data?.IntercityETSTicketListResult, "B01", "pgPayment.PrintTicket",
                //////////        extraMsg: "MsgObj: UICompleteTransactionResult");

                //////////IntercityETSTicketModel[] ticks = new IntercityETSTicketModel[transCompStt.Data.IntercityETSTicketListResult.Length];

                //////////for (int inx = 0; inx < transCompStt.Data.IntercityETSTicketListResult.Length; inx++)
                //////////    ticks[inx] = transCompStt.Data.IntercityETSTicketListResult[inx].Duplicate();

                //////////string logTickNo = "";
                //////////for (int inx = 0; inx < transCompStt.Data.IntercityETSTicketListResult.Length; inx++)
                //////////{
                //////////    logTickNo += $@"//{ticks[inx].BookingNo} : {ticks[inx].TicketNo}";
                //////////}

                //////////App.Log.LogText(_logChannel, "*", $@"logTickNo : {logTickNo}", "B02", "pgPayment.PrintTicket");
                ////////////----------------------------------------------
                ////////////----------------------------------------------
                ////////////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                LocalReport ticketRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\KTMB\IntercityETSTicket\{TicketReportSourceName}.rdlc", 
                    new ReportDataSource[] { new ReportDataSource("DataSet1", new List<IntercityETSTicketModel>(transCompStt.Data.IntercityETSTicketListResult)) });
                ReportImageSize ticketSize = new ReportImageSize(3.2M, 8.2M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
                Stream[] streamticketList = RdlcImageRendering.Export(ticketRep, ticketSize);

                App.Log.LogText(_logChannel, _ktmbSalesTransactionNo ?? "#?#", "Debug:Print01-End Render Data", "DBG10", classNMethodName: "pgPayment.PrintTicket", messageType: AppDecorator.Log.MessageType.Debug);

                ImagePrintingTools.InitService();
                ImagePrintingTools.AddPrintDocument(streamticketList, _ktmbSalesTransactionNo, ticketSize);

                if (streamPaywaveReceiptList != null)
                {
                    ImagePrintingTools.AddPrintDocument(streamPaywaveReceiptList, _ktmbSalesTransactionNo, paywaveReceiptSize);
                }
                
                App.Log.LogText(_logChannel, _ktmbSalesTransactionNo, "Start to print ticket", "A02", classNMethodName: "pgPayment.PrintTicket",
                    adminMsg: "Start to print receipt");

                if (ImagePrintingTools.ExecutePrinting(_ktmbSalesTransactionNo) == false)
                {
                    throw new Exception("Error: Printer Busy; EXP01");
                }
                /////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                App.ShowDebugMsg("Should be printed; pgPayment.PrintTicket");
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgPayment.PrintTicket; EX01");
                App.Log.LogError(_logChannel, _ktmbSalesTransactionNo, ex, "EX01", "pgPayment.PrintTicket",
                    adminMsg: $@"Error when printing; {ex.Message}");
            }
        }

        /// <summary>
        /// Payment Success but finally data tranaction fail in web server.
        /// </summary>
        /// <param name="uiCompltResult"></param>
        private void PrintTicketError1(UICompleteTransactionResult uiCompltResult)
        {
            UserSession session = uiCompltResult.Session;
            string transactionNo = "-";

            Reports.RdlcRendering rpRen = null;

            try
            {
                transactionNo = session.KtmbSalesTransactionNo;
                rpRen = new Reports.RdlcRendering(App.ReportPDFFileMan.TicketFolderPath);

                DsMelakaCentralErrorTicketMessage dsX = ReportDataQuery.GetTicketErrorDataSet(transactionNo, TerminalVerticalLogoPath);

                /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                ///// Phase 1 Print Solution
                //Thread createPDFThreadWorker = new Thread(new ThreadStart(new Action(() => {
                //    try
                //    {
                //        string resultFileName = rpRen.RenderReportFile(Reports.RdlcRendering.RdlcOutputFormat.PotraitTicketErrorPDF1, App.ExecutionFolderPath + @"\Reports\KTMB\IntercityETSTicket",
                //    TicketErrorReportSourceName, transactionNo, "DataSet1", dsX.Tables[0], null);

                //        App.Log.LogText(_logChannel, transactionNo, "Start to print fail transaction notice", "A02", classNMethodName: "pgPayment.PrintTicketError1",
                //                adminMsg: "Start to print fail transaction notice");

                //        //PDFTools.PrintPDFs(resultFileName, transactionNo, App.MainScreenControl.MainFormDispatcher);
                //    }
                //    catch (Exception ex)
                //    {
                //        App.ShowDebugMsg("Error on pgPayment.PrintTicketError1->createPDFThreadWorker; EX11");
                //        App.Log.LogError(_logChannel, _salesTransactionNo, ex, "EX11", "pgPayment.PrintTicketError1->createPDFThreadWorker",
                //            adminMsg: $@"Error when printing; {ex.Message}");
                //    }
                //})));
                //createPDFThreadWorker.IsBackground = true;
                //createPDFThreadWorker.Start();
                
                /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                ///// Phase 2 Print Solution
                ///
                LocalReport receiptRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\KTMB\IntercityETSTicket\{TicketErrorReportSourceName}.rdlc",
                    new ReportDataSource[] { new ReportDataSource("DataSet1", (DataTable)dsX.Tables[0]) });
                ReportImageSize receiptSize = new ReportImageSize(3.2M, 3.2M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
                Stream[] streamReceiptList = RdlcImageRendering.Export(receiptRep, receiptSize);

                ImagePrintingTools.InitService();
                ImagePrintingTools.AddPrintDocument(streamReceiptList, transactionNo, receiptSize);

                App.Log.LogText(_logChannel, transactionNo, "Start to print fail transaction notice", "A02", classNMethodName: "pgPayment.PrintTicketError1",
                        adminMsg: "Start to print fail transaction notice (1)");

                if (ImagePrintingTools.ExecutePrinting($@"{transactionNo}-Error(C)") == false)
                {
                    throw new Exception("Error: Printer Busy; EXP01");
                }
                /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                App.ShowDebugMsg("Should be printed (Error Msg); pgPayment.PrintTicketError1");
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgPayment.PrintTicketError1; EX01");
                App.Log.LogError(_logChannel, transactionNo, ex, "EX01", "pgPayment.PrintTicketError1",
                    adminMsg: $@"Error when printing -fail transaction notice-; {ex.Message}");
            }
        }

        /// <summary>
        /// Payment Success but finally data tranaction fail because local server has no response.
        /// </summary>
        /// <param name="uiCompltResult"></param>
        public void PrintTicketError2(string transactionNo)
        {
            Reports.RdlcRendering rpRen = null;
            try
            {
                rpRen = new Reports.RdlcRendering(App.ReportPDFFileMan.TicketFolderPath);

                DsMelakaCentralErrorTicketMessage dsX = ReportDataQuery.GetTicketErrorDataSet(transactionNo, TerminalVerticalLogoPath);

                /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                ///// Phase 1 Print Solution
                //Thread createPDFThreadWorker = new Thread(new ThreadStart(new Action(() => {
                //    try
                //    {
                        
                //        string resultFileName = rpRen.RenderReportFile(Reports.RdlcRendering.RdlcOutputFormat.PotraitTicketErrorPDF1, App.ExecutionFolderPath + @"\Reports\KTMB\IntercityETSTicket",
                //            TicketErrorReportSourceName, transactionNo, "DataSet1", dsX.Tables[0], null);

                //        App.Log.LogText(_logChannel, transactionNo, "Start to print fail transaction notice", "A02", classNMethodName: "pgPayment.PrintTicketError2",
                //                adminMsg: "Start to print fail transaction notice");

                //        //PDFTools.PrintPDFs(resultFileName, transactionNo, App.MainScreenControl.MainFormDispatcher);
                //    }
                //    catch (Exception ex)
                //    {
                //        App.ShowDebugMsg("Error on pgPayment.PrintTicket->createPDFThreadWorker; EX11");
                //        App.Log.LogError(_logChannel, _salesTransactionNo, ex, "EX11", "pgPayment.PrintTicketError2->createPDFThreadWorker",
                //            adminMsg: $@"Error when printing; {ex.Message}");
                //    }
                //})));
                //createPDFThreadWorker.IsBackground = true;
                //createPDFThreadWorker.Start();
                /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                ///// Phase 2 Print Solution
                ///
                LocalReport receiptRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\KTMB\IntercityETSTicket\{TicketErrorReportSourceName}.rdlc",
                    new ReportDataSource[] { new ReportDataSource("DataSet1", (DataTable)dsX.Tables[0]) });
                ReportImageSize receiptSize = new ReportImageSize(3.2M, 3.2M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
                Stream[] streamReceiptList = RdlcImageRendering.Export(receiptRep, receiptSize);

                ImagePrintingTools.InitService();
                ImagePrintingTools.AddPrintDocument(streamReceiptList, transactionNo, receiptSize);

                App.Log.LogText(_logChannel, transactionNo, "Start to print fail transaction notice", "A02", classNMethodName: "pgPayment.PrintTicketError2",
                        adminMsg: "Start to print fail transaction notice (2)");

                if (ImagePrintingTools.ExecutePrinting($@"{transactionNo}-Error(D)") == false)
                {
                    throw new Exception("Error: Printer Busy; EXP01");
                }
                /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                App.ShowDebugMsg("Should be printed (Error Msg); pgPayment.PrintTicketError2");
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgPayment.PrintTicketError2; EX01");
                App.Log.LogError(_logChannel, transactionNo, ex, "EX01", "pgPayment.PrintTicketError2",
                    adminMsg: $@"Error when printing -fail transaction notice-; {ex.Message}");
            }
        }
    }
}
