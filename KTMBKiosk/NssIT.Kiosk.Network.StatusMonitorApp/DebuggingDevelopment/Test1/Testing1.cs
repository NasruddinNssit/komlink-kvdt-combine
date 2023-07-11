using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.DomainLibs.Debugging;
using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus;
using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus.UIx;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Log.DB.StatusMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.StatusMonitorApp.DebugTesting.Test1
{
    /// <summary>
    /// ClassCode:EXIT65.T01
    /// </summary>
    public class Testing1 : IDebuggingDevelopment
    {
        private const string _logChannel = "DebuggingDevelopment";

        private DbLog _log = DbLog.GetDbLog();
        private MachineStatusMonitorApp _monApp = null;
        private StatusHub _statusHub = null;

        public Testing1(MachineStatusMonitorApp app)
        {
            _monApp = app;
            _statusHub = StatusHub.GetStatusHub();
        }

        public void DevExec()
        {
            //IsUIDisplayNormalTest1();
            IsUIDisplayNormalTest2();
        }

        private void IsUIDisplayNormalTest1()
        {
            //try
            //{
            //    bool isSystemRunWithDebugDev = CommonDebugging.IsSystemRunWithDebugDev(out Type aDebugType);

            //    _log.LogText(_logChannel, "*", $@"Is System Run With Debug Dev. : {isSystemRunWithDebugDev}; aDebugType: {aDebugType.FullName}", "IsSystemRunWithDebugDev", "Testing1.IsUIDisplayNormalTest1");

            //    string processId = DateTime.Now.Ticks.ToString();


            //    KioskStatusData sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, null);
            //    UIxStatusMonitorDataSendRequest data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    UIReq<UIxStatusMonitorDataSendRequest> req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);

            //    bool ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    // Error UI Page Testing

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X01");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X02");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X03");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X04");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X05");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X06");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X07");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X08");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X09");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X10");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X11");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X12");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X13");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X14");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X15");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X16");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X17");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X18");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X19");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X20");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X21");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X22");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    Thread.Sleep(60 * 1000);
            //    processId = DateTime.Now.Ticks.ToString();

            //    sttData = _statusHub.wNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X23");
            //    data1 = new UIxStatusMonitorDataSendRequest(processId, sttData);
            //    req1 = new UIReq<UIxStatusMonitorDataSendRequest>(DateTime.Now.Ticks.ToString(), AppModule.UIKioskStatusMonitor, DateTime.Now, data1);
            //    ret1 = _monApp.SendInternalCommand("*", Guid.NewGuid(), req1).GetAwaiter().GetResult();

            //    //

            //    string tt1000 = "End";
            //}
            //catch (Exception ex)
            //{
            //    _log.LogError(_logChannel, "*", ex, "Quit Debug Dev. with Error - EX01", "Testing1.DevExec");
            //}
        }

        private void IsUIDisplayNormalTest2()
        {
            try
            {
                bool isSystemRunWithDebugDev = CommonDebugging.IsSystemRunWithDebugDev(out Type aDebugType);

                _log.LogText(_logChannel, "*", $@"Is System Run With Debug Dev. : {isSystemRunWithDebugDev}; aDebugType: {aDebugType.FullName}", "IsSystemRunWithDebugDev", "Testing1.IsUIDisplayNormalTest2");


                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, null);
                
                // Error UI Page Testing
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X01");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X02");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X03");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X04");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X05");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X06");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X07");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X08");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X09");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X10");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Test Fail X11");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X12");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X13");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X14");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X15");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X16");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X17");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X18");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X19");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X20");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X21");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X22");
                //
                Thread.Sleep(60 * 1000);
                _statusHub.zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Test X23");
                //

                string tt1000 = "End";
            }
            catch (Exception ex)
            {
                _log.LogError(_logChannel, "*", ex, "Quit Debug Dev. with Error - EX01", "Testing1.DevExec");
            }
        }
    }
}
