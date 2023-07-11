using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Common.AppService.Network.SignalR
{
    /// <summary>
    /// SignalR Communication
    /// </summary>
    public class SignalRClientCom : IDisposable
    {
        private bool _disposed = false;
        private string _signalRSvrUrl = "";
        private string _signalRClientTag = "*";

        private string _authenticationHeader = null;
        private string _authenticationValue = "";

        private ConcurrentDictionary<string, MessageReceiveAction> _receiveMessageActionList = new ConcurrentDictionary<string, MessageReceiveAction>();

        public SignalRClientCom(string clientTag, string signalRSvrUrl, string authenticationHeader, string authenticationValue)
        {
            if (string.IsNullOrWhiteSpace(signalRSvrUrl))
                throw new Exception("Invalid SignalR Service URL");

            _signalRSvrUrl = signalRSvrUrl;

            if (string.IsNullOrWhiteSpace(_authenticationHeader) == false)
                _authenticationHeader = authenticationHeader;

            _authenticationValue = authenticationValue;

            if (string.IsNullOrWhiteSpace(clientTag) == false)
                _signalRClientTag = clientTag;
        }

        private DbLog _log = null;
        private DbLog Log
        {
            get
            {
                return _log ?? (_log = DbLog.GetDbLog());
            }
        }

        /// <summary>
        /// Add method that allow SignalR Server to callback.
        /// </summary>
        /// <param name="methodName">method name must start with small letter</param>
        /// <param name="runAction"></param>
        public void AddReceiveMessageAction(string methodName, MessageReceiveAction runAction)
        {
            bool x1 = _receiveMessageActionList.TryRemove(methodName, out _);

            if (_receiveMessageActionList.TryAdd(methodName, runAction) == false)
            {
                throw new Exception($@"Unable to add SignalR Client Receive Message Action ({methodName}); SnR.Tag: {_signalRClientTag};");
            }
        }

        // Add SendEvent


        // Start Communication

        // Stop Communication

        // Connection Checking

        // Shutdown-Dispose Service
        public void Dispose()
        {
            if (_disposed == false)
            {
                _disposed = true;
            }
        }

        class SnRSendMessagePack
        {

        }
    }
}
