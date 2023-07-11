using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working
{
    [Serializable]
    public class ManualCreatePrinterParameter : INP3611CommandParameter
    {
        public string ModelTag => "NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working.ManualCreatePrinterAccessParameter";
        public string PrinterName { get; private set; } = null;
        public bool CheckPrinterPaperLowState { get; private set; } = true;

        public ManualCreatePrinterParameter(string printerName, bool checkPrinterPaperLowState)
        {
            PrinterName = printerName;
            CheckPrinterPaperLowState = checkPrinterPaperLowState;
        }
    }
}
