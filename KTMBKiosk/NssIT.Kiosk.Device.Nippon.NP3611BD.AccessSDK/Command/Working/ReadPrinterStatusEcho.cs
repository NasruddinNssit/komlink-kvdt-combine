using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working
{
    [Serializable]
    public class ReadPrinterStatusEcho : INP3611CommandSuccessAnswer
    {
        public string ModelTag => "NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working.ReadPrinterStatusAccessAnswer";
        public bool IsPrinterError { get; private set; } = false;
        public bool IsPrinterWarning { get; private set; } = false;
        public string StatusDescription { get; private set; } = "Normal";
        public string LocalStatusDescription { get; private set; } = "Normal";

        public ReadPrinterStatusEcho(bool isPrinterError, bool isPrinterWarning, string statusDescription, string localStatusDescription)
        {
            IsPrinterError = isPrinterError;
            IsPrinterWarning = isPrinterWarning;
            StatusDescription = statusDescription;
            LocalStatusDescription = localStatusDescription;
        }
    }
}
