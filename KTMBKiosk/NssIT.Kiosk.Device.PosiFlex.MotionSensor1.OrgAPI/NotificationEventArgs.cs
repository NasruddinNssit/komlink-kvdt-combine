using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PosiFlex.MotionSensor1.OrgAPI
{
    public class NotificationEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public NotificationEventArgs(string message)
        {
            Message = message ?? "-";
        }
    }
}
