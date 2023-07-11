using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;
using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Access.Echo;
using NssIT.Kiosk.Device.PAX.IM30.IM30PayApp;
using NssIT.Train.Kiosk.Common.Data.Response;
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

namespace NssIT.Kiosk.Client.ViewPage.Alert
{
    /// <summary>
    /// Interaction logic for pgUnderMaintenance.xaml
    /// </summary>
    public partial class pgUnderMaintenance : Page, IMaintenance
    {
        private const string LogChannel = "IM30_APP";

        private PayWaveSettlementScheduler _scheduler = null;
        private Thread _timerThreadWorker = null;

        private bool _pageLoaded = false;
        public pgUnderMaintenance()
        {
            InitializeComponent();

            RequestOutstandingSettlementInfoHandle = new PayWaveSettlement.RequestOutstandingSettlementInfo(GetOutstandingSettlementHost);
            UpdateSettlementInfoHandle = new PayWaveSettlement.UpdateSettlementInfo(UpdateSettlementInfo);

            _timerThreadWorker = new Thread(new ThreadStart(TimerThreadWorking));
            _timerThreadWorker.IsBackground = true;
            _timerThreadWorker.Priority = ThreadPriority.BelowNormal;
            _timerThreadWorker.Start();
        }

        public PayWaveSettlement.RequestOutstandingSettlementInfo RequestOutstandingSettlementInfoHandle { get; private set; }
        public PayWaveSettlement.UpdateSettlementInfo UpdateSettlementInfoHandle { get; private set; }

        private void TimerThreadWorking()
        {
            while(true)
            {
                if (_pageLoaded)
                {
                    this.Dispatcher.Invoke(new Action(() => { 
                        TxtTimeStr.Text = DateTime.Now.ToString("yyyyMMdd-HHmmss-fffff");
                    }));

                    Thread.Sleep(1000);
                }
                else
                    Thread.Sleep(5000);
            }
        }

