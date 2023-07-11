using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Nippon.NP3611BD.OrgAPI.Data
{
    [Serializable]
    public class PrinterStatus : INP3611BDData
    {
        public bool IsPrinterError { get; set; } = false;
        public bool IsPrinterWarning { get; set; } = false;
        public string StandardStatusDescription { get; set; } = "";
        public string LocalStatusDescription { get; set; } = "";
    }
}
