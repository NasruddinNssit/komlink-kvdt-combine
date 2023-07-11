using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Network.StatusMonitorApp.JobApp;
using NssIT.Kiosk.Network.StatusMonitorApp.JobApp.IsUIDisplayNormalCheckTask;
using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NssIT.Kiosk.Network.StatusMonitorApp.JobApp.IsCreditCardSettlementDoneCheckTask;
using NssIT.Kiosk.Network.StatusMonitorApp.JobApp.IsPrinterStandByCheckTask;
using NssIT.Kiosk.Network.StatusMonitorApp.JobApp.IsCardMachineDataCommNormalCheckTask;

namespace NssIT.Kiosk.Network.StatusMonitorApp.CustomApp.KTMBApp
{
    /// <summary>
    /// ClassCode:EXIT65.02
    /// </summary>
    public class KTMBAppPlan : IServerAppPlan
    {
        private const string LogChannel = "StatusMonitor_App";

        private DbLog _log = null;
        private DbLog Log => (_log ?? (_log = DbLog.GetDbLog()));

        private StatusMonitorJob _statusMonitorJob = null;

        public KTMBAppPlan(StatusMonitorJob statusMonitorJob)
        {
            _statusMonitorJob = statusMonitorJob;
        }

        public void InitPlan()
        {
            _statusMonitorJob?.SetupCheckingTask(KioskCheckingCode.IsUIDisplayNormal, IsUIDisplayNormalChecking.GetStatusChecker());
            _statusMonitorJob?.SetupCheckingTask(KioskCheckingCode.IsCreditCardSettlementDone, IsCreditCardSettlementDoneChecking.GetStatusChecker());
            _statusMonitorJob?.SetupCheckingTask(KioskCheckingCode.IsPrinterStandBy, IsPrinterStandByChecking.GetStatusChecker());
            _statusMonitorJob?.SetupCheckingTask(KioskCheckingCode.IsCardMachineDataCommNormal, IsCardMachineDataCommNormalChecking.GetStatusChecker());
        }

        public void PreProcess(IKioskMsg refSvcMsg)
        {
            
        }
    }
}
