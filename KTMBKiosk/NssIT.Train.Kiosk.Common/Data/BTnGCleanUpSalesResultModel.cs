using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class BTnGCleanUpSalesResultModel
    {
        public string BTnGSaleTransactionNo { get; set; }
        public DateTime SaleCreatedTime { get; set; } = DateTime.MinValue;
        public string BookingNo { get; set; }
        public string DeviceId { get; set; }
        public string KioskId { get; set; }
        public string DetailResultStatus { get; set; }
        public string ErrorResultMessage { get; set; }
    }
}
