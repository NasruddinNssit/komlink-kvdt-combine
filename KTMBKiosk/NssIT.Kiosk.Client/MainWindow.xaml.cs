using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Client.ViewPage;
using NssIT.Kiosk.Client.ViewPage.Alert;
using NssIT.Kiosk.Client.ViewPage.CustInfo;
using NssIT.Kiosk.Client.ViewPage.Date;
using NssIT.Kiosk.Client.ViewPage.Insurance;
using NssIT.Kiosk.Client.ViewPage.Intro;
using NssIT.Kiosk.Client.ViewPage.Komuter;
using NssIT.Kiosk.Client.ViewPage.Language;
using NssIT.Kiosk.Client.ViewPage.Menu.Section;
using NssIT.Kiosk.Client.ViewPage.Navigator;
using NssIT.Kiosk.Client.ViewPage.Payment;
using NssIT.Kiosk.Client.ViewPage.PickupNDrop;
using NssIT.Kiosk.Client.ViewPage.PopupWindow;
using NssIT.Kiosk.Client.ViewPage.Seat;
using NssIT.Kiosk.Client.ViewPage.StationTerminal;
using NssIT.Kiosk.Client.ViewPage.TicketSummary;
using NssIT.Kiosk.Client.ViewPage.Trip;
using NssIT.Kiosk.Device.PAX.IM30.IM30PayApp;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NssIT.Kiosk.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml; ClassCode:EXIT80.02
    /// </summary>
    public partial class MainWindow : Window, IMainScreenControl
    {
        private const string LogChannel = "MainWindow";

        private const double _menuWidthNormal = 250D;
        private const double _menuWidthPayment = 500D;

        // xxxxx Pages xxxxx
        public INav MiniNavigator { get; set; }
        private IAlertPage AlertPage { get; set; }
        private IIntro IntroPage { get; set; }
        private IStation StationPage { get; set; }
        private ITravelDate DatePage { get; set; }
        private ITrip TripPage { get; set; }
        private ISeat2 SeatPage { get; set; }
        private IPickupNDrop PickupNDropPage { get; set; }
        private ICustInfo PassengerInfoPage { get; set; }
        private IInsurance InsurancePage { get; set; }
        private IPayment PaymentPage { get; set; }
        public ITicketSummary TicketSummary { get; set; }
        private pgLanguage LanguagePage { get; set; }
        //xx
        private IKomuter KomuterPage { get; set; }
        //xx
        private IMaintenance MaintenancePage { get; set; }
        private ITimeout TimeoutPage { get; set; }
        //public IInfo UserInfo { get; set; }

        // xxxxx X xxxxx xxxxx X xxxxx

        private CultureInfo _provider = CultureInfo.InvariantCulture;

        public MainWindow()
        {
            InitializeComponent();

            MaintenancePage = new pgUnderMaintenance();
            AlertPage = new pgOutofOrder();
            StationPage = new pgStation();

            IntroPage = new pgIntro();
            LanguagePage = new pgLanguage();

            MiniNavigator = new pgNavigator();

            TicketSummary = new pgTickSumm();

            //UserInfo = new pgInfo();
            DatePage = new pgDate();

            TripPage = new pgTrip();

            PickupNDropPage = new pgPickupNDrop();
            PassengerInfoPage = new pgCustInfo();
            InsurancePage = new pgInsurance();
            PaymentPage = new pgPayment();

            SeatPage = new pgSeat2();

            KomuterPage = new pgKomuter();

            TimeoutPage = new pgTimeout();

            MiniNavigator.OnPageNavigateChanged += MiniNavigator_OnPageNavigateChanged;
        }

        public void InitiateMaintenance(PayWaveSettlementScheduler cardSettScheduler)
        {
            MaintenancePage.InitMaintenance(cardSettScheduler);
            cardSettScheduler.OnRequestSettlement += IntroPage.MaintenanceScheduler_OnRequestSettlement;
            cardSettScheduler.OnSettlementDone += IntroPage.MaintenanceScheduler_OnSettlementDone;
            cardSettScheduler.OnSettlementDone += MaintenancePage.MaintenanceScheduler_OnSettlementDone;

            cardSettScheduler.Load(MaintenancePage.RequestOutstandingSettlementInfoHandle, MaintenancePage.UpdateSettlementInfoHandle);
        }

        public void AcknowledgeOutstandingCardSettlement(UISalesCheckOutstandingCardSettlementAck outstandingCardSettlement)
        {
            try
            {
                //_editSalesDetailFlag = false;
                App.ShowDebugMsg("MainWindow.AcknowledgeCardOutstandingSettlement");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    MaintenancePage.ProceedOutstandingMaintenance(outstandingCardSettlement);
                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000328)", ex), classNMethodName: "MainWindow.AcknowledgeCardOutstandingSettlement");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000328)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.AcknowledgeCardOutstandingSettlement;");
            }
        }

        public void AcknowledgeCardSettlementDBTransStatus(UISalesCardSettlementStatusAck cardSettSttAck)
        {
            try
            {
                //_editSalesDetailFlag = false;
                App.ShowDebugMsg("MainWindow.AcknowledgeCardOutstandingSettlement");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    MaintenancePage.CardSettlementStatusAcknowledge(cardSettSttAck);
                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000340)", ex), classNMethodName: "MainWindow.AcknowledgeCardOutstandingSettlement");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000340)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.AcknowledgeCardOutstandingSettlement;");
            }
        }

        public void ToTopMostScreenLayer()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.Topmost = true;
            }));
            System.Windows.Forms.Application.DoEvents();
        }

        public void ToNormalScreenLayer()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.Topmost = false;
            }));
            System.Windows.Forms.Application.DoEvents();
        }

        public Dispatcher MainFormDispatcher { get => this.Dispatcher; }


        private void MiniNavigator_OnPageNavigateChanged(object sender, MenuItemPageNavigateEventArgs e)
        {
            try
            {
                SubmitOnPageNavigateChanged(e.PageNavigateDirection);
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception("(EXIT10000313)", ex), "EX01", classNMethodName: "MainWindow.ExecMenu_OnEditMenuItem");
                App.MainScreenControl.Alert(detailMsg: $@"Error on language selection; (EXIT10000313)");
            }
        }

        private void SubmitOnPageNavigateChanged(PageNavigateDirection navigateDirection)
        {
            if (frmWorkDetail.Content is IKioskViewPage kiospPage)
                kiospPage.ShieldPage();

            System.Windows.Forms.Application.DoEvents();

            Thread submitWorker = new Thread(new ThreadStart(new Action(() =>
            {
                try
                {
                    App.NetClientSvc.SalesService.NavigateToPage(navigateDirection);
                }
                catch (Exception ex)
                {
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000316)");
                    App.Log.LogError(LogChannel, "", new Exception("(EXIT10000316)", ex), "EX01", "MainWindow.SubmitOnPageNavigateChanged");
                }
            })));
            submitWorker.IsBackground = true;
            submitWorker.Start();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) { }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            App.ShutdownX();
        }

        private void HideTicketSummary()
        {
            if (frmWorkTicketSummary.Content != null)
            {
                try
                {
                    frmWorkTicketSummary.Content = null;
                }
                catch { }
            }
        }

        public void ShowMaintenance()
        {
            try
            {
                App.Log.LogText(LogChannel, "-", "ShowMaintenance", classNMethodName: "MainWindow.ShowMaintenance");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    HideTicketSummary();
                    frmWorkDetail.Content = null;
                    frmWorkDetail.NavigationService.RemoveBackEntry();
                    System.Windows.Forms.Application.DoEvents();

                    frmWorkDetail.NavigationService.Navigate(MaintenancePage);
                    System.Windows.Forms.Application.DoEvents();
                }));

            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000329)", ex), classNMethodName: "MainWindow.ShowMaintenance");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000329)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.ShowMaintenance;");
            }
        }


        private bool _isFirstWelcome = true;
        public void ShowWelcome()
        {
            try
            {
                App.Log.LogText(LogChannel, "-", "XXXXXXXXXXXXXXXXXXXX Welcome XXXXXXXXXXXXXXXXXXXX", classNMethodName: "MainWindow.ShowWelcome");

                UserSession latestUserSession = App.LatestUserSession;

                App.LatestUserSession = null;

                this.Dispatcher.Invoke(new Action(() =>
                {

                    //frmWorkDetail.NavigationService.Content.GetType().Name.ToString()

                    if (frmWorkDetail.NavigationService?.Content?.GetType().Name.ToString().Equals(IntroPage.GetType().Name.ToString()) == true)
                    {
                        string debugStr = "*";
                        // By Pass
                    }
                    else
                    {
                        //Reports.PDFTools.KillAdobe("AcroRd32");

                        App.BookingTimeoutMan.ResetCounter();

                        EndUserSession(latestUserSession, _isFirstWelcome);

                        UnLoadTimeoutNoticePage();

                        HideTicketSummary();
                        frmWorkDetail.Content = null;
                        frmWorkDetail.NavigationService.RemoveBackEntry();
                        System.Windows.Forms.Application.DoEvents();

                        IntroPage.InitPage();
                        frmWorkDetail.NavigationService.Navigate(IntroPage);
                        System.Windows.Forms.Application.DoEvents();

                        App.IsAutoTimeoutExtension = false;
                    }
                }));

                App.PaymentScrImage.DeleteHistoryFile();
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000301)", ex), classNMethodName: "MainWindow.ShowWelcome");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000301)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.ShowWelcome;");
            }
            finally
            {
                _isFirstWelcome = false;
            }

            return;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            void EndUserSession(UserSession latestUserSession, bool isFirstWelcome)
            {
                if ((latestUserSession?.Expired == false) || isFirstWelcome)
                {
                    Thread submitWorker = new Thread(new ThreadStart(new Action(() =>
                    {
                        try
                        {
                            App.NetClientSvc.SalesService.EndUserSession();
                        }
                        catch (Exception ex)
                        {
                            App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000330)");
                            App.Log.LogError(LogChannel, "", new Exception("(EXIT10000330)", ex), "EX01", "MainWindow.EndUserSession");
                        }
                    })));
                    submitWorker.IsBackground = true;
                    submitWorker.Start();

                    if (App.LatestUserSession != null)
                        App.LatestUserSession.Expired = true;

                    Thread.Sleep(300);
                }
            }
        }

        public void ChooseLanguage(UILanguageSelectionAck lang)
        {
            try
            {
                //_editSalesDetailFlag = false;
                App.ShowDebugMsg("ChoiseLanguage");

                if (string.IsNullOrWhiteSpace(lang.ErrorMessage) == false)
                    throw new Exception(lang.ErrorMessage);

                this.Dispatcher.Invoke(new Action(() =>
                {
                    HideTicketSummary();

                    frmWorkDetail.Content = null;
                    frmWorkDetail.NavigationService.RemoveBackEntry();
                    System.Windows.Forms.Application.DoEvents();

                    frmWorkDetail.NavigationService.Navigate(LanguagePage);
                    System.Windows.Forms.Application.DoEvents();
                }));

            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000302)", ex), classNMethodName: "MainWindow.StartNewSales");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000302)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.ChooseLanguage;");
            }
        }

        public void ChooseDestinationStation(UIDestinationListAck uiDest, UserSession session)
        {
            try
            {
                //_editSalesDetailFlag = false;
                App.ShowDebugMsg("ChooseDestinationStation");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    //UserInfo.ShowInfo(InfoCode.DestinationInfo, session.Language);

                    DisplayTicketSummary(session, TickSalesMenuItemCode.ToStation);

                    StationPage.InitStationData(uiDest);
                    frmWorkDetail.Content = null;
                    frmWorkDetail.NavigationService.RemoveBackEntry();
                    System.Windows.Forms.Application.DoEvents();

                    frmWorkDetail.NavigationService.Navigate(StationPage);
                    //frmWorkDetail.Content

                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000303)", ex), classNMethodName: "MainWindow.ChooseDestinationStation");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000303)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.ChooseDestinationStation;");
            }
        }

        public void ChooseOriginStation(UIOriginListAck uiOrig, UserSession session)
        {
            try
            {
                //_editSalesDetailFlag = false;
                App.ShowDebugMsg("ChooseOriginStation");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    //UserInfo.ShowInfo(InfoCode.OriginInfo, session.Language);

                    DisplayTicketSummary(session, TickSalesMenuItemCode.FromStation);

                    StationPage.InitStationData(uiOrig);

                    frmWorkDetail.Content = null;
                    frmWorkDetail.NavigationService.RemoveBackEntry();
                    System.Windows.Forms.Application.DoEvents();

                    frmWorkDetail.NavigationService.Navigate(StationPage);
                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000303)", ex), classNMethodName: "MainWindow.ChooseDestinationStation");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000303)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.ChooseDestinationStation;");
            }
        }

        public void ChooseTravelDates(UITravelDatesEnteringAck uiTravelDate, UserSession session)
        {
            try
            {
                //_editSalesDetailFlag = false;
                App.ShowDebugMsg("ChooseTravelDates");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    //UserInfo.ShowInfo(InfoCode.TravelDateInfo, session.Language);

                    DisplayTicketSummary(session, TickSalesMenuItemCode.TravelDates);

                    DatePage.InitData(session);

                    frmWorkDetail.Content = null;
                    frmWorkDetail.NavigationService.RemoveBackEntry();
                    System.Windows.Forms.Application.DoEvents();

                    frmWorkDetail.NavigationService.Navigate(DatePage);
                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000304)", ex), classNMethodName: "MainWindow.ChooseDestinationStation");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000304)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.ChooseTravelDates;");
            }
        }

        public void ShowInitDepartTrip(UIDepartTripInitAck tripInit, UserSession session)
        {
            try
            {
                //_editSalesDetailFlag = false;
                App.ShowDebugMsg("MainWindow.ShowInitDepartTrip");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    //UserInfo.ShowInfo(InfoCode.DepartTripInfo, session.Language);

                    DisplayTicketSummary(session, TickSalesMenuItemCode.DepartTrip);

                    TripPage.InitData(session, TravelMode.DepartOnly);

                    frmWorkDetail.Content = null;
                    frmWorkDetail.NavigationService.RemoveBackEntry();
                    System.Windows.Forms.Application.DoEvents();

                    frmWorkDetail.NavigationService.Navigate(TripPage);
                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000308)", ex), classNMethodName: "MainWindow.ShowInitDepartTrip");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000308)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.ShowInitDepartTrip;");
            }
        }

        public void UpdateDepartTripList(UIDepartTripListAck uiDepartTripList, UserSession session)
        {
            try
            {
                //_editSalesDetailFlag = false;
                App.ShowDebugMsg("MainWindow.UpdateDepartTripList");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    TripPage.UpdateDepartTripList(uiDepartTripList, session);
                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000309)", ex), classNMethodName: "MainWindow.UpdateDepartTripList");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000309)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.UpdateDepartTripList;");
            }
        }

        public void ShowInitReturnTrip(UIReturnTripInitAck tripInit, UserSession session)
        {
            try
            {
                //_editSalesDetailFlag = false;
                App.ShowDebugMsg("MainWindow.ShowInitReturnTrip");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    //UserInfo.ShowInfo(InfoCode.ReturnTripInfo, session.Language);

                    DisplayTicketSummary(session, TickSalesMenuItemCode.ReturnTrip);

                    TripPage.InitData(session, TravelMode.ReturnOnly);

                    frmWorkDetail.Content = null;
                    frmWorkDetail.NavigationService.RemoveBackEntry();
                    System.Windows.Forms.Application.DoEvents();

                    frmWorkDetail.NavigationService.Navigate(TripPage);
                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000322)", ex), classNMethodName: "MainWindow.ShowInitReturnTrip");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000322)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.ShowInitReturnTrip;");
            }
        }

        public void UpdateReturnTripList(UIReturnTripListAck uiReturnTripList, UserSession session)
        {
            try
            {
                //_editSalesDetailFlag = false;
                App.ShowDebugMsg("MainWindow.UpdateReturnTripList");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    TripPage.UpdateReturnTripList(uiReturnTripList, session);
                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000323)", ex), classNMethodName: "MainWindow.UpdateDepartTripList");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000323)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.UpdateDepartTripList;");
            }
        }

        public void UpdateDepartTripSubmitError(UIDepartTripSubmissionErrorAck uiTripSubErr, UserSession session)
        {
            try
            {
                Thread thrWorker = new Thread(new ThreadStart(new Action(() =>
                {
                    try
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            TripPage.UpdateShieldErrorMessage(uiTripSubErr.ErrorMessage);
                            System.Windows.Forms.Application.DoEvents();

                            Task.Delay(1000 * 10).Wait();
                            TripPage.ResetPageAfterError();
                            System.Windows.Forms.Application.DoEvents();
                        }));

                        App.TimeoutManager.ResetTimeout();
                    }
                    catch (Exception ex)
                    {
                        App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000317)", ex), classNMethodName: "MainWindow.UpdateDepartTripSubmitError");
                        Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000317)");
                        App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.UpdateDepartTripSubmitError; (EXIT10000317)");
                    }
                })));
                thrWorker.IsBackground = true;
                thrWorker.Start();
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000318)", ex), classNMethodName: "MainWindow.UpdateDepartTripSubmitError");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000318)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.UpdateDepartTripSubmitError; (EXIT10000318)");
            }

        }

        public void ChooseDepartSeat(UIDepartSeatListAck uiDepartSeatList, UserSession session)
        {
            try
            {
                //_editSalesDetailFlag = false;
                App.ShowDebugMsg("ChooseDestinationStation");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    //UserInfo.ShowInfo(InfoCode.DepartSeatInfo, session.Language);

                    DisplayTicketSummary(session, TickSalesMenuItemCode.DepartSeat);

                    // ---------------------------------------------------
                    //Actual Codes
                    SeatPage.InitDepartSeatData(uiDepartSeatList, session);

                    frmWorkDetail.Content = null;
                    frmWorkDetail.NavigationService.RemoveBackEntry();
                    System.Windows.Forms.Application.DoEvents();

                    frmWorkDetail.NavigationService.Navigate(SeatPage);
                    // ---------------------------------------------------

                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000304)", ex), classNMethodName: "MainWindow.ChooseDestinationStation");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000304)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.ChooseDepartSeat;");
            }
        }

        public void ChooseReturnSeat(UIReturnSeatListAck uiReturnSeatList, UserSession session)
        {
            try
            {
                //_editSalesDetailFlag = false;
                App.ShowDebugMsg("ChooseDestinationStation");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    //UserInfo.ShowInfo(InfoCode.ReturnSeatInfo, session.Language);

                    DisplayTicketSummary(session, TickSalesMenuItemCode.ReturnSeat);

                    // ---------------------------------------------------
                    //Actual Codes
                    SeatPage.InitReturnSeatData(uiReturnSeatList, session);

                    frmWorkDetail.Content = null;
                    frmWorkDetail.NavigationService.RemoveBackEntry();
                    System.Windows.Forms.Application.DoEvents();

                    frmWorkDetail.NavigationService.Navigate(SeatPage);
                    // ---------------------------------------------------

                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000324)", ex), classNMethodName: "MainWindow.ChooseDestinationStation");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000324)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.ChooseReturnSeat;");
            }
        }

        public void ChoosePickupNDrop(UIDepartPickupNDropAck uiIDepartPickupNDrop)
        {
            try
            {
                //_editSalesDetailFlag = false;
                App.ShowDebugMsg("ChoosePickupNDrop");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    //UserInfo.ShowInfo(InfoCode.DepartPickupNDrop, uiIDepartPickupNDrop.Session.Language);

                    DisplayTicketSummary(uiIDepartPickupNDrop.Session, TickSalesMenuItemCode.ToStation);

                    PickupNDropPage.InitPickupDropData(uiIDepartPickupNDrop);

                    frmWorkDetail.Content = null;
                    frmWorkDetail.NavigationService.RemoveBackEntry();
                    System.Windows.Forms.Application.DoEvents();

                    frmWorkDetail.NavigationService.Navigate(PickupNDropPage);
                    // ---------------------------------------------------

                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000304)", ex), classNMethodName: "MainWindow.ChooseDestinationStation");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000304)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.ChoosePickupNDrop;");
            }
        }

        public void EnterPassengerInfo(UICustInfoAck uiCustInfo)
        {
            try
            {
                //_editSalesDetailFlag = false;
                App.ShowDebugMsg("EnterPassengerInfo");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    //UserInfo.ShowInfo(InfoCode.PassengerInfo, uiCustInfo.Session.Language);

                    DisplayTicketSummary(uiCustInfo.Session, TickSalesMenuItemCode.Passenger);

                    PassengerInfoPage.InitPassengerInfo(uiCustInfo);

                    frmWorkDetail.Content = null;
                    frmWorkDetail.NavigationService.RemoveBackEntry();
                    System.Windows.Forms.Application.DoEvents();

                    frmWorkDetail.NavigationService.Navigate(PassengerInfoPage);
                    // ---------------------------------------------------

                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000310)", ex), classNMethodName: "MainWindow.EnterPassengerInfo");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000310)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.EnterPassengerInfo;");
            }
        }

        public void UpdatePromoCodeVerificationResult(UICustPromoCodeVerifyAck codeVerificationAnswer)
        {
            try
            {
                App.ShowDebugMsg("SetPromoCodeVerificationResult");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    PassengerInfoPage.SetPromoCodeVerificationResult(codeVerificationAnswer);
                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000335)", ex), classNMethodName: "MainWindow.UpdatePromoCodeVerificationResult");
                //Alert(detailMsg: $@"Error: {ex.Message}; (EXIT100003xx)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.UpdatePromoCodeVerificationResult;");
            }
        }

        public void UpdatePNRTicketTicketResult(UICustInfoPNRTicketTypeAck custPNRTicketTypeResult)
        {
            try
            {
                App.ShowDebugMsg("UpdatePNRTicketTicketResult");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    PassengerInfoPage.UpdatePNRTicketTypeResult(custPNRTicketTypeResult);
                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000337)", ex), classNMethodName: "MainWindow.UpdatePNRTicketTicketResult");
                //Alert(detailMsg: $@"Error: {ex.Message}; (EXIT100003xx)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.UpdatePNRTicketTicketResult;");
            }
        }

        public void RequestAmentPassengerInfo(UICustInfoUpdateFailAck uiFailUpdateCustInfo)
        {
            try
            {
                App.ShowDebugMsg("RequestAmentPassengerInfo");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    PassengerInfoPage.RequestAmentPassengerInfo(uiFailUpdateCustInfo);
                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000336)", ex), classNMethodName: "MainWindow.RequestAmentPassengerInfo");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.RequestAmentPassengerInfo;");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000336)");
            }
        }

        public void ChooseInsurance(UIETSInsuranceListAck uiEstInsrLst)
        {
            try
            {
                //_editSalesDetailFlag = false;
                App.ShowDebugMsg("ChooseInsurance");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    //UserInfo.ShowInfo(InfoCode.PassengerInfo, uiCustInfo.Session.Language);

                    DisplayTicketSummary(uiEstInsrLst.Session, TickSalesMenuItemCode.Insurance);

                    InsurancePage.InitInsuranceInfo(uiEstInsrLst);

                    frmWorkDetail.Content = null;
                    frmWorkDetail.NavigationService.RemoveBackEntry();
                    System.Windows.Forms.Application.DoEvents();

                    frmWorkDetail.NavigationService.Navigate(InsurancePage);
                    // ---------------------------------------------------

                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000341)", ex), classNMethodName: "MainWindow.ChooseInsurance");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000341)");
                App.ShowDebugMsg($@"Error: {ex.Message}; (EXIT10000341); At MainWindow.ChooseInsurance;");
            }
        }

        /// <summary>
        /// This method is refer to KTM Komuter
        /// </summary>
        /// <param name="uiCustInfo"></param>
        public void StartSelling(UISalesStartSellingAck uiStartSales)
        {
            try
            {
                App.ShowDebugMsg("MainWindow.StartSelling");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    HideTicketSummary();
                    frmWorkDetail.Content = null;
                    frmWorkDetail.NavigationService.RemoveBackEntry();
                    System.Windows.Forms.Application.DoEvents();

                    KomuterPage.InitData(uiStartSales);
                    frmWorkDetail.NavigationService.Navigate(KomuterPage);
                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000325)", ex), classNMethodName: "MainWindow.StartSelling");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000325)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.StartSelling;");
            }
        }

        public void UpdateKomuterTicketTypePackage(UIKomuterTicketTypePackageAck uiTickPack)
        {
            try
            {
                App.ShowDebugMsg("MainWindow.UpdateKomuterTicketTypePackage");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    KomuterPage.UpdateTicketTypePackage(uiTickPack);
                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000326)", ex), classNMethodName: "MainWindow.UpdateKomuterTicketTypePackage");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000326)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.UpdateKomuterTicketTypePackage;");
            }
        }

        public void UpdateKomuterTicketBooking(UIKomuterTicketBookingAck uiTickBook)
        {
            try
            {
                App.ShowDebugMsg("MainWindow.UpdateKomuterTicketBooking");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    KomuterPage.UpdateBookingData(uiTickBook);
                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000327)", ex), classNMethodName: "MainWindow.UpdateKomuterTicketBooking");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000327)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.UpdateKomuterTicketBooking;");
            }
        }

        public void UpdateKomuterBookingCheckoutResult(UIKomuterBookingCheckoutAck bookingCheckoutAck)
        {
            try
            {
                App.ShowDebugMsg("MainWindow.UpdateKomuterBookingCheckoutResult");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    KomuterPage.UpdateBookingCheckoutResult(bookingCheckoutAck);
                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000331)", ex), classNMethodName: "MainWindow.UpdateKomuterBookingCheckoutResult");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000331)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.UpdateKomuterBookingCheckoutResult;");
            }
        }

        public void UpdateKomuterTicketPaymentStatus(UIKomuterCompletePaymentAck paymentStatusAck)
        {
            try
            {
                App.ShowDebugMsg("MainWindow.UpdateKomuterTicketPaymentStatus");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    KomuterPage.UpdateKomuterTicketPaymentStatus(paymentStatusAck);
                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000332)", ex), classNMethodName: "MainWindow.UpdateKomuterTicketPaymentStatus");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000332)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.UpdateKomuterTicketPaymentStatus;");
            }
        }

        public void MakeTicketPayment(UISalesPaymentProceedAck uiSalesPayment)
        {
            string _transactionNo = null;

            try
            {
                App.ShowDebugMsg("MakeTicketPayment");

                _transactionNo = uiSalesPayment.Session.SeatBookingId;

                //if (App.SysParam.PrmNoPaymentNeed)
                //{
                //App.ShowDebugMsg($@"Received UIMakeSalesPaymentAck but PrmNoPaymentNeed set to true");
                //this.Dispatcher.Invoke(new Action(() =>
                //{
                //	DisplayTicketSummary(uiSalesPayment.Session, TickSalesMenuItemCode.Payment);
                //	// ---------------------------------------------------
                //	System.Windows.Forms.Application.DoEvents();
                //	//MessageBox.Show("Debug-Testing -- Payment Done");
                //}));

                //ShowWelcome();
                ////}
                //else
                //{
                //if (App.SysParam.PrmNoPaymentNeed)
                //{
                //	//By Pass
                //}
                //else
                //{
                Thread machChkThreadWorker = new Thread(new ThreadStart(new Action(() =>
                {
                    try
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            DisplayTicketSummary(uiSalesPayment.Session, TickSalesMenuItemCode.Payment);
                            PaymentPage.InitPayment(uiSalesPayment.Session);

                            frmWorkDetail.Content = null;
                            frmWorkDetail.NavigationService.RemoveBackEntry();
                            System.Windows.Forms.Application.DoEvents();

                            frmWorkDetail.NavigationService.Navigate(PaymentPage);
                            System.Windows.Forms.Application.DoEvents();
                        }));
                    }
                    catch (Exception ex)
                    {
                        Exception err2 = new Exception($@"{ex.Message};(EXIT10000320);", ex);
                        ReleaseTicketOnError(err2);
                    }
                })));

                machChkThreadWorker.IsBackground = true;
                machChkThreadWorker.Start();
                //}
                //}
            }
            catch (Exception ex)
            {
                Exception err2 = new Exception($@"{ex.Message}; (EXIT10000311);", ex);
                ReleaseTicketOnError(err2);
            }

            void ReleaseTicketOnError(Exception ex5)
            {
                string errEx = "";

                if (string.IsNullOrWhiteSpace(_transactionNo) == false)
                {
                    try
                    {
                        App.NetClientSvc.SalesService.RequestSeatRelease(_transactionNo);
                    }
                    catch (Exception ex2)
                    {
                        errEx = ex2.Message;
                    }
                    finally
                    {
                        if (string.IsNullOrWhiteSpace(errEx) == false)
                            errEx = $@"Error When Request for Seat Release; {errEx}";
                    }
                }

                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex5.Message}; {errEx};", ex5), classNMethodName: "MainWindow.MakeTicketPayment");
                Alert(detailMsg: $@"Error: {ex5.Message}; {errEx}; ");
                App.ShowDebugMsg($@"Error: {ex5.Message}; {errEx}; At MainWindow.MakeTicketPayment;");
            }
        }

        public void UpdateTransactionCompleteStatus(UICompleteTransactionResult uiCompltResult)
        {
            try
            {
                //_editSalesDetailFlag = false;
                App.ShowDebugMsg("MainWindow.UpdateTransactionCompleteStatus");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    DisplayTicketSummary(uiCompltResult.Session, TickSalesMenuItemCode.AfterPayment);
                    PaymentPage.UpdateTransCompleteStatus(uiCompltResult);
                }));

            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000309)", ex), classNMethodName: "MainWindow.UpdateDepartTripList");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000309)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.UpdateDepartTripList;");
            }
        }

        /// <summary>
        /// FuncCode:EXIT80.0250
        /// </summary>
        /// <param name="kioskMsg"></param>
        public void BTnGShowPaymentInfo(IKioskMsg kioskMsg)
        {
            try
            {
                App.ShowDebugMsg("MainWindow.BTnGShowPaymentInfo");

                this.Dispatcher.Invoke(new Action(() =>
                {

                    if (App.CurrentTransStage == Base.TicketTransactionStage.ETS)
                        PaymentPage.BTnGShowPaymentInfo(kioskMsg);

                    else if (App.CurrentTransStage == Base.TicketTransactionStage.Komuter)
                        KomuterPage.BTnGShowPaymentInfo(kioskMsg);

                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT80.0250.EX01)", ex), classNMethodName: "MainWindow.UpdateDepartTripList");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT80.0250.EX01)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.UpdateDepartTripList;");
            }
        }

        private object _popupWindowLock = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uiTimeoutWarn"></param>
        /// <param name="requestResultState"></param>
        /// <param name="isSuccess">This output is accurate when requestResultState is true</param>
        public void AcquireUserTimeoutResponse(UISalesTimeoutWarningAck uiTimeoutWarn, bool requestResultState, out bool? isSuccess)
        {
            bool? isSuccessX = null;
            isSuccess = null;

            Thread threadWorker = new Thread(new ThreadStart(AcquireUserTimeoutResponseThreadWorking));
            threadWorker.IsBackground = true;
            threadWorker.Start();

            if (requestResultState)
            {
                int waitDelaySec = 30;
                DateTime startTime = DateTime.Now;
                DateTime endTime = startTime.AddSeconds(waitDelaySec);

                while ((endTime.Subtract(DateTime.Now).TotalSeconds > 0) && (isSuccessX.HasValue == false))
                {
                    Task.Delay(100).Wait();
                }

                if (isSuccessX.HasValue == false)
                    isSuccessX = false;

                isSuccess = isSuccessX;
            }

            return;

            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void AcquireUserTimeoutResponseThreadWorking()
            {
                try
                {
                    App.ShowDebugMsg("MainWindow.AcquireUserTimeoutResponse");

                    lock (_popupWindowLock)
                    {
                        App.Log.LogText(LogChannel, "*", uiTimeoutWarn, "B01", "MainWindow.AcquireUserTimeoutResponseThreadWorking",
                            extraMsg: $@"Start - AcquireUserTimeoutResponseThreadWorking; requestResultState: {requestResultState};  MsgObj: UISalesTimeoutWarningAck");

                        this.Dispatcher.Invoke(new Action(() =>
                        {

                            if (App.IsAutoTimeoutExtension == true)
                            {
                                isSuccessX = true;
                                App.TimeoutManager.ResetTimeout();
                            }
                            else if (frmWorkDetail.NavigationService?.Content?.GetType().Name.ToString().Equals(IntroPage?.GetType().Name.ToString()) == true)
                            {
                                isSuccessX = false;
                            }
                            else if (frmPopupWindow.NavigationService?.Content?.GetType().Name.ToString().Equals(TimeoutPage.GetType().Name.ToString()) == true)
                            {
                                isSuccessX = false;
                            }
                            else
                            {
                                frmPopupWindow.Content = null;
                                frmPopupWindow.NavigationService.RemoveBackEntry();
                                frmPopupWindow.NavigationService.Content = null;

                                BdMainWindowSheild.Visibility = Visibility.Visible;
                                System.Windows.Forms.Application.DoEvents();

                                LanguageCode langg = LanguageCode.Malay;

                                if (App.LatestUserSession != null)
                                    langg = App.LatestUserSession.Language;

                                TimeoutPage.InitPageData(uiTimeoutWarn, langg);

                                frmPopupWindow.NavigationService.Navigate(TimeoutPage);
                                System.Windows.Forms.Application.DoEvents();

                                isSuccessX = true;
                            }
                        }));

                        App.Log.LogText(LogChannel, "*", $@"End - AcquireUserTimeoutResponseThreadWorking; isSuccessX: {isSuccessX}", "B10", "MainWindow.AcquireUserTimeoutResponseThreadWorking");
                    }
                }
                catch (Exception ex)
                {
                    App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000333)", ex), classNMethodName: "MainWindow.AcquireUserTimeoutResponse");
                    App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.AcquireUserTimeoutResponse;");
                    Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000333)");
                }
                finally
                {
                    if (isSuccessX.HasValue == false)
                        isSuccessX = false;
                }
            }
        }

        public void UnLoadTimeoutNoticePage()
        {
            try
            {
                ///// App.ShowDebugMsg("MainWindow.UnSheildTimeoutPage");

                this.Dispatcher.Invoke(new Action(() =>
                {
                    if (frmPopupWindow.NavigationService?.Content != null)
                    {
                        frmPopupWindow.Content = null;
                        frmPopupWindow.NavigationService.RemoveBackEntry();
                        frmPopupWindow.NavigationService.Content = null;

                        System.Windows.Forms.Application.DoEvents();

                        BdMainWindowSheild.Visibility = Visibility.Collapsed;
                        System.Windows.Forms.Application.DoEvents();

                        App.ShowDebugMsg("**>UnLoad Timeout Notice Page");
                    }
                }));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000334)", ex), classNMethodName: "MainWindow.UnSheildTimeoutPage");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.UnSheildTimeoutPage;");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000334)");
            }
        }

        //public void StartSelling(LanguageCode langCode)
        //{
        //	try
        //	{
        //		App.ShowDebugMsg($@"Language Code : {Enum.GetName(typeof(LanguageCode), langCode)}");

        //		this.Dispatcher.Invoke(new Action(() => { }));
        //		//frmWorkDetail.NavigationService.Navigate(_pgLanguage);
        //	}
        //	catch (Exception ex)
        //	{
        //		App.Log.LogError(LogChannel, "-", ex, classNMethodName: "MainWindow.StartSelling");
        //		App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.StartSelling;");
        //	}
        //}

        public void Alert(string malayShortMsg = "TIDAK BERFUNGSI", string engShortMsg = "OUT OF ORDER", string detailMsg = "")
        {
            try
            {
                App.Log.LogText(LogChannel, "*", "Running .. TowerLight.ShowErrorState", "DEBUG01", "MainWindow.Alert", AppDecorator.Log.MessageType.Debug);

                AlertPage.ShowAlertMessage(malayShortMsg, engShortMsg, detailMsg);
                App.ShowDebugMsg($@"Show Alert Page..");

                UnLoadTimeoutNoticePage();

                this.Dispatcher.Invoke(new Action(() =>
                {
                    if (frmWorkDetail.Content.GetType().IsEquivalentTo(AlertPage.GetType()) == false)
                    {
                        HideTicketSummary();

                        frmWorkDetail.Content = null;
                        frmWorkDetail.NavigationService.RemoveBackEntry();
                        System.Windows.Forms.Application.DoEvents();

                        frmWorkDetail.NavigationService.Navigate(AlertPage);
                        System.Windows.Forms.Application.DoEvents();
                    }
                }));

                //_editSalesDetailFlag = false;

                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", ex, classNMethodName: "MainWindow.StartSelling");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.Alert;");
            }
        }

        private void DisplayTicketSummary(UserSession session, TickSalesMenuItemCode currentEditItemCode)
        {
            try
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    if (frmWorkTicketSummary.Content == null)
                    {
                        frmWorkTicketSummary.NavigationService.Navigate(TicketSummary);
                        System.Windows.Forms.Application.DoEvents();
                    }
                }));

                TicketSummary.UpdateSummary(session, currentEditItemCode);
                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception($@"Error: {ex.Message}; (EXIT10000321)", ex), classNMethodName: "MainWindow.DisplayTicketSummary");
                Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000321)");
                App.ShowDebugMsg($@"Error: {ex.Message}; At MainWindow.DisplayTicketSummary;");
            }
        }

        public void UpdateDepartDate(DateTime newDepartDate)
        {
            TicketSummary.UpdateDepartDate(newDepartDate);
        }

        public void UpdateReturnDate(DateTime newReturnDate)
        {
            TicketSummary.UpdateReturnDate(newReturnDate);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Application.Current.Shutdown();
        }
    }
}
