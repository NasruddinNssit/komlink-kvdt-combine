using NssIT.Kiosk.Log.DB.MarkingLog.TriggerNTimeInterval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WFmMarkingLogTest.TestLib
{
    public class LogTest01 : IDisposable
    {
        private TriggerTimeIntervalMarkingLog _log = null;
        private Thread _tWorker = null;
        private object _tLock = new object();
        private string _markMsg = null;
        private bool _isLogTriggered = false;

        public LogTest01(string clsMetName,
            int intervalLogReqSec, int minDataCountOnLog, int maxIntervalLogReqMinutes = 0, int validLogIntervalSec = 0)
        {
            _log = new TriggerTimeIntervalMarkingLog(clsMetName,
                intervalLogReqSec, minDataCountOnLog, maxIntervalLogReqMinutes, validLogIntervalSec);

            _tWorker = new Thread(TestThreadWorking);
            _tWorker.IsBackground = true;
            _tWorker.Start();
        }

        private bool _disposed = false;
        public void Dispose()
        {
            _disposed = true;
        }

        public void TestThreadWorking()
        {
            string markMsg = null;
            bool isLogTriggered = false;

            while (_disposed == false)
            {
                markMsg = _markMsg;
                isLogTriggered = _isLogTriggered;

                if (string.IsNullOrWhiteSpace(markMsg))
                {
                    lock (_tLock)
                    {
                        Monitor.Wait(_tLock, 500);
                    }
                }
                else
                {
                    _markMsg = null;
                    _isLogTriggered = false;
                    _log.LogMark(markMsg, isLogTriggered);
                }
            }
        }

        public void SendLog(string markStr, bool isLogTriggered = false)
        {
            lock (_tLock)
            {
                _markMsg = markStr;
                _isLogTriggered = isLogTriggered;
                Monitor.PulseAll(_tLock);
            }
        }

        public void SendOutstanding()
        {
            _log.SendOutstanding();
        }

        private int _sendMultipleCount = 0;
        private object _sendMultipleLock = new object();
        public void SendMultiple(int numOfMsg, bool isLogTriggered = false)
        {
            Thread tWorker = new Thread(delegate() 
            {
                lock (_sendMultipleLock)
                {
                    int inx = 0;
                    while (inx < numOfMsg)
                    {
                        _log.LogMark($@"MP-{_sendMultipleCount}", isLogTriggered);

                        inx++;
                        _sendMultipleCount++;
                    }
                }
            });
            tWorker.IsBackground = true;
            tWorker.Start();
        }
    }
}
