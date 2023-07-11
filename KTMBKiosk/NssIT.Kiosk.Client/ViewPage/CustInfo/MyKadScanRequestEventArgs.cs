using NssIT.Kiosk.Client.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
    public class MyKadScanRequestEventArgs : EventArgs
    {
        public Guid[] _tripScheduleSeatLayoutDetails_Ids { get; private set; } = null;

        public MyKadScanRequestEventArgs(Guid[] tripScheduleSeatLayoutDetails_Ids)
        {
            _tripScheduleSeatLayoutDetails_Ids = tripScheduleSeatLayoutDetails_Ids;
        }
        public string PassengerLineNo { get; private set; }
    }
}