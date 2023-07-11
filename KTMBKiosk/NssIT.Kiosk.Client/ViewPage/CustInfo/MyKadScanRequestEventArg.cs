using NssIT.Kiosk.Client.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
    public class MyKadScanRequestEventArg : EventArgs
    {
        public MyKadScanRequestEventArg(string passengerLineNo)
        {
            PassengerLineNo = passengerLineNo;
        }
        public string PassengerLineNo { get; private set; }
    }
}