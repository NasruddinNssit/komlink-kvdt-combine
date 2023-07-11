using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Seat
{
    public class ServiceTypeSelectedEventArgs : EventArgs
    {
        public string ServiceType { get; private set; } = null;

        public ServiceTypeSelectedEventArgs(string serviceType)
        {
            ServiceType = serviceType;
        }
    }
}
