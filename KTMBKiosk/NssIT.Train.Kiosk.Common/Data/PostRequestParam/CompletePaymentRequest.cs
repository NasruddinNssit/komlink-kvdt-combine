using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.PostRequestParam
{
    [Serializable]
    public class CompletePaymentRequest : IPostRequestParam
    {
        public string Booking_Id { get; set; }
        public string MCurrencies_Id { get; set; }
        public string MCounters_Id { get; set; }
        public string Channel { get; set; }

        public decimal Amount { get; set; }
        public string FinancePaymentMethod { get; set; }
        public string ReferenceNo { get; set; }

        public CreditCardResponseModel CreditCardResponse { get; set; }
        public BTnGSuccessPaidModel PaymentGatewaySuccessPaidModel { get; set; }
    }
}
