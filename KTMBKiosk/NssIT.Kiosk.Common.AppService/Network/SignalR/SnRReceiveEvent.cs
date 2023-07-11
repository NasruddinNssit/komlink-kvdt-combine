using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Common.AppService.Network.SignalR
{
    public delegate void MessageReceiveAction(string jsonObjectString);

    public class SnRReceiveEvent
    {
        public string MethodName { get; private set; }
        public MessageReceiveAction RunAction { get; private set; }

        public SnRReceiveEvent(string methodName, MessageReceiveAction runAction)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                throw new Exception("SignalR Client method name cannot be null/empty.");

            if (runAction is null)
                throw new Exception("SignalR Client method cannot be nully.");

            MethodName = methodName;
            RunAction = runAction;
        }
    }
}