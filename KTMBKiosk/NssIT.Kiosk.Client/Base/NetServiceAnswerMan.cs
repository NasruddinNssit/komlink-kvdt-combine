using NssIT.Kiosk.AppDecorator;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.Client.NetClient;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Base
{
    // Note : Refer to pgKomuter.QueryTicketTypePackage
    /// <summary>
    /// ClassCode:EXIT80.12; Used to send Data Message Pack to Local Server and wait for response. If fail to get response, run an expected delegated method (FailLocalServerResponseCallBackDelg) to call back
    /// </summary>
    public class NetServiceAnswerMan : IDisposable
    {
        public delegate void FailLocalServerResponseCallBackDelg(string noResponseErrorMessage);

        private bool _isThreadWorkingEnd = false;
        private bool _startCallBack = false;
        private Guid _netProcessId = Guid.Empty;
        private string _processId = "*";
        private string _logChannel = "#NetServiceAnswerMan_Channel#";

        private Thread _threadWorker = null;
        private DbLog _log = DbLog.GetDbLog();
        private FailLocalServerResponseCallBackDelg _failLocalServerResponseCallBackDelgHandle = null;
        private INetMediaInterface _netInterface = null;
        private object _callBackExecLock = new object();

        public string RunningTag { get; private set; } = "*NetServiceAnswerMan*";
        
        /// <summary>
        /// FuncCode:EXIT80.1201
        /// </summary>
        public NetServiceAnswerMan(NetMessagePack msgPack, string runningTag, string noResponseErrorMessage, string logChannel,
            FailLocalServerResponseCallBackDelg failLocalServerResponseCallBackDelgHandle,
            INetMediaInterface netInterface,
            NssIT.Kiosk.Client.NetClient.NetClientSalesService.ReceivedNetProcessIdTracker recvedNetProcIdTracker,
            string processId = "*",
            int waitDelaySec = 60, 
            ThreadPriority threadPriority = ThreadPriority.Normal)
        {
            RunningTag = string.IsNullOrWhiteSpace(runningTag) ? "**NetServiceAnswerMan**" : runningTag.Trim();
            _logChannel = (string.IsNullOrWhiteSpace(logChannel) ? "#NetServiceAnswerMan_Channel#" : logChannel.Trim());
            _disposed = false;
            _isThreadWorkingEnd = false;

            string clsMthTag = $@"NetServiceAnswerMan.(ctor) -> ~{RunningTag}~";

            _netInterface = netInterface;
            _netProcessId = msgPack.NetProcessId;
            _processId = (string.IsNullOrWhiteSpace(processId) || (processId.Equals("*")) ) ? _netProcessId.ToString(): processId.Trim();
            bool isServerResponded = false;
            bool isCallBack = false;
            _failLocalServerResponseCallBackDelgHandle = failLocalServerResponseCallBackDelgHandle;

            _threadWorker = new Thread(new ThreadStart(new Action(() => {
                try
                {
                    waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;
                    DateTime startTime = DateTime.Now;
                    DateTime endTime = startTime.AddSeconds(waitDelaySec);

                    _log?.LogText(_logChannel, _processId, msgPack, "A01", clsMthTag,
                        extraMsg: $@"Start new NetServiceAnswerMan; waitDelaySec: {waitDelaySec}; endTime: {endTime:yyyy-MM-dd HH:mm:ss}"
                        , netProcessId: _netProcessId);

                    // .. Sleep to allow msgPack to be saved into log
                    Thread.Sleep(1);
                    //-- -- -- -- -- 

                    netInterface.SendMsgPack(msgPack);

                    //CYA-TEST
                    ///////////ExpireNetProcessId();
                    //----------------------------------------------------------------------------------------

                    while ((endTime.Subtract(DateTime.Now).TotalSeconds > 0) && (_disposed == false))
                    {
                        if (recvedNetProcIdTracker.CheckReceivedResponded(_netProcessId) == false)
                            Thread.Sleep(100);

                        else
                        {
                            isServerResponded = true;
                            break;
                        }
                    }
                                        
                    if (_disposed == false)
                    {
                        string msgX = "";

                        if (isServerResponded == false)
                        {
                            bool alreadyExpired = false;
                            alreadyExpired = ExpireNetProcessId();
                            
                            msgX += "; Unable to read from Local Server; Adnormal result !!";

                            if (alreadyExpired)
                            {
                                msgX += "; already Expired (A)";
                            }

                            try
                            {
                                lock(_callBackExecLock)
                                {
                                    if (_disposed == false)
                                    {
                                        _startCallBack = true;
                                    }
                                }
                                if (_startCallBack == true)
                                {
                                    isCallBack = true;
                                    _failLocalServerResponseCallBackDelgHandle?.Invoke(noResponseErrorMessage);
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Error when callback noLocalServerResponseCallBackDelgHandle; (EXIT80.1201.X1)", ex);
                            }
                        }
                        else
                            msgX = "; Response found";

                        _log?.LogText(_logChannel, _processId,
                            $@"End{msgX}; IsLocalServerResponded: {isServerResponded}",
                            "A100",
                            clsMthTag, netProcessId: _netProcessId);
                    }
                    else
                    {
                        ExpireNetProcessId();

                        _log?.LogText(_logChannel, _processId,
                            $@"End - Stop Requested; Is disposing: {_disposed}",
                            "A110",
                            clsMthTag, netProcessId: _netProcessId);
                    }
                }
                catch (ThreadAbortException)
                {
                    ExpireNetProcessId();

                    _log.LogText(_logChannel, _processId,
                        $@"ThreadAbortException",
                        "A111",
                        clsMthTag, netProcessId: _netProcessId);
                }
                catch (Exception ex)
                {
                    ExpireNetProcessId();
                    string msg = $@"{ex.Message}; (EXIT80.1201.EX01); Error when {RunningTag}";

                    _log?.LogError(_logChannel, _processId, new Exception(msg, ex), "EX01", clsMthTag, netProcessId: _netProcessId);

                    if ((isCallBack == false) && (_disposed == false))
                    {
                        try
                        {
                            bool reqCallBack = false;
                            lock (_callBackExecLock)
                            {
                                if ((isCallBack == false) && (_disposed == false))
                                {
                                    _startCallBack = true;
                                    reqCallBack = true;
                                }
                            }

                            if (reqCallBack)
                            {
                                isCallBack = true;
                                _failLocalServerResponseCallBackDelgHandle?.Invoke(msg);
                            }
                        }
                        catch (Exception ex2)
                        {
                            _log?.LogError(_logChannel, _processId, new Exception($@"Error when callback noLocalServerResponseCallBackDelgHandle; (EXIT80.1201.EX02); {msg}", ex2)
                                , "EX02", clsMthTag, netProcessId: _netProcessId);
                        }
                    }
                }
                finally
                {
                    _log?.LogText(_logChannel, _processId, $@"End NetServiceAnswerMan", "A200", clsMthTag
                        , netProcessId: _netProcessId);

                    _isThreadWorkingEnd = true;
                }
            })));
            _threadWorker.IsBackground = true;
            _threadWorker.Priority = threadPriority;
            _threadWorker.Start();
        }

        private bool _isNetProcessExpired = false;
        private bool ExpireNetProcessId()
        {
            bool? ret = false;
            try
            {
                if ((_isNetProcessExpired == false) && (_netInterface != null))
                {
                    ret = _netInterface?.SetExpiredNetProcessId(_netProcessId);
                    _isNetProcessExpired = true;
                }
            }
            catch { }
            return ret ?? false;
        }

        public bool IsEnd
        {
            get => (_isThreadWorkingEnd);
        }

        public void WaitToComplete()
        {
            while (IsEnd == false)
            {
                Thread.Sleep(100);
            }
        }

        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed)
                return;

            if (IsEnd)
            {
                _disposed = true;
                _failLocalServerResponseCallBackDelgHandle = null;
                _threadWorker = null;
                _netInterface = null;
                _log = null;
                _callBackExecLock = null;
                return;
            }

            int maxWaitSec = 25;
            DateTime expiredTime = DateTime.Now.AddSeconds(maxWaitSec);

            string clsMthTag = $@"NetServiceAnswerMan.Dispose -> ~{RunningTag}~";

            // Note : Using Thread to dispose is to avoid calling _failLocalServerResponseCallBackDelgHandle and also call the Dispose(), in the same time and same thread.
            //          Without using thread, when calling StopAnswering() will cause the _threadWorker always being Aborted when _failLocalServerResponseCallBackDelgHandle is raised.
            //          This refer to pgKomuter.QueryTicketTypePackage
            Thread dThread = new Thread(new ThreadStart(new Action(() => 
            {
                bool proceedToDispose = false;
                lock (_callBackExecLock)
                {
                    if (_startCallBack == false)
                    {
                        proceedToDispose = true;
                        _disposed = true;
                    }
                }

                while ((expiredTime.Subtract(DateTime.Now).TotalMilliseconds > 0) && (_startCallBack) && (_isThreadWorkingEnd == false))
                {
                    Thread.Sleep(100);
                }

                if ((proceedToDispose) || (_disposed == false))
                {
                    _disposed = true;
                    try
                    {
                        StopAnswering();
                    }
                    catch (Exception ex)
                    {
                        _log?.LogError(_logChannel, _processId, ex, "EX01", clsMthTag, netProcessId: _netProcessId);
                    }

                    _failLocalServerResponseCallBackDelgHandle = null;
                    _threadWorker = null;
                    _netInterface = null;
                    _log = null;
                    _callBackExecLock = null;
                }
            })));
            dThread.IsBackground = true;
            dThread.Start();

            return;

            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            void StopAnswering()
            {
                string clsMthTag2 = $@"NetServiceAnswerMan.StopAnswering -> ~{RunningTag}~";

                if (_isThreadWorkingEnd == false)
                {
                    _log?.LogText(_logChannel, _processId, $@"NetServiceAnswerMan stop operation", "A01", clsMthTag2, netProcessId: _netProcessId);

                    Thread.Sleep(100);

                    if (_isThreadWorkingEnd == false)
                    {
                        try
                        {
                            int maxWaitSec2 = 3;
                            DateTime expiredTime2 = DateTime.Now.AddSeconds(maxWaitSec2);

                            while ((expiredTime2.Subtract(DateTime.Now).TotalMilliseconds > 0) && (_isThreadWorkingEnd == false))
                            {
                                Thread.Sleep(100);
                            }

                            if ((_isThreadWorkingEnd == false) 
                                && (_threadWorker?.ThreadState.IsStateInList(ThreadState.Aborted, ThreadState.AbortRequested, ThreadState.Stopped, ThreadState.StopRequested) == false))
                            {
                                _log?.LogText(_logChannel, _processId, $@"NetServiceAnswerMan abort operation", "A05", clsMthTag2, netProcessId: _netProcessId);

                                _threadWorker?.Abort();

                                DateTime expTime = DateTime.Now.AddMilliseconds(1500);
                                while 
                                    (
                                        (_threadWorker != null) && 
                                        (_threadWorker?.ThreadState.IsStateInList(ThreadState.Aborted, ThreadState.AbortRequested, ThreadState.Stopped, ThreadState.StopRequested) == false) && 
                                        (expTime.Ticks > DateTime.Now.Ticks)
                                    )
                                    Thread.Sleep(100);
                            }
                        }
                        catch (Exception ex)
                        {
                            _log?.LogError(_logChannel, _processId, ex, "EX01", clsMthTag2, netProcessId: _netProcessId);
                        }

                        if (_isThreadWorkingEnd == false)
                        {
                            _log?.LogError(_logChannel, _processId, new Exception($@"Fail to stop thread of NetServiceAnswerMan"), "X01", clsMthTag2, netProcessId: _netProcessId);
                        }

                        _isThreadWorkingEnd = true;
                    }
                }
            }
        }
    }
}