using NssIT.Kiosk.Device.Nippon.NP3611BD.OrgAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working
{
    [Serializable]
    public class AutoCreatePrinterEcho : INP3611CommandSuccessAnswer
    {
        public string ModelTag => "NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working.AutoCreatePrinterAccessAnswer";
        public string PrinterName { get; private set; }
        public NP3611BDPrinter PrinterApi { get; private set; }

        public AutoCreatePrinterEcho(string printerName, NP3611BDPrinter printerApi)
        {
            PrinterName = printerName;
            PrinterApi = printerApi;
        }

        public void DetachPrinterApi()
        {
            PrinterApi = null;
        }
    }
}