        public void InitMaintenance(PayWaveSettlementScheduler scheduler)
        {
            if (_scheduler == null)
            {
                _scheduler = scheduler;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _scheduler.AgreeForSettlement();

            string svrVer = App.AppHelp.ServerApplicationVersion;
            string cliVer = App.SystemVersion;

            TxtSysVer.Text = $@"{cliVer} / {svrVer}";
            TxtStartTimeStr.Text = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            TxtTimeStr.Text = DateTime.Now.ToString("yyyyMMdd-HHmmss-fffff");

            _pageLoaded = true;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _pageLoaded = false;
        }

        public void MaintenanceScheduler_OnSettlementDone(object sender, SettlementDoneEventArgs e)
        {
            Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
                try
                {
                    Thread.Sleep(3000); /* Sleep to allow Intro Page Get Ready */

                    if (e.RebootMachineRequest)
                    {
                        if (App.NetClientSvc.SalesServiceV2.QueryKioskLastRebootTime(out KioskLastRebootTimeEcho kioskLastRebootTime, out _) == true)
                        {
                            bool proceedtoShutdown = false;
                            
                            if (kioskLastRebootTime.LastRebootTime.HasValue == false)
                                proceedtoShutdown = true;

                            else
                            {
                                int rebootIntervalHours = 12;
                                DateTime rebootTime = kioskLastRebootTime.LastRebootTime.Value.AddHours(rebootIntervalHours);

                                if (DateTime.Now.Ticks > rebootTime.Ticks)
                                    proceedtoShutdown = true;
                            }

                            if (proceedtoShutdown)
                            {
                                App.ShutdownX();
                                App.NetClientSvc.SalesService.RestartMachineRequest();
                                Thread.Sleep(5000); /* Sleep to allow Restart Machine procedure */
                            }
                        }
                        else
                        {
                            App.Log.LogError(LogChannel, "", new Exception("Fail to read Last Kiosk Reboot Time ; (EXIT10000051)"), "EX05", "pgUnderMaintenance.MaintenanceScheduler_OnSettlementDone");
                        }
                    }

                    App.NetClientSvc.SalesService.SubmitFinishedMaintenance();
                }
                catch (Exception ex)
                {
                    App.Log.LogError(LogChannel, "", new Exception("(EXIT10000049)", ex), "EX01", "pgUnderMaintenance.MaintenanceScheduler_OnSettlementDone");
                }
                finally
                {
                    App.MainScreenControl.ShowWelcome();
                }
            })));
            submitWorker.IsBackground = true;
            submitWorker.Start();
        }

        private void BtnFinishMaintenance_Click(object sender, RoutedEventArgs e)
        {
            Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
                try
                {
                    App.NetClientSvc.SalesService.SubmitFinishedMaintenance();
                }
                catch (Exception ex)
                {
                    App.Log.LogError(LogChannel, "", new Exception("(EXIT10000049)", ex), "EX01", "pgUnderMaintenance.BtnFinishMaintenance_Click");
                }
            })));
            submitWorker.IsBackground = true;
            submitWorker.Start();

            App.MainScreenControl.ShowWelcome();
        }

        //private string[] _latestOutstandingSettlementHostList;
        private string[] GetOutstandingSettlementHost(out bool isRequestSuccessful)
        {
            isRequestSuccessful = false;
            _latestOutstandingSettlement.IsQueryResponsed = false;
            _latestOutstandingSettlement.IsQuerySuccess = false;
            _latestOutstandingSettlement.HostArr = null;

            Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
                try
                {
                    App.NetClientSvc.SalesService.RequestOutstandingCardSettlementStatus(out bool isServerResponded);

                    if (isServerResponded == false)
                    {
                        // App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT100005xx)");
                        throw new Exception("No response when Get Outstanding Card Settlement Host; (EXIT10000920)");
                    }
                }
                catch (Exception ex)
                {
                    //App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT100005xx)");
                    App.Log.LogError(LogChannel, "*", new Exception("(EXIT10000921)", ex), "EX01", "pgUnderMaintenance.GetOutstandingSettlementInfo");
                }
            })));
            submitWorker.IsBackground = true;
            submitWorker.Start();

            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(80);

            while ((endTime.Subtract(DateTime.Now).TotalSeconds > 0) && (_latestOutstandingSettlement.IsQueryResponsed == false))
            {
                Thread.Sleep(300);
            }

            isRequestSuccessful = _latestOutstandingSettlement.IsQuerySuccess;

            return _latestOutstandingSettlement.HostArr;
        }

        private (bool IsQueryResponsed, bool IsQuerySuccess, string[] HostArr) _latestOutstandingSettlement =
            (IsQueryResponsed: false, IsQuerySuccess: false, HostArr: null);

        public void ProceedOutstandingMaintenance(UISalesCheckOutstandingCardSettlementAck outstandingCardSettlement)
        {
            OutstandingCardSettlementResult res = (OutstandingCardSettlementResult)outstandingCardSettlement.MessageData;

            _scheduler.AgreeForSettlement();

            if (res.Status == true)
            {
                List<string> hostList = new List<string>();

                if (res.Data?.Host?.Length > 0)
                    _latestOutstandingSettlement.HostArr = res.Data.Host;
                else
                    _latestOutstandingSettlement.HostArr = new string[0];

                _latestOutstandingSettlement.IsQuerySuccess = true;
            }
            else
            {
                _latestOutstandingSettlement.HostArr = new string[0];
                _latestOutstandingSettlement.IsQuerySuccess = false;
            }
            _latestOutstandingSettlement.IsQueryResponsed = true;
        }

        private bool? _isCardSettlementSaveSuccess;
        /// <summary>
        /// Return true when update DB successful.
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="responseInfo"></param>
        /// <returns></returns>
        private bool UpdateSettlementInfo(string processId, ResponseInfo responseInfo)
        {
            _isCardSettlementSaveSuccess = null;

            Thread submitWorker = new Thread(new ThreadStart(new Action(() =>
            {
                try
                {
                    App.NetClientSvc.SalesService.SubmitCardSettlement(processId, 
                        responseInfo.HostNo, 
                        responseInfo.BatchNumber, 
                        responseInfo.BatchCount, 
                        responseInfo.BatchCurrencyAmount, 
                        responseInfo.StatusCode, 
                        responseInfo.MachineId, 
                        responseInfo.ErrorMsg,
                        out bool isServerResponded);

                    if (isServerResponded == false)
                    {
                        // App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT100005xx)");
                        App.Log.LogText(LogChannel, "*", responseInfo, "A01", "pgUnderMaintenance.UpdateSettlementInfo", AppDecorator.Log.MessageType.Error,
                            extraMsg: "No response when update card settlement to Database; (EXIT10000922); MsgObj: ResponseInfo");
                    }
                }
                catch (Exception ex)
                {
                    //App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT100005xx)");
                    App.Log.LogError(LogChannel, "*", new Exception("(EXIT10000923)", ex), "EX01", "pgUnderMaintenance.GetOutstandingSettlementInfo");
                }
            })));
            submitWorker.IsBackground = true;
            submitWorker.Start();

            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(80);

            while ((endTime.Subtract(DateTime.Now).TotalSeconds > 0) && (_isCardSettlementSaveSuccess.HasValue == false))
            {
                Thread.Sleep(300);
            }

            return (_isCardSettlementSaveSuccess.HasValue) ? _isCardSettlementSaveSuccess.Value : false;
        }

        public void CardSettlementStatusAcknowledge(UISalesCardSettlementStatusAck cardSettlementStatusAck)
        {
            CardSettlementResult cardSettRes = (CardSettlementResult)cardSettlementStatusAck.MessageData;
            _isCardSettlementSaveSuccess = cardSettRes.Status;
        }

    }
}
