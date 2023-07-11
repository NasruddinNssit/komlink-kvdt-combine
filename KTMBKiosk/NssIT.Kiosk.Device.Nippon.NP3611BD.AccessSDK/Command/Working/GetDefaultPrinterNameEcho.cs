using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working
{
    [Serializable]
    public class GetDefaultPrinterNameEcho : INP3611CommandSuccessAnswer
    {
        public string ModelTag => "NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working.DetectDefaultPrinterNameAccessAnswer";
        public string PrinterName { get; private set; }

        public GetDefaultPrinterNameEcho(string printerName)
        {
            PrinterName = printerName;
        }
    }
}
