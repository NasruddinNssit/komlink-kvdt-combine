using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfBoostTouchNGoSample.Data;
using WpfBoostTouchNGoSample.Data.Request;
using WpfBoostTouchNGoSample.Data.Response;

namespace WpfBoostTouchNGoSample.Base.Extension
{
    public static class MyExtension
    {
        public static string ToJSonStringX(this PaymentGatewayResp obj)
        {
            string retStr = JsonConvert.SerializeObject(obj, Formatting.Indented);
            if (obj != null)
                retStr = JsonConvert.SerializeObject(obj, Formatting.Indented);
            return retStr;
        }
        public static string ToJSonStringX(this CreateSaleResp obj)
        {
            string retStr = JsonConvert.SerializeObject(obj, Formatting.Indented);
            if (obj != null)
                retStr = JsonConvert.SerializeObject(obj, Formatting.Indented);
            return retStr;
        }

        public static string GetSignatureString(this CreateSaleReq sale)
        {
            string formatedHashMsg = $@"{sale.MerchantId}{sale.MerchantTransactionNo}{sale.PaymentGateway}{sale.Currency}{sale.Amount.ToString("0.##")}{sale.ExpirySecond}{DateTime.Now.ToString("yyyyMMdd")}";
            return PaymentGuard.HashRSAKey(formatedHashMsg);
        }
    }
}
