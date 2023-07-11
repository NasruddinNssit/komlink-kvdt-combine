using NssIT.Train.Kiosk.Common.Data.PostRequestParam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class BTnGCancelRefundReqInfo
    {
        public string MerchantId { get; set; }
        public string PaymentGateway { get; set; }
        public string MerchantTransactionNo { get; set; }
        public string SalesTransactionNo { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public string Signature { get; set; }
    }
}
