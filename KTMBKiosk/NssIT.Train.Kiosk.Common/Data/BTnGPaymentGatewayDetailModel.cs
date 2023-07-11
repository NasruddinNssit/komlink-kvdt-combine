using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class BTnGPaymentGatewayDetailModel
    {
        public string PaymentGateway { get; set; }
        public string PaymentGatewayName { get; set; }
        public string DisplayName { get; set; }
        public string Logo { get; set; }
        public string MdPaymentGatewayTypeKey { get; set; }
        public string PaymentMethod { get; set; }
    }
}
