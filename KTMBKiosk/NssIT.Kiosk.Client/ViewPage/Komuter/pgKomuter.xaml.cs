using Microsoft.Reporting.WinForms;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Client.Reports;
using NssIT.Kiosk.Client.Reports.KTMB;
using NssIT.Kiosk.Client.ViewPage.Payment;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Log.DB.StatusMonitor;
using NssIT.Kiosk.Tools.ThreadMonitor;
using NssIT.Train.Kiosk.Common.Common;
using NssIT.Train.Kiosk.Common.Constants;
using NssIT.Train.Kiosk.Common.Data;
using NssIT.Train.Kiosk.Common.Data.Response;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NssIT.Kiosk.Client.ViewPage.Komuter
{
    /// <summary>
    /// Interaction logic for pgKomuter.xaml; ClassCode:EXIT80.05
    /// </summary>
    public partial class pgKomuter : Page, IKomuter
    {
        private string _logChannel = "ViewPage";

        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;
        private ResourceDictionary _langPayMal = null;
        private ResourceDictionary _langPayEng = null;
        private ResourceDictionary _currentLanguage = null;
        private ResourceDictionary _currentPayLanguage = null;

        private pgMap _mapPage = null;
        private pgSalesPanel _salesPanelPage = null;
        private pgKomuterPax _paxPage = null;
        private pgKomuterTicketMyKadList _komuterTicketMyKadListPage = null;

        private RouteSelector _routeSelector = null;

        private KomuterSummaryModel _latestTicketTypePackageSummary = null;

        private pgCreditCardPayWaveV2 _cardPayPage = null;
        private pgPrintTicket2 _printTicketPage = null;
        
        private Thread _printingThreadWorker = null;
        private Thread _endPaymentThreadWorker = null;

        private (string Id, string Description, string Duration) _currSelectedPackage = (Id: null, Description: null, Duration: null);

        /// <summary>
        /// Station Id Refer to database
        /// </summary>
        private string _originStationId = "100";
        private string _originStationName = "Butterworth";
        private string _destinationStationId = null;
        private string _destinationStationName = null;

        private bool _pageLoaded = false;
        private int _maxNoOfPaxAllowed = 5;
        private string _currencyId = "RM";
        private TicketSelectionChangedEventArgs _selectedTicketList = null;
        private KomuterBookingCheckoutResult _lastCheckoutResult = null;

        private string _bookingId = null;
        private string _bookingCurrency = null;
        private string _ktmbSalesTransactionNo = null;
        private CreditCardResponse _lastCreditCardAnswer = null;
        private decimal _bookingTotalAmount = 0.0M;
        public DateTime? _bookingExpiredDateTime = null;

        private bool _alreadyExitPaymentFlag = false;
        private object _alreadyExitPaymentLock = new object();

        private string _finalCurrency = null;
        private decimal _finalTotalAmount = 0.0M;

        private PaymentType _lastSelectedTypeOfPayment = PaymentType.Unknown;
        private uscPaymentGateway.StartBTngPaymentDelg _startBTngPaymentDelgHandle = null;
        private pgKomuter_BTnGPayment _bTnGPaymentStaff = null;
        private (string SelectedPaymentGateway, string SelectedPaymentGatewayLogoUrl, string SelectedPaymentMethod) _paymentGatewayInfo = (SelectedPaymentGateway: null, SelectedPaymentGatewayLogoUrl: null, SelectedPaymentMethod: null);

        //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        //private string TerminalLogoPath
        //{
        //    get => $@"file://{App.ExecutionFolderPath}\Resources\MelSenLogo.jpg";
        //}

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
                return "RPTOnlineKomuter3iNCH";
            }
        }

        private string TicketErrorReportSourceName
        {
            get
            {
                return "TicketKTMBErrorMessage";
            }
        }

        private string CreditCardReceiptReportSourceName
        {
            get
            {
                return "RPTCreditCardReceipt";
            }
        }
        //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX


        public pgKomuter()
        {
            InitializeComponent();

            _mapPage = new pgMap();
            _salesPanelPage = new pgSalesPanel();
            _paxPage = new pgKomuterPax();
            _printTicketPage = new pgPrintTicket2();
            _komuterTicketMyKadListPage = new pgKomuterTicketMyKadList();
            

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\Komuter\rosKomuterMal.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\Komuter\rosKomuterEng.xaml");
            _langPayMal = CommonFunc.GetXamlResource(@"ViewPage\Payment\rosPaymentMalay.xaml");
            _langPayEng = CommonFunc.GetXamlResource(@"ViewPage\Payment\rosPaymentEnglish.xaml");

            _cardPayPage = pgCreditCardPayWaveV2.GetCreditCardPayWavePage();

            _startBTngPaymentDelgHandle = new uscPaymentGateway.StartBTngPaymentDelg(StartBTnGPayment);

            _bTnGPaymentStaff = new pgKomuter_BTnGPayment(this, FrmSubFrame, BdSubFrame, frmPrinting, _printTicketPage, frmMap, frmSales, _mapPage, 
               new pgSalesPanel.ActivatePaymentSelectionDelg(_salesPanelPage.ActivatePaymentSelection));

            _mapPage.OnResetClick += _mapPage_OnResetClick;
            _salesPanelPage.OnJourneyTypeChanged += _salesPanelPage_OnJourneyTypeChanged;
            _salesPanelPage.OnStartPayment += _salesPanelPage_OnStartPayment;
            _salesPanelPage.Init(_startBTngPaymentDelgHandle);

            _paxPage.OnOkClick += _paxPage_OnOkClick;
            _paxPage.OnCancelClick += _paxPage_OnCancelClick;
            _komuterTicketMyKadListPage.OnOkClick += _komuterTicketMyKadListPage_OnOkClick;
            _komuterTicketMyKadListPage.OnCancelClick += _komuterTicketMyKadListPage_OnCancelClick;

            _routeSelector = new RouteSelector(_mapPage, new RouteSelector.ResetRouteSelection(TryResetAllSelection));
            _routeSelector.OnStationSelectChanged += _routSelector_OnStationSelectChanged;

            _printTicketPage.OnDoneClick += _printTicketPage_OnDoneClick;
            _printTicketPage.OnPauseClick += _printTicketPage_OnPauseClick;
        }

        private void _salesPanelPage_OnStartPayment(object sender, EventArgs e)
        {
            try
            {
                StartCreditCardPayWavePayment();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgKomuter._printTicketPage_OnDoneClick");
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
                                App.MainScreenControl.ShowWelcome();
                                //App.NetClientSvc.SalesService.NavigateToPage(PageNavigateDirection.Exit);
                                _alreadyExitPaymentFlag = true;
                                App.ShowDebugMsg("pgKomuter._printTicketPage_OnDoneClick");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10001125)");
                        App.Log.LogError(_logChannel, "", new Exception("(EXIT10001125)", ex), "EX01", "pgKomuter._printTicketPage_OnDoneClick");
                    }
                })));
                submitWorker.IsBackground = true;
                submitWorker.Start();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgKomuter._printTicketPage_OnDoneClick");
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
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgKomuter._printTicketPage_OnPauseClick");
            }
        }

        private void _komuterTicketMyKadListPage_OnOkClick(object sender, TicketSelectionChangedEventArgs e)
        {
            try
            {
                if (_selectedTicketList == null)
                    _selectedTicketList = e;

                else if (e.TicketList?.Length > 0)
                {
                    foreach(KomuterTicket resTick in e.TicketList)
                    {
                        foreach (KomuterTicket selTick in _selectedTicketList.TicketList)
                        {
                            if ((selTick.TicketTypeId?.Equals(resTick.TicketTypeId) == true) && (resTick.DetailList?.Count > 0))
                            {
                                KomuterTicketDetail[] dList = resTick.DetailList.ToArray();
                                selTick.DetailList.Clear();
                                selTick.DetailList.AddRange(dList);
                                break;
                            }
                        }
                    }
                }

                BdSubFrame.Visibility = Visibility.Collapsed;
                FrmSubFrame.Content = null;
                FrmSubFrame.NavigationService.RemoveBackEntry();
                FrmSubFrame.NavigationService.Content = null;

                BdSubFrame.Visibility = Visibility.Collapsed;

                if (e.TicketList?.Length > 0)
                {
                    _currSelectedPackage.Id = e.KomuterPackageId;
                    _currSelectedPackage.Description = e.KomuterPackageDescription;
                    _currSelectedPackage.Duration = e.KomuterPackageDuration;
                    _currency = (e.TicketList[0]?.Currency) ?? _currency;
                    _salesPanelPage.ShowOnlySelectedPackage(e.KomuterPackageId);

                    System.Windows.Forms.Application.DoEvents();

                    RequestBooking();

                    _mapPage.DeactivateReset();
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001211); {ex.Message}");
                App.Log.LogError(_logChannel, "", new Exception("(EXIT10001211);", ex), "EX01", "pgKomuter._komuterTicketMyKadListPage_OkClick");
            }
        }

        private void _komuterTicketMyKadListPage_OnCancelClick(object sender, EventArgs e)
        {
            try
            {
                _currSelectedPackage.Id = null;
                _selectedTicketList = null;

                BdSubFrame.Visibility = Visibility.Collapsed;
                FrmSubFrame.Content = null;
                FrmSubFrame.NavigationService.RemoveBackEntry();
                FrmSubFrame.NavigationService.Content = null;

                BdSubFrame.Visibility = Visibility.Collapsed;

                _salesPanelPage.ActivateTicketPackageSelection();
                _salesPanelPage.ResetJourneyTypeSelection();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001212); {ex.Message}");
                App.Log.LogError(_logChannel, "", new Exception("(EXIT10001212);", ex), "EX01", "pgKomuter._komuterTicketMyKadListPage_OnCancelClick");
            }
        }

        private void SubmitPause()
        {
            System.Windows.Forms.Application.DoEvents();

            Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
                try
                {
                    App.NetClientSvc.SalesService.PauseCountDown(out bool isServerResponded);
                }
                catch (Exception ex)
                {
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10001162)");
                    App.Log.LogError(_logChannel, "", new Exception("(EXIT10001162)", ex), "EX01", "pgKomuter._printTicketPage_OnPauseClick");
                }
            })));
            submitWorker.IsBackground = true;
            submitWorker.Start();
        }

        private string _currency = "*";
        
        private void _paxPage_OnOkClick(object sender, TicketSelectionChangedEventArgs e)
        {
            try
            {
                _selectedTicketList = e;
                BdSubFrame.Visibility = Visibility.Collapsed;
                FrmSubFrame.Content = null;
                FrmSubFrame.NavigationService.RemoveBackEntry();
                FrmSubFrame.NavigationService.Content = null;

                BdSubFrame.Visibility = Visibility.Collapsed;

                if (e.TicketList?.Length > 0)
                {
                    List<KomuterTicket> verificationReqKomuterTicketList = (from tick in e.TicketList
                                                                            where (tick.VerifyMalaysianKomuter)
                                                                            select tick).ToList();

                    if (verificationReqKomuterTicketList.Count > 0)
                    {
                        _komuterTicketMyKadListPage.InitTicketPackage(e.KomuterPackageId, e.KomuterPackageDescription, e.KomuterPackageDuration, verificationReqKomuterTicketList.ToArray(), _currentLanguage, _language);
                        FrmSubFrame.NavigationService.Navigate(_komuterTicketMyKadListPage);
                        BdSubFrame.Visibility = Visibility.Visible;
                        System.Windows.Forms.Application.DoEvents();
                    }
                    else
                    {
                        _currSelectedPackage.Id = e.KomuterPackageId;
                        _currSelectedPackage.Description = e.KomuterPackageDescription;
                        _currSelectedPackage.Duration = e.KomuterPackageDuration;
                        _currency = (e.TicketList[0]?.Currency) ?? _currency;
                        _salesPanelPage.ShowOnlySelectedPackage(e.KomuterPackageId);

                        System.Windows.Forms.Application.DoEvents();

                        RequestBooking();

                        _mapPage.DeactivateReset();
                    }
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001116); {ex.Message}");
                App.Log.LogError(_logChannel, "", new Exception("(EXIT10001116);", ex), "EX01", "pgKomuter._paxPage_OnOkClick");
            }
        }

        private NetServiceAnswerMan _requestKomuterTicketBookingAnswerMan = null;
        private void RequestBooking()
        {
            try
            {
                _requestKomuterTicketBookingAnswerMan?.Dispose();

                List<TicketItem> tickItemList = new List<TicketItem>();

                foreach (KomuterTicket komTick in _selectedTicketList.TicketList)
                {
                    TicketItem tick = new TicketItem()
                    {
                        TicketTypeId = komTick.TicketTypeId,
                        Quantity = komTick.SelectedNoOfPax
                    };

                    if (komTick.DetailList?.Count > 0)
                    {
                        List<TicketItemDetail> tickDetList = new List<TicketItemDetail>();
                        foreach (var det in komTick.DetailList)
                        {
                            tickDetList.Add(new TicketItemDetail() { Name = det.Name, MyKadId = det.MyKadId, TicketTypeId = komTick.TicketTypeId });
                        }

                        tick.DetailList = tickDetList.ToArray();
                    }

                    tickItemList.Add(tick);
                }

                _requestKomuterTicketBookingAnswerMan =
                    App.NetClientSvc.SalesService.RequestKomuterTicketBooking(
                        _originStationId,
                        _originStationName,
                        _destinationStationId,
                        _destinationStationName,
                        _currSelectedPackage.Id,
                        tickItemList.ToArray(),
                        "Local Server not responding (EXIT10001119)",
                        new NetServiceAnswerMan.FailLocalServerResponseCallBackDelg(delegate (string failMessage)
                        {
                            App.MainScreenControl.Alert(detailMsg: failMessage);
                        }),
                        waitDelaySec: 25);
            }
            catch (Exception ex)
            {
                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10001118)"); 
                App.Log.LogError(_logChannel, "", new Exception("(EXIT10001118)", ex), "EX01", "pgKomuter.RequestBooking");
            }
        }

        private void _creditCardPayWavePage_OnEndPayment(object sender, EndOfPaymentEventArgs e)
        {
            string bankRefNo = "";
            bool isAllowAlert = true;
            try
            {
                bankRefNo = e.BankReferenceNo;

                //CYA-DEMO
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
                isAllowAlert = false;
                _endPaymentThreadWorker = new Thread(new ThreadStart(OnEndPaymentThreadWorking));
                _endPaymentThreadWorker.IsBackground = true;
                _endPaymentThreadWorker.Start();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, _ktmbSalesTransactionNo, 
                    new WithDataException($@"{ex.Message}; (EXIT10001126)", ex, e), "EX02", "pgKomuter._cashPaymentPage_OnEndPayment", 
                    adminMsg: $@"{ex.Message}; (EXIT10001126)");

                if (isAllowAlert)
                {
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10001126)");
                }
            }

            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
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
                                camt = _bookingTotalAmount,
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
                            frmMap.Content = null;
                            frmMap.NavigationService.RemoveBackEntry();

                            frmSales.Content = null;
                            frmSales.NavigationService.RemoveBackEntry();
                            System.Windows.Forms.Application.DoEvents();

                            FrmSubFrame.Content = null;
                            FrmSubFrame.NavigationService.RemoveBackEntry();
                            FrmSubFrame.NavigationService.Content = null;
                            System.Windows.Forms.Application.DoEvents();

                            _printTicketPage.InitSuccessPaymentCompleted(_ktmbSalesTransactionNo, _language);

                            _cardPayPage.ClearEvents();
                            //FrmGoPay.NavigationService.Navigate(_printTicketPage);
                            FrmSubFrame.Content = null;
                            FrmSubFrame.NavigationService.RemoveBackEntry();
                            FrmSubFrame.NavigationService.Content = null;
                            BdSubFrame.Visibility = Visibility.Collapsed;

                            frmPrinting.NavigationService.Navigate(_printTicketPage);

                            //App.MainScreenControl.ExecMenu.HideMiniNavigator();
                            System.Windows.Forms.Application.DoEvents();
                        }));

                        App.NetClientSvc.SalesService.SubmitKomuterBookingPayment(
                            _ktmbSalesTransactionNo, _bookingId, _bookingCurrency, _bookingTotalAmount, FinancePaymentMethod.CreditCard, _lastCreditCardAnswer, out bool isServerResponded);

                        //DEBUG-Testing .. isServerResponded = false;

                        if (isServerResponded == false)
                        {
                            _printTicketPage.UpdateCompleteTransactionState(isTransactionSuccess: false, _language);

                            string probMsg = "Local Server not responding (EXIT10000914)";
                            probMsg = $@"{probMsg}; Transaction No.:{_ktmbSalesTransactionNo}";

                            App.Log.LogError(_logChannel, _ktmbSalesTransactionNo, new Exception(probMsg), "EX01", "pgKomuter.OnEndPaymentThreadWorking");

                            _printingThreadWorker = new Thread(new ThreadStart(PrintErrorThreadWorking));
                            _printingThreadWorker.IsBackground = true;
                            _printingThreadWorker.Start();
                            //PrintTicketError(_transactionNo);
                            //App.ShowDebugMsg("Print Sales Receipt on Fail Completed Transaction ..; pgKomuter.OnEndPaymentThreadWorking");
                            //App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000912)");
                        }
                    }
                    else if ((e.ResultState == AppDecorator.Common.PaymentResult.Cancel) || (e.ResultState == AppDecorator.Common.PaymentResult.Fail))
                    {
                        App.TimeoutManager.ResetTimeout();

                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            _cardPayPage.ClearEvents();
                            FrmSubFrame.Content = null;
                            FrmSubFrame.NavigationService.RemoveBackEntry();
                            FrmSubFrame.NavigationService.Content = null;

                            _mapPage.ActivateReset();
                            BdSubFrame.Visibility = Visibility.Collapsed;
                            _salesPanelPage.ActivatePaymentSelection();
                        }));
                    }
                    else
                    {
                        // Below used to handle result like ..
                        //------------------------------------------
                        // AppDecorator.Common.PaymentResult.Timeout
                        // AppDecorator.Common.PaymentResult.Unknown

                        //if (_isPauseOnPrinting == false)

                        App.Log.LogError(_logChannel, $@"{_ktmbSalesTransactionNo}", new WithDataException("Abnormal workflow (Timeout / Unknown)", new Exception("Abnormal workflow"), e), "X11", "pgKomuter.OnEndPaymentThreadWorking");

                        App.MainScreenControl.ShowWelcome();
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; EX02; pgKomuter.OnEndPaymentThreadWorking");
                    App.Log.LogError(_logChannel, "-", ex, "EX02", "pgKomuter.OnEndPaymentThreadWorking");
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
                    App.ShowDebugMsg("Print Sales Receipt on Fail Completed Transaction ..; pgKomuter.OnEndPaymentThreadWorking");

                    // App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000912)");
                }
                catch (ThreadAbortException)
                {
                    //PDFTools.KillAdobe("AcroRd32");
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; EX02; pgKomuter.PrintErrorThreadWorking");
                    App.Log.LogError(_logChannel, "-", ex, "EX03", "pgKomuter.PrintErrorThreadWorking");
                }
            }
        }

        public void UpdateBookingData(UIKomuterTicketBookingAck uiBookingAck)
        {
            if ((uiBookingAck.MessageData is KomuterBookingResult res) 
                && (res.Code.Equals(WebAPIAgent.ApiCodeOK)))
            {
                _bookingId = res.Data.Booking_Id;
                _bookingCurrency = res.Data.MCurrencies_Id;
                _ktmbSalesTransactionNo = res.Data.BookingNo;
                _bookingTotalAmount = res.Data.TotalAmount;
                _bookingExpiredDateTime = res.Data.BookingExpiredDateTime;

                _salesPanelPage.ActivatePaymentSelection();
                _salesPanelPage.ShowTripDescriptionForPayment();
                _salesPanelPage.ShowTicketDurationForPayment($@"{_currSelectedPackage.Duration} [{_currSelectedPackage.Description}]");
                _salesPanelPage.ShowSelectedTicketItemList(_selectedTicketList.TicketList);
                _salesPanelPage.ShowSalesTotalAmount(_bookingCurrency, _bookingTotalAmount);

                _mapPage.ActivateReset();
                System.Windows.Forms.Application.DoEvents();
            }
        }

        public void UpdateKomuterTicketPaymentStatus(UIKomuterCompletePaymentAck paymentStatusAck)
        {
            //MessageBox.Show("Payment Success");

            ///// 20201201 - Remark to continue print ticket.
            ////if (_pageLoaded == false)
            ////    return;

            //////App.MainScreenControl.MainFormDispatcher.Invoke(new Action(() => {
            //////    //App.MainScreenControl.ExecMenu.HideMiniNavigator();
            //////}));

            _printingThreadWorker = new Thread(new ThreadStart(PrintThreadWorking));
            _printingThreadWorker.IsBackground = true;
            _printingThreadWorker.Start();

            void PrintThreadWorking()
            {
                try
                {
                    CompleteKomuterPaymentResult payRes = (CompleteKomuterPaymentResult)paymentStatusAck.MessageData;

                    App.Log.LogText(_logChannel, (payRes?.Data?.KomuterBookingPaymentResult?.BookingNo) ?? "-", paymentStatusAck, "A01", "pgKomuter.UpdateKomuterTicketPaymentStatus",
                        extraMsg: "Start - UpdateKomuterTicketPaymentStatus; MsgObj: UIKomuterCompletePaymentAck");

                    //DEBUG-Testing .. payRes.Data.KomuterBookingPaymentResult.Error = YesNo.Yes;
                    //------------------------------------- _ktmbSalesTransactionNo

                    bool isProceedToPrintTicket = false;

                    //Fix QR Code
                    if ((payRes?.Data?.KomuterBookingPaymentResult?.Error?.Equals(YesNo.No) == true)
                        && (payRes?.Data?.KomuterTicketPrintList?.Length > 0))
                    {
                        
                        string trnNo = _ktmbSalesTransactionNo;
                        if (FixTicketQRCodeData(trnNo, payRes.Data.KomuterTicketPrintList, out KomuterPrintTicketModel[] resultTicketList))
                        {
                            payRes.Data.KomuterTicketPrintList = resultTicketList;
                            isProceedToPrintTicket = true;
                        }
                    }

                    if (isProceedToPrintTicket)
                    {
                        _printTicketPage.UpdateCompleteTransactionState(isTransactionSuccess: true, _language);

                        //CYA-TEST
                        //PaymentSubmissionResult stt = ReportDataQuery.GetTicketTestData01();
                        //UICompleteTransactionResult uiCompltResult2 = new UICompleteTransactionResult(uiCompltResult.RefNetProcessId, uiCompltResult.ProcessId, DateTime.Now, stt, ProcessResult.Success);
                        //uiCompltResult2.UpdateSession(uiCompltResult.Session);
                        //uiCompltResult = uiCompltResult2;
                        //----------------------------------------------------------------------------------------

                        System.Windows.Forms.Application.DoEvents();
                        App.ShowDebugMsg("Printing Ticket ..; pgKomuter.UpdateKomuterTicketPaymentStatus");

                        App.Log.LogText(_logChannel, _ktmbSalesTransactionNo, "....enter sequence of printing", "A05", classNMethodName: "pgKomuter.PrintThreadWorking", messageType: AppDecorator.Log.MessageType.Debug);

                        if (_lastSelectedTypeOfPayment == PaymentType.PaymentGateway)
                            PrintTicket(paymentStatusAck, null, _bTnGPaymentStaff.LastBTnGSaleTransNo);

                        else
                            PrintTicket(paymentStatusAck, _lastCreditCardAnswer, null);

                        App.Log.LogText(_logChannel, _ktmbSalesTransactionNo, "....exit sequence of printing", "A10", classNMethodName: "pgKomuter.PrintThreadWorking", messageType: AppDecorator.Log.MessageType.Debug);

                        ImagePrintingTools.WaitForPrinting(60 * 2);

                        lock (_alreadyExitPaymentLock)
                        {
                            if ((_isPauseOnPrinting == false) && (_alreadyExitPaymentFlag == false))
                            {
                                App.Log.LogText(_logChannel, _ktmbSalesTransactionNo, $@"isPauseOnPrinting : {_isPauseOnPrinting}; alreadyExitPaymentFlag: {_alreadyExitPaymentFlag}", "A15", classNMethodName: "pgKomuter.PrintThreadWorking", messageType: AppDecorator.Log.MessageType.Debug);

                                App.MainScreenControl.ShowWelcome();
                                _alreadyExitPaymentFlag = true;
                            }
                        }
                    }
                    else
                    {
                        _printTicketPage.UpdateCompleteTransactionState(isTransactionSuccess: false, _language);

                        System.Windows.Forms.Application.DoEvents();

                        string probMsg = (string.IsNullOrWhiteSpace(payRes?.MessageString()) == false) ? payRes.MessageString() : "Fail Complete Payment Transaction (U)";
                        probMsg = $@"{probMsg}; Transation No.:{_ktmbSalesTransactionNo}";

                        App.Log.LogError(_logChannel, _ktmbSalesTransactionNo, new Exception(probMsg), "EX01", "pgKomuter.UpdateKomuterTicketPaymentStatus", adminMsg: "Problem ;" + probMsg);

                        PrintTicketError1(paymentStatusAck);
                        App.ShowDebugMsg("Print Sales Receipt on Fail Completed Transaction ..; pgKomuter.UpdateKomuterTicketPaymentStatus");

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
                    App.ShowDebugMsg($@"Error: {ex.Message}; pgKomuter.PrintThreadWorking");
                    App.Log.LogError(_logChannel, "-", ex, "EX02", "pgKomuter.PrintThreadWorking");
                    //App.MainScreenControl.Alert(detailMsg: $@"Unable to read Transaction Status; (EXIT10000911)");
                }
            }

            return;


            bool FixTicketQRCodeData(string transactionNo, KomuterPrintTicketModel[] ticketList, out KomuterPrintTicketModel[] resultTicketList)
            {
                resultTicketList = null;
                bool retVal = false;
                int ticketInx = 0;
                string errorNote = "";

                try
                {
                    foreach (KomuterPrintTicketModel tk in ticketList)
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

                    App.Log.LogError(_logChannel, transactionNo, ex, "EX01", "pgKomuter.FixTicketQRCodeData",
                        adminMsg: errMsg);
                }
                return retVal;
            }
        }

        private void _paxPage_OnCancelClick(object sender, EventArgs e)
        {
            try
            {
                _currSelectedPackage.Id = null;
                _selectedTicketList = null;

                BdSubFrame.Visibility = Visibility.Collapsed;
                FrmSubFrame.Content = null;
                FrmSubFrame.NavigationService.RemoveBackEntry();
                FrmSubFrame.NavigationService.Content = null;

                BdSubFrame.Visibility = Visibility.Collapsed;

                _salesPanelPage.ActivateTicketPackageSelection();
                _salesPanelPage.ResetJourneyTypeSelection();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001117); {ex.Message}");
                App.Log.LogError(_logChannel, "", new Exception("(EXIT10001117);", ex), "EX01", "pgKomuter._paxPage_OnCancelClick");
            }
        }

        private void _salesPanelPage_OnJourneyTypeChanged(object sender, JourneyTypeChangeEventArgs e)
        {
            try
            {
                App.TimeoutManager.ResetTimeout();

                _paxPage.InitTicketPackage(_currencyId, _maxNoOfPaxAllowed, e.KomuterPackage, _currentLanguage);
                FrmSubFrame.NavigationService.Navigate(_paxPage);
                BdSubFrame.Visibility = Visibility.Visible;
                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001115); {ex.Message}");
                App.Log.LogError(_logChannel, "", new Exception("(EXIT10001115);", ex), "EX01", "pgKomuter._salesPanelPage_OnJourneyTypeChanged");
            }
        }

        private void _mapPage_OnResetClick(object sender, EventArgs e)
        {
            try
            {
                App.TimeoutManager.ResetTimeout();
                ResetAllSelection(imposeResetPermissionCheck: false);
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001114); {ex.Message}");
                App.Log.LogError(_logChannel, "", new Exception("(EXIT10001114);", ex), "EX01", "pgKomuter._mapPage_OnResetClick");
            }
        }

        /// <summary>
        /// Return true if ResetPermission has agreed. Else return false
        /// </summary>
        /// <param name="imposeResetPermissionCheck"></param>
        /// <returns></returns>
        public bool ResetAllSelection(bool imposeResetPermissionCheck = true)
        {
            if (imposeResetPermissionCheck == true)
            {
                if (_mapPage.IsResetActivated == false)
                    return false;
            }

            try
            {
                App.NetClientSvc.SalesService.ResetKomuterUserSession();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgKomuter.ResetAllSelection");
            }

            _lastCheckoutResult = null;
            _bookingId = null;
            _bookingCurrency = null;
            _ktmbSalesTransactionNo = null;
            _bookingTotalAmount = 0.0M;
            _bookingExpiredDateTime = null;

            _finalCurrency = null;
            _finalTotalAmount = 0.0M;

            _selectedTicketList = null;
            _currSelectedPackage.Id = null;
            _destinationStationId = null;
            _destinationStationName = null;
            _routeSelector.LoadSelector(_originStationId);
            _salesPanelPage.ResetSelection(_originStationId, _originStationName);
            _salesPanelPage.ActivateDestinationSelection();

            return true;
        }

        private bool TryResetAllSelection()
        {
            return ResetAllSelection(imposeResetPermissionCheck: true);
        }

        private void _routSelector_OnStationSelectChanged(object sender, StationSelectedEventArgs e)
        {
            try
            {
                App.TimeoutManager.ResetTimeout();

                _currSelectedPackage.Id = null;
                _destinationStationId = e.StationId;
                _destinationStationName = e.StationName;

                App.ShowDebugMsg($@"Selected Station Id: {e.StationId}; Selected Station Name: {e.StationName}");
                _salesPanelPage.UpdateDestination(e.StationId, e.StationName);
                _salesPanelPage.ActivateTicketPackageSelection();
                _mapPage.DeactivateReset();

                QueryTicketTypePackage();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001111); {ex.Message}");
                App.Log.LogError(_logChannel, "", new Exception("(EXIT10001111);", ex), "EX01", "pgKomuter._routSelector_OnStationSelectChanged");
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                BdSubFrame.Visibility = Visibility.Collapsed;
                FrmSubFrame.Content = null;
                FrmSubFrame.NavigationService.RemoveBackEntry();
                FrmSubFrame.NavigationService.Content = null;

                BdSubFrame.Visibility = Visibility.Collapsed;

                frmPrinting.Content = null;
                frmPrinting.NavigationService.RemoveBackEntry();

                frmMap.Content = null;
                frmMap.NavigationService.RemoveBackEntry();

                frmSales.Content = null;
                frmSales.NavigationService.RemoveBackEntry();

                _mapPage.SetLanguage(_language, _currentLanguage);
                _salesPanelPage.SetLanguage(_language, _currentLanguage);
                frmMap.NavigationService.Navigate(_mapPage);
                frmSales.NavigationService.Navigate(_salesPanelPage);

                _bookingId = null;
                _bookingCurrency = null;
                _ktmbSalesTransactionNo = null;
                _bookingTotalAmount = 0.0M;
                _bookingExpiredDateTime = null;

                _finalCurrency = null;
                _finalTotalAmount = 0.0M;

                _selectedTicketList = null;
                _currSelectedPackage.Id = null;
                _routeSelector.LoadSelector(_originStationId);
                _salesPanelPage.ResetSelection(_originStationId, _originStationName);
                _salesPanelPage.ActivateDestinationSelection();
                _mapPage.ActivateReset();

                System.Windows.Forms.Application.DoEvents();

                _pageLoaded = true;
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgKomuter.Page_Loaded");
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _pageLoaded = false;

                _requestKomuterTicketBookingAnswerMan?.Dispose();
                _queryTicketTypePackageAnswerMan?.Dispose();

                _cardPayPage.ClearEvents();
                _bTnGPaymentStaff.PageUnloaded();
                BdSubFrame.Visibility = Visibility.Collapsed;
                FrmSubFrame.Content = null;
                FrmSubFrame.NavigationService.RemoveBackEntry();
                FrmSubFrame.NavigationService.Content = null; 

                frmPrinting.Content = null;
                frmPrinting.NavigationService.RemoveBackEntry();
                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgKomuter.Page_Unloaded");
            }
        }

        public void InitData(UISalesStartSellingAck uiStartSellingAck)
        {
            _lastCreditCardAnswer = null;
            _lastCheckoutResult = null;
            _latestTicketTypePackageSummary = null;
            _destinationStationId = null;
            _destinationStationName = null;
            _maxNoOfPaxAllowed = uiStartSellingAck.Session.Komuter_MaxPaxAllowed;

            _language = (uiStartSellingAck.Session != null ? uiStartSellingAck.Session.Language : LanguageCode.English);
            if (_language == LanguageCode.Malay)
            {
                _currentLanguage = _langMal;
                _currentPayLanguage = _langPayMal;
            }
            else
            {
                _currentLanguage = _langEng;
                _currentPayLanguage = _langPayEng;
            }

            _originStationId = uiStartSellingAck.Session.OriginStationCode;
            _originStationName = uiStartSellingAck.Session.OriginStationName;
            _currSelectedPackage.Id = null;

            _alreadyExitPaymentFlag = false;
            _bookingId = null;
            _bookingCurrency = null;
            _ktmbSalesTransactionNo = null;
            _bookingTotalAmount = 0.0M;
            _bookingExpiredDateTime = null;

            _finalCurrency = null;
            _finalTotalAmount = 0.0M;
        }

        /// <summary>
        /// FuncCode:EXIT80.0505
        /// </summary>
        private void StartBTnGPayment(string paymentGateWay, string paymentGatewayLogoUrl, string paymentMethod)
        {
            try
            {
                if (_salesPanelPage.TriggerPaymentInProgress() == false)
                {
                    return;
                }

                _lastSelectedTypeOfPayment = PaymentType.PaymentGateway;
                _paymentGatewayInfo.SelectedPaymentGateway = paymentGateWay;
                _paymentGatewayInfo.SelectedPaymentGatewayLogoUrl = paymentGatewayLogoUrl;
                _paymentGatewayInfo.SelectedPaymentMethod = paymentMethod;

                _mapPage.DeactivateReset();

                if (_lastCheckoutResult is null)
                {
                    ValidatePaymentConditionForCheckout();
                    //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                    App.NetClientSvc.SalesService.RequestKomuterBookingCheckout(
                            _ktmbSalesTransactionNo, _bookingId, _bookingTotalAmount, paymentMethod, out bool isServerResponded, 20);

                    if (isServerResponded == false)
                        App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT80.0505.X01)");
                }
                else
                {
                    RunPaymentGateway(paymentGateWay, paymentGatewayLogoUrl, paymentMethod);
                }
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgPayment.StartCreditCardPayWavePayment");
            }
        }

        private void StartCreditCardPayWavePayment()
        {
            try
            {
                if (_salesPanelPage.TriggerPaymentInProgress() == false)
                {
                    return;
                }

                _lastSelectedTypeOfPayment = PaymentType.CreditCard;

                _paymentGatewayInfo.SelectedPaymentGateway = null;
                _paymentGatewayInfo.SelectedPaymentGatewayLogoUrl = null;
                _paymentGatewayInfo.SelectedPaymentMethod = null;

                _mapPage.DeactivateReset();

                if (_lastCheckoutResult is null)
                {
                    ValidatePaymentConditionForCheckout();
                    //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                    App.NetClientSvc.SalesService.RequestKomuterBookingCheckout(
                            _ktmbSalesTransactionNo, _bookingId, _bookingTotalAmount, FinancePaymentMethod.CreditCard, out bool isServerResponded, 20);

                    if (isServerResponded == false)
                        App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10001163)");
                }
                else
                {
                    RunCardPayment();
                }               
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgKomuter.StartCreditCardPayWavePayment");
            }
        }

        private void ValidatePaymentConditionForCheckout()
        {
            // Check Printer Status xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            try
            {
                bool isPrinterError = false;
                bool isPrinterWarning = false;
                string statusDescription = null;
                string locStateDesc = null;

                try
                {
                    App.AppHelp.PrinterApp.GetPrinterStatus(out isPrinterError, out isPrinterWarning, out statusDescription, out locStateDesc);
                }
                catch (Exception ex)
                {
                    App.Log.LogError(_logChannel, "-", ex, "EX02", "pgKomuter.StartCreditCardPayWavePayment");
                    throw new Exception($@"{ex.Message}; (EXIT10000921); {NssIT.Kiosk.Client.AppHelper.PrinterErrorTag}");
                }

                if (isPrinterError || isPrinterWarning)
                {
                    StatusHub.GetStatusHub().zNewStatus_IsPrinterStandBy(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, statusDescription);

                    if (string.IsNullOrWhiteSpace(statusDescription))
                        statusDescription = "Printer Error (X01); (EXIT10000922)";

                    App.Log.LogText(_logChannel, "-", $@"Error; (EXIT10000923); {statusDescription}", "A11", "pgKomuter.StartCreditCardPayWavePayment", AppDecorator.Log.MessageType.Error);
                    throw new Exception($@"{statusDescription}; (EXIT10000923); {NssIT.Kiosk.Client.AppHelper.PrinterErrorTag}");
                }
                else
                {
                    StatusHub.GetStatusHub().zNewStatus_IsPrinterStandBy(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Printer Standing By");
                }

                if (AppDecorator.Config.Setting.GetSetting().DisablePrinterTracking == false)
                {
                    if (string.IsNullOrWhiteSpace(App.SysParam.FinalizedPrinterName) == true)
                    {
                        App.Log.LogText(_logChannel, "-", $@"Error; (EXIT10000924); Default (or correct) printer not found", "A11", "pgKomuter.StartCreditCardPayWavePayment", AppDecorator.Log.MessageType.Error);
                        throw new Exception($@"{statusDescription}; (EXIT10000924); Default (or correct) printer not found; {NssIT.Kiosk.Client.AppHelper.PrinterErrorTag}");
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {

                    App.NetClientSvc.SalesService.EndUserSession();
                }
                catch { }

                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000925)");
                App.Log.LogError(_logChannel, "", new Exception("(EXIT10000925)", ex), "EX01", "pgKomuter.StartCreditCardPayWavePayment");
            }
        }
                
        public void UpdateBookingCheckoutResult(UIKomuterBookingCheckoutAck bookingCheckoutAck)
        {
            if (bookingCheckoutAck.MessageData is KomuterBookingCheckoutResult chkOutResult)
            {
                try
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (chkOutResult.Code.Equals(WebAPIAgent.ApiCodeOK))
                        {
                            _lastCheckoutResult = chkOutResult;

                            _finalCurrency = chkOutResult.Data.MCurrencies_Id;
                            _finalTotalAmount = chkOutResult.Data.PayableAmount;

                            PaymentType currPayType = PaymentType.Unknown;
                            if (_lastSelectedTypeOfPayment != PaymentType.Unknown)
                            {
                                try 
                                {
                                    currPayType = _lastSelectedTypeOfPayment;
                                }
                                catch 
                                {
                                    currPayType = PaymentType.Unknown;
                                }
                            }

                            if (currPayType != PaymentType.Unknown)
                            {
                                if ((currPayType == PaymentType.PaymentGateway)
                                    && (string.IsNullOrWhiteSpace(_paymentGatewayInfo.SelectedPaymentGateway) == false)
                                    && (string.IsNullOrWhiteSpace(_paymentGatewayInfo.SelectedPaymentMethod) == false)
                                    )
                                {
                                    RunPaymentGateway(_paymentGatewayInfo.SelectedPaymentGateway, _paymentGatewayInfo.SelectedPaymentGatewayLogoUrl, _paymentGatewayInfo.SelectedPaymentMethod);
                                }
                                else if (currPayType == PaymentType.CreditCard)
                                {
                                    RunCardPayment();
                                }
                            }
                        }
                        else
                        {
                            _finalCurrency = null;
                            _finalTotalAmount = 0.0M;

                            string errMsg = chkOutResult.MessageString();

                            if (string.IsNullOrWhiteSpace(errMsg))
                                errMsg = $@"Unknown error/answer from local server; (EXIT10001164); ";
                            
                            throw new Exception(errMsg);
                        }
                    }));
                }
                catch (Exception ex)
                {
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10001165)");
                    App.Log.LogError(_logChannel, "", new Exception("(EXIT10001165)", ex), "EX01", "pgPayment.UpdateBookingCheckoutResult");
                }
            }
        }

        private void RunCardPayment()
        {
            _cardPayPage.ClearEvents();
            _cardPayPage.InitPaymentData(_finalCurrency, _finalTotalAmount, _ktmbSalesTransactionNo, App.SysParam.PrmPayWaveCOM, _currentPayLanguage);
            _cardPayPage.OnEndPayment += _creditCardPayWavePage_OnEndPayment;
            FrmSubFrame.Content = null;
            FrmSubFrame.NavigationService.RemoveBackEntry();
            FrmSubFrame.NavigationService.Content = null;
            System.Windows.Forms.Application.DoEvents();

            App.CurrentTransStage = TicketTransactionStage.Komuter;
            FrmSubFrame.NavigationService.Navigate(_cardPayPage);
            BdSubFrame.Visibility = Visibility.Visible;
            System.Windows.Forms.Application.DoEvents();
        }

        private void RunPaymentGateway(string paymentGateWay, string paymentGatewayLogoUrl, string paymentMethod)
        {
            ResourceDictionary langRec = (_language == LanguageCode.Malay) ? _langPayMal : _langPayEng;

            _bTnGPaymentStaff.StartBTnGPayment(_currency, _bookingTotalAmount, _ktmbSalesTransactionNo, paymentGateWay, "-Komuter Passenger-", "-Komuter Passenger-", "1234567890",
                _bookingId, paymentGatewayLogoUrl, paymentMethod,
                langRec, _language);
        }

        private NetServiceAnswerMan _queryTicketTypePackageAnswerMan = null;
        private void QueryTicketTypePackage()
        {
            if ((string.IsNullOrWhiteSpace(_originStationId) == false) && (string.IsNullOrWhiteSpace(_originStationName) == false)
                && (string.IsNullOrWhiteSpace(_destinationStationId) == false) && (string.IsNullOrWhiteSpace(_destinationStationName) == false)
                )
            {
                try
                {
                    _queryTicketTypePackageAnswerMan?.Dispose();
                
                    _queryTicketTypePackageAnswerMan =
                        App.NetClientSvc.SalesService.QueryKomuterTicketTypePackage(
                            _originStationId, _originStationName, _destinationStationId, _destinationStationName,
                            "Local Server not responding (EXIT10001112)",
                            new NetServiceAnswerMan.FailLocalServerResponseCallBackDelg(delegate (string failMessage) 
                                { 
                                    App.MainScreenControl.Alert(detailMsg: failMessage); 
                                }),
                            waitDelaySec: 25);
                }
                catch (Exception ex)
                {
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10001113)");
                    App.Log.LogError(_logChannel, "", new Exception("(EXIT10001113)", ex), "EX01", "pgKomuter.QueryTicketTypePackage");
                }
            }
            return;
        }

        public void UpdateTicketTypePackage(UIKomuterTicketTypePackageAck uiTickPack)
        {
            KomuterTicketTypePackageResult res = (KomuterTicketTypePackageResult)uiTickPack.MessageData;

            KomuterSummaryModel _latestTicketTypePackageSummary = res.Data;

            _currencyId = _latestTicketTypePackageSummary.MCurrencies_Id;

            _mapPage.ActivateReset();
            _salesPanelPage.ShowTicketPackage(_latestTicketTypePackageSummary);
        }

        public void BTnGShowPaymentInfo(IKioskMsg kioskMsg)
        {
            _bTnGPaymentStaff.BTnGShowPaymentInfo(kioskMsg);
        }

        private void PrintTicket(UIKomuterCompletePaymentAck uiCompltResult, CreditCardResponse creditCardAnswer, string bTnGSaleTransNo)
        {
            UserSession session = uiCompltResult.Session;
            Reports.RdlcRendering rpRen = null;
            DSCreditCardReceipt.DSCreditCardReceiptDataTable dtPaywave = null;

            try
            {
                App.Log.LogText(_logChannel, _ktmbSalesTransactionNo ?? "#?#", "Debug:Print01-Start", "DBG01", classNMethodName: "pgKomuter.PrintTicket", messageType: AppDecorator.Log.MessageType.Debug);

                CompleteKomuterPaymentResult transCompStt = (CompleteKomuterPaymentResult)uiCompltResult.MessageData;

                rpRen = new Reports.RdlcRendering(App.ReportPDFFileMan.TicketFolderPath);

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
                    rw.AmountString = $@"{_finalCurrency} {_finalTotalAmount:#,###.00}";
                    rw.MachineId = uiCompltResult.MachineId;
                    rw.RefNumber = _ktmbSalesTransactionNo;
                    dtPaywave.Rows.Add(rw);
                    dtPaywave.AcceptChanges();
                }

                

                /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                ///// Phase 1 Print Solution
                //Thread createPDFThreadWorker = new Thread(new ThreadStart(new Action(() => {
                //    try
                //    {
                //        //Create Receipt
                //        string resultFileName1 = rpRen.RenderReportFile(Reports.RdlcRendering.RdlcOutputFormat.PotraitTicketPDF1, App.ExecutionFolderPath + @"\Reports\KTMB",
                //            CreditCardReceiptReportSourceName, _salesTransactionNo, "DataSet1", dt, null);

                //        // Render Ticket
                //        string resultFileName2 = rpRen.RenderReportFile(Reports.RdlcRendering.RdlcOutputFormat.PotraitPDF3INx3IN, App.ExecutionFolderPath + @"\Reports\KTMB\KomuterTicket",
                //            TicketReportSourceName, _salesTransactionNo, "DataSet1", new List<KomuterPrintTicketModel>(transCompStt.Data.KomuterTicketPrintList), null);

                //        App.Log.LogText(_logChannel, _salesTransactionNo, "Start to print ticket", "A02", classNMethodName: "pgKomuter.PrintTicket",
                //            adminMsg: "Start to print ticket");

                //        //string[] pdfFileList = new string[] { resultFileName1, resultFileName2 };
                //        //PDFTools.PrintPDFs_2(pdfFileList, _salesTransactionNo, App.MainScreenControl.MainFormDispatcher);
                //    }
                //    catch (Exception ex)
                //    {
                //        App.ShowDebugMsg("Error on pgKomuter.PrintTicket->createPDFThreadWorker; EX11");
                //        App.Log.LogError(_logChannel, _salesTransactionNo, ex, "EX11", "pgKomuter.PrintTicket->createPDFThreadWorker",
                //            adminMsg: $@"Error when printing; {ex.Message}");
                //    }
                //})));
                //createPDFThreadWorker.IsBackground = true;
                //createPDFThreadWorker.Start();
                /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                ///// Phase 2 Print Solution

                App.Log.LogText(_logChannel, _ktmbSalesTransactionNo ?? "#?#", "Debug:Print01-Start Render Data", "DBG05", classNMethodName: "pgKomuter.PrintTicket", messageType: AppDecorator.Log.MessageType.Debug);

                ReportImageSize receiptSize = new ReportImageSize(3.2M, 8.2M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
                Stream[] streamReceiptList = null;
                if (dtPaywave != null)
                {
                    LocalReport receiptRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\KTMB\{CreditCardReceiptReportSourceName}.rdlc",
                    new ReportDataSource[] { new ReportDataSource("DataSet1", (DataTable)dtPaywave) });
                    streamReceiptList = RdlcImageRendering.Export(receiptRep, receiptSize);
                }
                
                LocalReport ticketRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\KTMB\KomuterTicket\{TicketReportSourceName}.rdlc",
                    new ReportDataSource[] { new ReportDataSource("DataSet1", new List<KomuterPrintTicketModel>(transCompStt.Data.KomuterTicketPrintList)) });
                ReportImageSize ticketSize = new ReportImageSize(3M, 3M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
                Stream[] streamticketList = RdlcImageRendering.Export(ticketRep, ticketSize);

                App.Log.LogText(_logChannel, _ktmbSalesTransactionNo ?? "#?#", "Debug:Print01-End Render Data", "DBG10", classNMethodName: "pgKomuter.PrintTicket", messageType: AppDecorator.Log.MessageType.Debug);

                ImagePrintingTools.InitService();
                ImagePrintingTools.AddPrintDocument(streamticketList, _ktmbSalesTransactionNo, ticketSize);

                if (streamReceiptList != null)
                    ImagePrintingTools.AddPrintDocument(streamReceiptList, _ktmbSalesTransactionNo, receiptSize);

                App.Log.LogText(_logChannel, _ktmbSalesTransactionNo, "Start to print ticket", "A02", classNMethodName: "pgKomuter.PrintTicket",
                    adminMsg: "Start to print ticket");

                if (ImagePrintingTools.ExecutePrinting(_ktmbSalesTransactionNo) == false)
                {
                    throw new Exception("Error: Printer Busy; EXP01");
                }

                /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                App.ShowDebugMsg("Should be printed; pgKomuter.PrintTicket");
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgKomuter.PrintTicket; EX01");
                App.Log.LogError(_logChannel, _ktmbSalesTransactionNo, ex, "EX01", "pgKomuter.PrintTicket",
                    adminMsg: $@"Error when printing; {ex.Message}");
            }
        }

        /// <summary>
        /// Payment Success but finally data tranaction fail in web server.
        /// </summary>
        /// <param name="uiCompltResult"></param>
        private void PrintTicketError1(UIKomuterCompletePaymentAck uiCompltResult)
        {
            UserSession session = uiCompltResult.Session;
            string transactionNo = "-";

            Reports.RdlcRendering rpRen = null;

            try
            {
                transactionNo = _ktmbSalesTransactionNo;
                rpRen = new Reports.RdlcRendering(App.ReportPDFFileMan.TicketFolderPath);

                DsMelakaCentralErrorTicketMessage dsX = ReportDataQuery.GetTicketErrorDataSet(transactionNo, TerminalVerticalLogoPath);

                /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                ///// Phase 1 Print Solution
                //Thread createPDFThreadWorker = new Thread(new ThreadStart(new Action(() => {
                //    try
                //    {
                //        string resultFileName = rpRen.RenderReportFile(Reports.RdlcRendering.RdlcOutputFormat.PotraitTicketErrorPDF1, App.ExecutionFolderPath + @"\Reports\KTMB\IntercityETSTicket",
                //    TicketErrorReportSourceName, transactionNo, "DataSet1", dsX.Tables[0], null);

                //        App.Log.LogText(_logChannel, transactionNo, "Start to print fail transaction notice", "A02", classNMethodName: "pgKomuter.PrintTicketError1",
                //                adminMsg: "Start to print fail transaction notice");
                //        //PDFTools.PrintPDFs(resultFileName, transactionNo, App.MainScreenControl.MainFormDispatcher);
                //    }
                //    catch (Exception ex)
                //    {
                //        App.ShowDebugMsg("Error on pgKomuter.PrintTicketError1->createPDFThreadWorker; EX11");
                //        App.Log.LogError(_logChannel, _salesTransactionNo, ex, "EX11", "pgKomuter.PrintTicketError1->createPDFThreadWorker",
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

                App.Log.LogText(_logChannel, transactionNo, "Start to print fail transaction notice", "A02", classNMethodName: "pgKomuter.PrintTicketError1",
                        adminMsg: "Start to print fail transaction notice(1)");

                if (ImagePrintingTools.ExecutePrinting($@"{transactionNo}-Error(A)") == false)
                {
                    throw new Exception("Error: Printer Busy; EXP01");
                }

                /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                App.ShowDebugMsg("Should be printed (Error Msg); pgKomuter.PrintTicketError1");
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgKomuter.PrintTicketError1; EX01");
                App.Log.LogError(_logChannel, transactionNo, ex, "EX01", "pgKomuter.PrintTicketError1",
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
                //    TicketErrorReportSourceName, transactionNo, "DataSet1", dsX.Tables[0], null);

                //        App.Log.LogText(_logChannel, transactionNo, "Start to print fail transaction notice", "A02", classNMethodName: "pgKomuter.PrintTicketError2",
                //                adminMsg: "Start to print fail transaction notice");

                //        //PDFTools.PrintPDFs(resultFileName, transactionNo, App.MainScreenControl.MainFormDispatcher);
                //    }
                //    catch (Exception ex)
                //    {
                //        App.ShowDebugMsg("Error on pgKomuter.PrintTicketError2->createPDFThreadWorker; EX11");
                //        App.Log.LogError(_logChannel, _salesTransactionNo, ex, "EX11", "pgKomuter.PrintTicketError2->createPDFThreadWorker",
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

                App.Log.LogText(_logChannel, transactionNo, "Start to print fail transaction notice", "A02", classNMethodName: "pgKomuter.PrintTicketError2",
                        adminMsg: "Start to print fail transaction notice(2)");

                if (ImagePrintingTools.ExecutePrinting($@"{transactionNo}-Error(B)") == false)
                {
                    throw new Exception("Error: Printer Busy; EXP01");
                }
                /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                App.ShowDebugMsg("Should be printed (Error Msg); pgKomuter.PrintTicketError2");
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgKomuter.PrintTicketError2; EX01");
                App.Log.LogError(_logChannel, transactionNo, ex, "EX01", "pgKomuter.PrintTicketError2",
                    adminMsg: $@"Error when printing -fail transaction notice-; {ex.Message}");
            }
        }		
	}
}