using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus;
using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus.UIx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.NetClient
{
    /// <summary>
    /// ClassCode:EXIT80.09
    /// </summary>
    public class NetClientStatusMonitorService
    {
        public const int WebServerTimeout = 9999999;
        public const int InvalidAuthentication = 9999998;

        private AppModule _appModule = AppModule.UIKioskStatusMonitor;
        private string _logChannel = "NetClientService";

        private NssIT.Kiosk.Log.DB.DbLog _log = null;
        private INetMediaInterface _netInterface;

        private ReceivedNetProcessIdTracker _recvedNetProcIdTracker = new ReceivedNetProcessIdTracker();

        // OnDataReceived : New Implemention for UI Page change only and special event like Ongoing Payment process.
        public event EventHandler<DataReceivedEventArgs> OnDataReceived;

        public NetClientStatusMonitorService(INetMediaInterface netInterface)
        {
            _netInterface = netInterface;

            _log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();

            if (_netInterface != null)
                _netInterface.OnDataReceived += _netInterface_OnDataReceived;
        }

        public void Dispose()
        {
            try
            {
                if (_netInterface != null)
                    _netInterface.OnDataReceived -= _netInterface_OnDataReceived;
            }
            catch { }

            try
            {
                if (OnDataReceived?.GetInvocationList() is Delegate[] delgList)
                {
                    foreach (EventHandler<DataReceivedEventArgs> delg in delgList)
                        OnDataReceived -= delg;
                }
            }
            catch { }

            _netInterface = null;
        }

        private int GetServerPort() => App.SysParam.PrmLocalServerPort;

        public void SendStatusRequest(KioskStatusData statusData)
        {
            string proId = DateTime.Now.Ticks.ToString();

            UIxStatusMonitorDataSendRequest data1 = new UIxStatusMonitorDataSendRequest(proId, statusData.Duplicate());
            UIReq<UIxStatusMonitorDataSendRequest> req1 = new UIReq<UIxStatusMonitorDataSendRequest>(proId, _appModule, DateTime.Now, data1);

            SendToServerOnly(req1, $@"SendStatusRequest({proId})");
        }

        /// <summary>
        /// Send to local server then expect at least one response.
        /// </summary>
        /// <param name="sendKioskMsg"></param>
        /// <param name="processTag"></param>
        /// <param name="waitDelaySec"></param>
        /// <returns></returns>
        private UIxTResult Request<UIxTResult>(IKioskMsg sendKioskMsg, string processTag, out bool isServerResponded, int waitDelaySec = 60)
        {
            string pId = Guid.NewGuid().ToString();

            isServerResponded = false;
            IKioskMsg kioskMsg = null;

            Guid lastNetProcessId;

            waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

            _log?.LogText(_logChannel, pId, $@"Start - {processTag}", "A01", "NetClientStatusMonitorService.Request");

            NetMessagePack msgPack = new NetMessagePack(sendKioskMsg) { DestinationPort = GetServerPort() };
            lastNetProcessId = msgPack.NetProcessId;

            _log?.LogText(_logChannel, pId,
                msgPack, "A05", $@"{processTag} => NetClientStatusMonitorService.Request", extraMsg: "MsgObj: NetMessagePack");

            _netInterface.SendMsgPack(msgPack);

            UIxKioskDataRequestBase reqBase = sendKioskMsg?.GetMsgData();
            DateTime endTime = DateTime.Now.AddSeconds(waitDelaySec);

            while (endTime.Ticks >= DateTime.Now.Ticks)
            {
                if (Thread.CurrentThread.ThreadState.IsStateInList(
                    ThreadState.AbortRequested, ThreadState.StopRequested,
                    ThreadState.Aborted, ThreadState.Stopped))
                {
                    break;
                }

                else if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId, out IKioskMsg data) == false)
                    Task.Delay(100).Wait();

                else
                {
                    kioskMsg = data;
                    isServerResponded = true;
                    break;
                }
            }

            _log?.LogText(_logChannel, pId, kioskMsg, "A10", "NetClientStatusMonitorService.Request", extraMsg: $@"MsgObj: {kioskMsg?.GetType().Name}");

            if ((isServerResponded) && (kioskMsg?.GetMsgData() is UIxTResult result))
            {
                return result;
            }
            else
            {
                _log?.LogText(_logChannel, pId, $@"Problem; isServerResponded : {isServerResponded}", "B01", "NetClientStatusMonitorService.Request");
                return default;
            }
        }


        /// <summary>
        /// Send to local server without expecting response 
        /// </summary>
        /// <param name="sendKioskMsg"></param>
        /// <param name="processTag"></param>
        private void SendToServerOnly(IKioskMsg sendKioskMsg, string processTag)
        {
            Guid lastNetProcessId;

            _log.LogText(_logChannel, "-", $@"Start - {processTag}", "A01", "NetClientStatusMonitorService.SendToServerOnly");

            NetMessagePack msgPack = new NetMessagePack(sendKioskMsg) { DestinationPort = GetServerPort() };
            lastNetProcessId = msgPack.NetProcessId;

            _log.LogText(_logChannel, "-",
                msgPack, "A05", $@"{processTag} => NetClientStatusMonitorService.SendToServerOnly", extraMsg: "MsgObject: NetMessagePack");

            _netInterface.SendMsgPack(msgPack);
        }

        private void _netInterface_OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.ReceivedData?.Module == AppModule.UIKioskStatusMonitor)
            {
                _recvedNetProcIdTracker.AddNetProcessId(e.ReceivedData.NetProcessId, e.ReceivedData.MsgObject);

                RaiseOnDataReceived(sender, e);
            }
        }

        /// <summary>
        /// FuncCode:EXIT80.0901
        /// </summary>
        private void RaiseOnDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                if (OnDataReceived != null)
                {
                    OnDataReceived.Invoke(sender, e);
                }
            }
            catch (Exception ex)
            {
                _log.LogError(_logChannel, "-", new Exception("Unhandled event exception; (EXIT80.0901.EX01)", ex), "EX01", "NetClientStatusMonitorService.RaiseOnDataReceived", netProcessId: e?.ReceivedData?.NetProcessId);
            }
        }
    }
}
