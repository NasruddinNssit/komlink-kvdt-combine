using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Log.DB.StatusMonitor;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace NssIT.Kiosk.Client.ViewPage.Intro
{
    /// <summary>
    /// Interaction logic for pgIntro.xaml
    /// Job Group
    ///		# Check System healty
    ///		# Settlement
    ///		# Customer Detector
    ///		# Pax press button
    ///		# Tower Light
    ///		# Page Animation
    /// </summary>
    public partial class pgIntro : Page, IIntro
    {
        private const string LogChannel = "ViewPage";

        private IntroUIAnimateHelper _introAniHelp = null;

        private Thread _sysInitThreadWorker = null;

        private Thread _maintenanceThreadStartingWorker = null;
        //private DateTime _maintenanceThread_LastStartTime = DateTime.MinValue;
        //private int _maintenanceThread_MaximumExecuteTimeSec = 60;

        private static SemaphoreSlim _manLock = new SemaphoreSlim(1);
        private bool _maintenanceRequest = false;
        private bool _pageLoaded = false;

        private bool? _isSystemHealthy = null;
        private bool _isCustomerPresent = false;

        public pgIntro()
        {
            InitializeComponent();

            _introAniHelp = new IntroUIAnimateHelper(this, CvIntroFrame, ScvIntro);

            _introAniHelp.OnIntroBegin += _introAniHelp_OnIntroBegin;

            App.OnAbsentCustomer += App_OnAbsentCustomer;
            App.OnPresentCustomer += App_OnPresentCustomer;
        }

        private void App_OnPresentCustomer(object sender, EventArgs e)
        {
            try
            {
                _manLock.WaitAsync().Wait();

                if (_pageLoaded)
                    _isCustomerPresent = true;
            }
            catch
            { }
            finally
            {
                if (_manLock.CurrentCount == 0)
                    _manLock.Release();
            }
        }

        private void App_OnAbsentCustomer(object sender, EventArgs e)
        {
            try
            {
                _manLock.WaitAsync().Wait();

                if (_pageLoaded)
                    _isCustomerPresent = false;
            }
            catch
            { }
            finally
            {
                if (_manLock.CurrentCount == 0)
                    _manLock.Release();
            }
        }

        private DateTime _lastMaintenanceRequestTime = DateTime.MinValue;
        public void MaintenanceScheduler_OnRequestSettlement(object sender, EventArgs e)
        {
            try
            {
                _manLock.WaitAsync().Wait();

                if (_pageLoaded == false)
                    return;

                if ((_isSystemHealthy.HasValue && _isSystemHealthy.Value == true) || (_isSystemHealthy.HasValue == false))
                {
                    _maintenanceRequest = true;

                    int maxWaitingSec = 120;

                    // Check Existing Maintenance Request Thread Working
                    if (_maintenanceThreadStartingWorker != null)
                    {
                        if (((_maintenanceThreadStartingWorker?.ThreadState & ThreadState.Aborted) == ThreadState.Aborted)
                            || ((_maintenanceThreadStartingWorker?.ThreadState & ThreadState.Stopped) == ThreadState.Stopped)
                            )
                        {
                            _maintenanceThreadStartingWorker = null;
                        }
                        else
                        {
                            if (_maintenanceThreadStartingWorker != null)
                            {
                                DateTime expiredTime = _lastMaintenanceRequestTime.AddSeconds(maxWaitingSec);

                                if (expiredTime.Subtract(DateTime.Now).TotalSeconds <= 0)
                                {
                                    try
                                    {
                                        _maintenanceThreadStartingWorker?.Abort();
                                    }
                                    catch { /* By Pass Any Error */}
                                    Thread.Sleep(1000);

                                    _maintenanceThreadStartingWorker = null;
                                }
                            }
                        }
                    }

                    if (_maintenanceThreadStartingWorker == null)
                    {
                        _maintenanceThreadStartingWorker = new Thread(new ThreadStart(StartMaintenanceThreadWorking));
                        _maintenanceThreadStartingWorker.IsBackground = true;
                        _maintenanceThreadStartingWorker.Start();
                        _lastMaintenanceRequestTime = DateTime.Now;
                    }
                }
                else
                {
                    string debugStr = "*debugStr";
                }
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", ex, "EX01", classNMethodName: "pgIntro.MaintenanceScheduler_OnRequestSettlement");
            }
            finally
            {
                if (_manLock.CurrentCount == 0)
                    _manLock.Release();
            }
        }

        public void MaintenanceScheduler_OnSettlementDone(object sender, SettlementDoneEventArgs e)
        {

            try
            {
                _manLock.WaitAsync().Wait();
                _maintenanceRequest = false;
            }
            catch
            { }
            finally
            {
                if (_manLock.CurrentCount == 0)
                    _manLock.Release();
            }
        }

        private void StartMaintenanceThreadWorking()
        {
            try
            {
                _manLock.WaitAsync().Wait();

                if ((_pageLoaded == false) || (_isSaleHasBegun))
                {
                    App.ShowDebugMsg("Request Maintenance - Intro Page not loaded OR Sale has begun.");
                    return;
                }

                _introAniHelp.SetStartButtonEnabled(false, null);
                System.Windows.Forms.Application.DoEvents();

                bool healthChkResult = true;
                bool proceed = true;
                try
                {
                    healthChkResult = App.AppHelp.SystemHealthCheck();
                }
                catch (Exception ex)
                {
                    proceed = false;
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000050)");
                }

                if (proceed)
                {
                    if (healthChkResult == false)
                    {
                        App.MainScreenControl.Alert(detailMsg: $@"Sales Server has unknown error; (EXIT10000042)");
                    }
                    else
                    {
                        App.ShowDebugMsg("Start Request Maintenance");
                        App.TowerLight.ShowBusyState();

                        App.ShowDebugMsg("Running .. TowerLight.ShowBusyState; DEBUG01; pgIntro.StartMaintenanceThreadWorking");
                        App.Log.LogText(LogChannel, "*", "Running .. TowerLight.ShowBusyState", "DEBUG01", "pgIntro.StartMaintenanceThreadWorking", AppDecorator.Log.MessageType.Debug);

                        App.NetClientSvc.SalesService.RequestMaintenance(out bool isServerResponded);
                        App.ShowDebugMsg("End Request Maintenance");

                        if (isServerResponded == false)
                        {
                            App.ShowDebugMsg("Request Maintenance found no server response.");

                            App.Log.LogError(LogChannel, "-", new Exception("Local Server no response when Request Maintenance"), "EX01",
                                classNMethodName: "pgIntro.MaintenanceScheduler_OnRequestSettlement");

                            App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000047); Error on Maintenance Event;");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", ex, "EX05", classNMethodName: "pgIntro.StartMaintenanceThreadWorking");
            }
            finally
            {
                if (_manLock.CurrentCount == 0)
                    _manLock.Release();
            }
        }

        private bool _isSaleHasBegun = false;
        private void _introAniHelp_OnIntroBegin(object sender, StartRunningEventArgs e)
        {
            try
            {
                _manLock.WaitAsync().Wait();

                if (_pageLoaded == false)
                    return;

                if (_isSaleHasBegun)
                    return;

                if (_maintenanceRequest)
                {
                    _introAniHelp.SetStartButtonEnabled(false, null);
                    System.Windows.Forms.Application.DoEvents();
                    return;
                }

                _introAniHelp.SetStartButtonEnabled(false, e.CallerButton);
                System.Windows.Forms.Application.DoEvents();

                //App.IsLocalServerReady = false;
                App.ResetMaxTicketAdvanceDate();
                App.TowerLight.ShowBusyState();

                App.ShowDebugMsg("Running .. TowerLight.ShowBusyState; DEBUG01; pgIntro._introAniHelp_OnIntroBegin");
                App.Log.LogText(LogChannel, "*", "Running .. TowerLight.ShowBusyState", "DEBUG01", "pgIntro._introAniHelp_OnIntroBegin", AppDecorator.Log.MessageType.Debug);

                Submit(e.VehicleGroup);

                _isSaleHasBegun = true;
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", ex, classNMethodName: "pgIntro._introAniHelp_OnIntroBegin");
                App.MainScreenControl.Alert(detailMsg: ex.Message);
            }
            finally
            {
                if (_manLock.CurrentCount == 0)
                    _manLock.Release();
            }
        }

        public void InitPage()
        {
            try
            {
                _manLock.WaitAsync().Wait();

                _maintenanceRequest = false;
                _isSystemHealthy = null;
            }
            catch
            { }
            finally
            {
                if (_manLock.CurrentCount == 0)
                    _manLock.Release();
            }
        }

        private void Submit(TransportGroup vehicleGroup)
        {
            System.Windows.Forms.Application.DoEvents();

            Thread submitWorker = new Thread(new ThreadStart(new Action(() =>
            {
                try
                {
                    App.IsLocalServerReady = false;

                    if (RDLCLibraryStarter.LibraryIsReady == false)
                    {
                        string pMsg = "RDLC report engine initiation in progress; Please wait a moment";

                        App.Log.LogText(LogChannel, "-", $@"{pMsg}; (EXIT10000041B)", "X002", "pgIntro.Submit", AppDecorator.Log.MessageType.Error);
                        throw new Exception($@"{pMsg}; (EXIT10000041B)");
                    }

                    if (App.AppHelp.SystemHealthCheck())
                    {
                        App.IsLocalServerReady = true;

                        _introAniHelp.UpdateServerVersionTag(App.AppHelp.ServerApplicationVersion);
                        _introAniHelp.ShowSalesButton(App.AppHelp.IsETSIntercitySalesValid, App.AppHelp.IsKomuterSalesValid);

                        //_isSaleHasBegun = true;
                        App.NetClientSvc.SalesService.StartNewSessionCountDown(out bool isServerResponded, vehicleCategory: vehicleGroup);

                        if (isServerResponded == false)
                            App.MainScreenControl.Alert(detailMsg: "Unable to start new transaction at the moment.");
                    }
                    else
                        throw new Exception("Sales Server has unknown error; (EXIT10000041)");
                }
                catch (Exception ex)
                {
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000044)");
                    App.Log.LogError(LogChannel, "", new Exception("(EXIT10000044)", ex), "EX01", "pgIntro.Submit");
                }
            })));
            submitWorker.IsBackground = true;
            submitWorker.Start();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _introAniHelp.InitOnLoad();
            App.TowerLight.ShowAvailableState();

            App.ShowDebugMsg("Running .. TowerLight.ShowAvailableState; DEBUG01; pgIntro.Page_Loaded");
            App.Log.LogText(LogChannel, "*", "Running .. TowerLight.ShowAvailableState", "DEBUG01", "pgIntro.Page_Loaded", AppDecorator.Log.MessageType.Debug);

            _introAniHelp.SetStartButtonEnabled(true, null);
            _introAniHelp.EnabledAnimation = true;

            if (_manLock.CurrentCount == 0)
                _manLock.Release();

            //if (App.IsLocalServerReady == false)
            //{
            _detectCustmerThreadWorker = null;
            _sysInitThreadWorker = new Thread(new ThreadStart(SystemInitThreadWorking));
            _sysInitThreadWorker.IsBackground = true;
            //}
            //else
            //{
            //	_introAniHelp.UpdateServerVersionTag(App.AppHelp.ServerApplicationVersion);
            //	//Start To Detect Customer
            //	_detectCustmerThreadWorker = new Thread(new ThreadStart(DetectCustomerThreadWorking));
            //	_detectCustmerThreadWorker.IsBackground = true;
            //	_detectCustmerThreadWorker.Start();
            //}

            System.Windows.Forms.Application.DoEvents();

            _isSaleHasBegun = false;
            _pageLoaded = true;
            _sysInitThreadWorker.Start();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _pageLoaded = false;
            _introAniHelp.OnPageUnload();
            try
            {
                App.EndCustomerSensor();

                if (_sysInitThreadWorker != null)
                {
                    if ((_sysInitThreadWorker.ThreadState & ThreadState.Aborted) != ThreadState.Aborted)
                    {
                        _sysInitThreadWorker.Abort();
                    }
                }
            }
            catch (Exception ex) { }
            finally
            {
                if (_manLock.CurrentCount == 0)
                    _manLock.Release();
            }
        }


        private void SystemInitThreadWorking()
        {
            //if (App.IsLocalServerReady == false)
            //{
            try
            {
                if (_pageLoaded == false)
                    return;

                _introAniHelp.SetStartButtonEnabled(false, null);
                System.Windows.Forms.Application.DoEvents();

                _manLock.WaitAsync().Wait();

                App.IsLocalServerReady = false;

                if (App.AppHelp.SystemHealthCheck())
                {
                    App.IsLocalServerReady = true;

                    if (_pageLoaded == false)
                        return;

                    _introAniHelp.UpdateServerVersionTag(App.AppHelp.ServerApplicationVersion);
                    _introAniHelp.ShowSalesButton(App.AppHelp.IsETSIntercitySalesValid, App.AppHelp.IsKomuterSalesValid);
                    App.TowerLight.ShowAvailableState();

                    App.ShowDebugMsg("Running .. TowerLight.ShowAvailableState; DEBUG01; pgIntro.SystemInitThreadWorking");
                    App.Log.LogText(LogChannel, "*", "Running .. TowerLight.ShowAvailableState", "DEBUG01", "pgIntro.SystemInitThreadWorking", AppDecorator.Log.MessageType.Debug);

                    _introAniHelp.SetStartButtonEnabled(true, null);
                    _isSystemHealthy = true;
                    System.Windows.Forms.Application.DoEvents();

                    /////App.ShowDebugMsg($@"Entering _detectCustmerThreadWorker init ..; pgIntro.SystemInitThreadWorking");

                    //Start To Detect Customer
                    if (_detectCustmerThreadWorker is null)
                    {
                        App.ShowDebugMsg("Starting .. DetectCustomerThreadWorking; DEBUG02; pgIntro.SystemInitThreadWorking");
                        App.Log.LogText(LogChannel, "*", "Starting .. DetectCustomerThreadWorking", "DEBUG02", "pgIntro.SystemInitThreadWorking", AppDecorator.Log.MessageType.Debug);

                        _detectCustmerThreadWorker = new Thread(new ThreadStart(DetectCustomerThreadWorking));
                        _detectCustmerThreadWorker.IsBackground = true;
                        _detectCustmerThreadWorker.Start();

                        App.ShowDebugMsg("Started .. DetectCustomerThreadWorking; DEBUG03; pgIntro.SystemInitThreadWorking");
                        App.Log.LogText(LogChannel, "*", "Started .. DetectCustomerThreadWorking", "DEBUG03", "pgIntro.SystemInitThreadWorking", AppDecorator.Log.MessageType.Debug);
                    }
                    else
                    {
                        App.ShowDebugMsg("NOT Started .. DetectCustomerThreadWorking; DEBUG04; pgIntro.SystemInitThreadWorking");
                        App.Log.LogText(LogChannel, "*", "NOT Started .. DetectCustomerThreadWorking", "DEBUG04", "pgIntro.SystemInitThreadWorking", AppDecorator.Log.MessageType.Debug);
                    }

                    StatusHub.GetStatusHub().zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Welcome Page");
                }
                else
                {
                    if (_pageLoaded == false)
                        return;

                    _isSystemHealthy = false;
                    throw new Exception("Sales Server has unknown error; (EXIT10000043)");
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                App.IsLocalServerReady = false;

                App.ShowDebugMsg($@"Error : {ex.Message} ; EX01; pgIntro.SystemInitThreadWorking");

                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgIntro.SystemInit");
                App.MainScreenControl.Alert(detailMsg: ex.Message);
            }
            finally
            {
                if (_manLock.CurrentCount == 0)
                    _manLock.Release();
            }
            //}
            //else
            //{
            //	_introAniHelp.UpdateServerVersionTag(App.AppHelp.ServerApplicationVersion);
            //}
        }

        private Thread _detectCustmerThreadWorker = null;
        private void DetectCustomerThreadWorking()
        {
            App.ShowDebugMsg("Start - pgIntro.DetectCustmerThreadWorking");

            if (App.IsCustomerSensorExist == true)
            {
                _isCustomerPresent = false;
                App.StartCustomerSensor();

                bool? lastCustomerPresentFlag = null;
                while (_pageLoaded == true)
                {
                    try
                    {
                        if ((lastCustomerPresentFlag.HasValue == false) ||
                            (lastCustomerPresentFlag.Value != _isCustomerPresent))
                        {
                            if (_isCustomerPresent == false)
                                _introAniHelp.EnabledAnimation = true;
                            else
                                _introAniHelp.EnabledAnimation = false;

                            _introAniHelp.CustomerDetected(_isCustomerPresent);

                            lastCustomerPresentFlag = _isCustomerPresent;
                            System.Windows.Forms.Application.DoEvents();
                        }
                    }
                    catch (Exception ex)
                    {
                        App.Log.LogError(LogChannel, "-", ex, "EX01", "pgIntro.DetectCustmerThreadWorking");
                        App.ShowDebugMsg($@"Error (EX01) - pgIntro.DetectCustmerThreadWorking; {ex.ToString()}");
                    }
                    finally
                    {
                        if (_pageLoaded == true)
                            Thread.Sleep(300);
                    }
                }
            }

            App.ShowDebugMsg("Quit - pgIntro.DetectCustmerThreadWorking");
        }
    }
}