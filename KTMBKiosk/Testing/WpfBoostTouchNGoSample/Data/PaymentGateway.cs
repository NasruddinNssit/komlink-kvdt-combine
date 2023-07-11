using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfBoostTouchNGoSample.Data
{
    public class PaymentGateway
    {
        public string MerchantId { get; set; }
        public PaymentGatewayDetail[] PaymentGatewayList { get; set; }
    }
}