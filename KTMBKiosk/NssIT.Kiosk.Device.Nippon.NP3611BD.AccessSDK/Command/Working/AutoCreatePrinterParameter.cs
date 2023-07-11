using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working
{
    [Serializable]
    public class AutoCreatePrinterParameter : INP3611CommandParameter
    {
        public string ModelTag => "NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working.AutoCreatePrinterAccessParameter";
        public bool CheckPrinterPaperLowState { get; private set; }

        public AutoCreatePrinterParameter(bool checkPrinterPaperLowState)
        {
            CheckPrinterPaperLowState = checkPrinterPaperLowState;
        }
    }
}
