using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfBoostTouchNGoAPITest.Data.Response
{
    public class PaymentGatewayResp
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public string Code { get; set; }
        public PaymentGateway Data { get; set; }
    }
}