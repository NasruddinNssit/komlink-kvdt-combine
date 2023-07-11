using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfBoostTouchNGoSample.Data.Response
{
    /// <summary>
    /// Create/New Sales Response
    /// </summary>
    public class CreateSaleResp : BaseResponse
    {
        public MerchantSaleInfo Data { get; set; }
    }

    public class PaymentGatewayResp : BaseResponse
    {
        public PaymentGateway Data { get; set; }
    }
}