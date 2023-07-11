using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus;
using NssIT.Kiosk.Network.StatusMonitorApp.JobApp.IsCardMachineDataCommNormalCheckTask;
using NssIT.Kiosk.Network.StatusMonitorApp.JobApp.IsCreditCardSettlementDoneCheckTask;
using NssIT.Kiosk.Network.StatusMonitorApp.JobApp.IsPrinterStandByCheckTask;
using NssIT.Kiosk.Network.StatusMonitorApp.JobApp.IsUIDisplayNormalCheckTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.StatusMonitorApp.JobApp
{
    /// <summary>
    /// ClassCode:EXIT65.04
    /// </summary>
    public class StatusMonitorCheckerCollection : IDisposable 
    {
        private const string LogChannel = "StatusMonitor_App";

        private static SemaphoreSlim _manLock = new SemaphoreSlim(1);
        private static StatusMonitorCheckerCollection _statusMonitorTaskLIst = null;

        private object _lock = new object();

        ///// Task Execution List
        private IsUIDisplayNormalChecking _isUIDisplayNormalChecking = null;
        private IsCreditCardSettlementDoneChecking _isCreditCardSettlementDoneChecking = null;
        private IsPrinterStandByChecking _isPrinterStandByChecking = null;
        private IsCardMachineDataCommNormalChecking _isCardMachineDataCommNormalChecking = null;
        ///// ===================================================================================

        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _isUIDisplayNormalChecking = null;
            _isCreditCardSettlementDoneChecking = null;
            _isPrinterStandByChecking = null;
            _isCardMachineDataCommNormalChecking = null;
        }

        private StatusMonitorCheckerCollection()
        { }

        public static StatusMonitorCheckerCollection GetStatusMonitorCheckerCollection()
        {
            if (_statusMonitorTaskLIst == null)
            {
                try
                {
                    _manLock.WaitAsync().Wait();
                    if (_statusMonitorTaskLIst == null)
                    {
                        _statusMonitorTaskLIst = new StatusMonitorCheckerCollection();
                    }
                    return _statusMonitorTaskLIst;
                }
                finally
                {
                    if (_manLock.CurrentCount == 0)
                        _manLock.Release();
                }
            }
            else
                return _statusMonitorTaskLIst;
        }

        public void SetupCheckingTask(KioskCheckingCode checkCode, IStatusCheckingTask checkingTask)
        {
            Thread tWorker = new Thread(new ThreadStart(new Action(() => 
            { 
                try
                {
                    lock (_lock)
                    {
                        if ((checkCode == KioskCheckingCode.IsUIDisplayNormal) && (checkingTask is IsUIDisplayNormalChecking isUIDisplayNormalTask))
                        {
                            _isUIDisplayNormalChecking = isUIDisplayNormalTask;
                        }
                        else if ((checkCode == KioskCheckingCode.IsCreditCardSettlementDone) && (checkingTask is IsCreditCardSettlementDoneChecking isCreditCardSettlementErrorTask))
                        {
                            _isCreditCardSettlementDoneChecking = isCreditCardSettlementErrorTask;
                        }
                        else if ((checkCode == KioskCheckingCode.IsPrinterStandBy) && (checkingTask is IsPrinterStandByChecking isPrinterStandByCheckingTask))
                        {
                            _isPrinterStandByChecking = isPrinterStandByCheckingTask;
                        }
                        else if ((checkCode == KioskCheckingCode.IsCardMachineDataCommNormal) && (checkingTask is IsCardMachineDataCommNormalChecking isCardMachineDataCommNormalChecking))
                        {
                            _isCardMachineDataCommNormalChecking = isCardMachineDataCommNormalChecking;
                        }

                        //_isCardMachineDataCommNormalChecking
                    }
                }
                catch (Exception ex)
                {
                    string ms = ex.Message;
                }
            })));
            tWorker.IsBackground = true;
            tWorker.Priority = ThreadPriority.AboveNormal;
            tWorker.Start();
            tWorker.Join();
        }

        public KioskStatusData[] GetAllOustandingStatus()
        {
            List<KioskStatusData> outStdList = new List<KioskStatusData>();

            if (_isUIDisplayNormalChecking != null)
            {
                try
                {
                    KioskStatusData stt = _isUIDisplayNormalChecking.CheckInNewStatus(null);
                    if (stt != null)
                    {
                        outStdList.Add(stt);
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = ex.Message;
                }
            }

            if (_isCreditCardSettlementDoneChecking != null)
            {
                try
                {
                    KioskStatusData stt = _isCreditCardSettlementDoneChecking.CheckInNewStatus(null);
                    if (stt != null)
                    {
                        outStdList.Add(stt);
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = ex.Message;
                }
            }

            if (_isPrinterStandByChecking != null)
            {
                try
                {
                    KioskStatusData stt = _isPrinterStandByChecking.CheckInNewStatus(null);
                    if (stt != null)
                    {
                        outStdList.Add(stt);
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = ex.Message;
                }
            }

            if (_isCardMachineDataCommNormalChecking != null)
            {
                try
                {
                    KioskStatusData stt = _isCardMachineDataCommNormalChecking.CheckInNewStatus(null);
                    if (stt != null)
                    {
                        outStdList.Add(stt);
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = ex.Message;
                }
            }

            return outStdList.ToArray();
        }

        public void ReportSentStatusList(KioskStatusData[] sentStatusList)
        {
            _isUIDisplayNormalChecking?.ReportSentStatus(sentStatusList);
            _isCreditCardSettlementDoneChecking?.ReportSentStatus(sentStatusList);
            _isPrinterStandByChecking?.ReportSentStatus(sentStatusList);
            _isCardMachineDataCommNormalChecking?.ReportSentStatus(sentStatusList);
        }

        public IStatusCheckingTask GetStatusChecker(KioskCheckingCode checkCode)
        {
            if (checkCode == KioskCheckingCode.IsUIDisplayNormal)
                return _isUIDisplayNormalChecking;
        
            else if (checkCode == KioskCheckingCode.IsCreditCardSettlementDone)
                return _isCreditCardSettlementDoneChecking;

            else if (checkCode == KioskCheckingCode.IsPrinterStandBy)
                return _isPrinterStandByChecking;

            else if (checkCode == KioskCheckingCode.IsCardMachineDataCommNormal)
                return _isCardMachineDataCommNormalChecking;

            return null;
        }

        public IStatusCheckingTask[] GetAllStatusChecker()
        {
            List<IStatusCheckingTask> retList = new List<IStatusCheckingTask>();

            if (_isUIDisplayNormalChecking != null)
                retList.Add(_isUIDisplayNormalChecking);

            if (_isCreditCardSettlementDoneChecking != null)
                retList.Add(_isCreditCardSettlementDoneChecking);

            if (_isPrinterStandByChecking != null)
                retList.Add(_isPrinterStandByChecking);

            if (_isCardMachineDataCommNormalChecking != null)
                retList.Add(_isCardMachineDataCommNormalChecking);

            return retList.ToArray();
        }
    }
}
