using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.StatusMonitorApp.JobApp.IsCreditCardSettlementDoneCheckTask
{
    /// <summary>
	/// ClassCode:EXIT65.08; Status Enum using KioskCommonStatus
	/// </summary>
    public class IsCreditCardSettlementDoneChecking : IStatusCheckingTask
    {
        private const string LogChannel = "StatusMonitor_App";

        //CYA-TEST ---------------------------------------------------------------
        //private const int NormalReportIntervalMinutes = 15;
        //-------------------------------------------------------------------------
        private const int NormalReportIntervalMinutes = ((24 + 3) * 60);

        private static SemaphoreSlim _manLock = new SemaphoreSlim(1);
        private static IsCreditCardSettlementDoneChecking _auditor = null;

        private DbLog _log = null;
        private DbLog Log => (_log ?? (_log = DbLog.GetDbLog()));
        public KioskCheckingCode ThisCheckingCode => KioskCheckingCode.IsCreditCardSettlementDone;

        /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        private KioskStatusData _lastReportedStatus = null;

        //CYA-TEST -----private DateTime _nextReportTime = DateTime.Now.AddMinutes(15);
        //-------------------------------------------------------------------------
        private DateTime _nextReportTime = DateTime.Now.AddMinutes(3 * 60);

        private IsCreditCardSettlementDoneChecking() { }

        public static IsCreditCardSettlementDoneChecking GetStatusChecker()
        {
            if (_auditor == null)
            {
                try
                {
                    _manLock.WaitAsync().Wait();
                    if (_auditor == null)
                    {
                        _auditor = new IsCreditCardSettlementDoneChecking();
                    }
                    return _auditor;
                }
                finally
                {
                    if (_manLock.CurrentCount == 0)
                        _manLock.Release();
                }
            }
            else
                return _auditor;
        }

        public KioskStatusData CheckInNewStatus(KioskStatusData newStatusData)
        {
            if (newStatusData is null)
            {
                // By Pass
            }
            else if (newStatusData.CheckingCode != ThisCheckingCode)
            {
                return null;
            }

            KioskStatusData inComingNewStatusData = newStatusData?.Duplicate();
            KioskStatusData returnStatusData = null;
            DateTime currentTime = DateTime.Now;

            bool isReportStatusTimeout = _nextReportTime.Ticks < currentTime.Ticks;

            /////-----------------------------------------------------------------------------------------------------------
            ///// Timeout !!
            if (isReportStatusTimeout)
            {
                if (inComingNewStatusData is null)
                {
                        returnStatusData = new KioskStatusData()
                        {
                            CheckingGroup = StatusCheckingGroup.CreditCard,
                            CheckingCode = ThisCheckingCode,
                            Remark = "Caution ! Credit Card Settlement not done",
                            Status = (int)KioskCommonStatus.No
                        };
                        returnStatusData.MachineLocalTime = KioskStatusDataUniqueTime.GetNewMachineLocalTime();
                }
                else
                {
                    returnStatusData = inComingNewStatusData.Duplicate();
                }
            }
            /////-----------------------------------------------------------------------------------------------------------
            ///// Not Yet Timeout
            else if (inComingNewStatusData != null)
            {
                returnStatusData = inComingNewStatusData.Duplicate();
            }

            return returnStatusData;
        }

        public KioskStatusData GetDefaultStatus(string remark, IStatusRemark RemarkObj)
        {
            string rem = string.IsNullOrWhiteSpace(remark) ? "-Default (DF)-" : remark.Trim();
            KioskCommonStatus aStatus = KioskCommonStatus.Yes;

            if (rem.Equals(StatusMonitorJob.SystemStartedTag, StringComparison.InvariantCultureIgnoreCase))
            {
                rem = $@"{rem} - Settlement in progress ..";
                aStatus = KioskCommonStatus.No;
            }

            KioskStatusData stt = new KioskStatusData()
            {
                CheckingGroup = StatusCheckingGroup.CreditCard,
                CheckingCode = ThisCheckingCode,
                Remark = rem,
                Status = (int)aStatus
            };
            stt.MachineLocalTime = KioskStatusDataUniqueTime.GetNewMachineLocalTime();
            return null;
        }

        public void ReportSentStatus(KioskStatusData[] sentStatusList)
        {
            if ((sentStatusList is null) || (sentStatusList.Length == 0))
                return;

            KioskStatusData sentStatus = (from stt in sentStatusList
                                          where stt.CheckingCode == ThisCheckingCode
                                          select stt).FirstOrDefault();

            if (sentStatus is null)
                return;
            /////-------------------------------------------------------------------------------------
            _nextReportTime = DateTime.Now.AddMinutes(NormalReportIntervalMinutes);
            _lastReportedStatus = sentStatus.Duplicate();
        }
    }
}
