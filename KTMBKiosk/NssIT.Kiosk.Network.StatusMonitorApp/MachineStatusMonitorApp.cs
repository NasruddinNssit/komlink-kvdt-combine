using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus;
using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus.UIx;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Log.DB.StatusMonitor;
using NssIT.Kiosk.Network.StatusMonitorApp.CustomApp;
using NssIT.Kiosk.Network.StatusMonitorApp.CustomApp.KTMBApp;
//using NssIT.Kiosk.Network.StatusMonitorApp.DebugTesting.Test1;
using NssIT.Kiosk.Network.StatusMonitorApp.JobApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.StatusMonitorApp
{
    /// <summary>
    /// ClassCode:EXIT65.01
    /// </summary>
    public class MachineStatusMonitorApp : IUIApplicationJob, IDisposable
    {
        private const string LogChannel = "StatusMonitor_App";

        public event EventHandler<UIMessageEventArgs> OnShowResultMessage;

        private SemaphoreSlim _asyncSendLock = new SemaphoreSlim(1);

        private DbLog _log = null;
        private DbLog Log => (_log ?? (_log = DbLog.GetDbLog()));

        private KTMBAppPlan _appPlan = null;
        private IAppAccessCallBackPlan _accCallBackPlan = null;
        //private AppCallBackEvent _appCallBackEvent = null;
        private StatusHub _statusHub = null;
        private StatusMonitorJob _statusMonitorJob = null;

        public MachineStatusMonitorApp()
        {
            _statusHub = StatusHub.GetStatusHub();
            //_appCallBackEvent = new AppCallBackEvent(AccessCallBackEvent);
            _statusMonitorJob = StatusMonitorJob.GetStatusMonitorJob();
            
            _appPlan = new KTMBAppPlan(_statusMonitorJob);
            _appPlan.InitPlan();

            _accCallBackPlan = new KTMBApp_AccessCallBackPlan(this);

            //Testing();
        }

        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed == false)
            {
                ShutDown();
                _disposed = true;
            }
        }

        ///// <summary>
        ///// FuncCode:EXIT65.0105
        ///// </summary>
        //public async void AccessCallBackEvent(UIxKioskDataAckBase accessResult)
        //{
        //    string processId = string.IsNullOrWhiteSpace(accessResult?.BaseProcessId) ? "*" : accessResult?.BaseProcessId;

        //    try
        //    {
        //        if (accessResult.BaseRefNetProcessId.HasValue == false)
        //            throw new Exception($@"Unable to read NetProcessId from '{accessResult.GetType().Name}'");

        //        await _accCallBackPlan.DeliverAccessResult(accessResult);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log?.LogError(LogChannel, processId, new WithDataException(ex.Message, ex, accessResult), "EX01", "MachineStatusMonitorApp.AccessCallBackEvent");
        //    }
        //}

        public async Task<bool> SendInternalCommand(string processId, Guid? netProcessId, IKioskMsg svcMsg)
        {
            bool lockSuccess = false;

            try
            {
                lockSuccess = await _asyncSendLock.WaitAsync(1 * 60 * 1000);

                if (lockSuccess == false)
                    return false;

                
                if ((svcMsg is UIReq<UIxStatusMonitorDataSendRequest> uiReq1) && (uiReq1.MsgData is UIxStatusMonitorDataSendRequest uiReq1Data))
                {
                    _statusHub?.LogStatus(uiReq1Data.StatusData.Duplicate());
                }
                //if ((svcMsg is UIReq<UIxMonSttIsUIDisplayNormalSendRequest> uiReq2) && (uiReq2.MsgData is UIxMonSttIsUIDisplayNormalSendRequest uiReq2Data))
                //{
                //    _statusHub.LogStatus(uiReq2Data.StatusData.Duplicate());
                //}
                //UIxStatusMonitorDataSendRequest

                return true;
            }
            catch (Exception ex)
            {
                Log.LogError(LogChannel, processId, new WithDataException("MsgObj : IKioskMsg", ex, svcMsg), "EX02", "MachineStatusMonitorApp.SendInternalCommand", netProcessId: netProcessId);
            }
            finally
            {
                if ((lockSuccess == true) && (_asyncSendLock.CurrentCount == 0))
                    _asyncSendLock.Release();
            }
            return false;
        }

        public bool ShutDown()
        {
            _statusMonitorJob.Dispose();

            _appPlan = null;
            _statusHub = null;
            _statusMonitorJob = null;
            return false;
        }

        //private void Testing()
        //{
        //    Testing1 t1 = new Testing1(this);

        //    Thread tWork = new Thread(new ThreadStart(new Action(() => 
        //    {
        //        Thread.Sleep(5000);

        //        t1.DevExec();
        //    })));
        //    tWork.IsBackground = true;
        //    tWork.Start();
        //}
    }
}
