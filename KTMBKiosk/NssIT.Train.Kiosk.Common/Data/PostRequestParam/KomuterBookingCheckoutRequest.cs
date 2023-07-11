using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.PostRequestParam
{
    [Serializable]
    public class KomuterBookingCheckoutRequest : IPostRequestParam
    {
        public string Channel { get; set; }
        public string MCounter_Id { get; set; }
        public string Booking_Id { get; set; }
        public decimal TotalAmount { get; set; }
        public string FinancePaymentMethod { get; set; }
    }
}
