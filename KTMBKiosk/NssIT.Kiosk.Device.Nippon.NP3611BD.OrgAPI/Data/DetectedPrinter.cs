using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Nippon.NP3611BD.OrgAPI.Data
{
    [Serializable]
    public class DetectedPrinter : INP3611BDData 
    {
        public bool IsPrinterDetected { get; set; } = false;
        public string PrinterName { get; set; } = "";
    }
}
