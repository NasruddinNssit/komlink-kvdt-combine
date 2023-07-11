using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.PostRequestParam
{
    [Serializable]
    public class UpdatePaymentRequest : IPostRequestParam
    {
        public string Booking_Id { get; set; }
        public string MCurrencies_Id { get; set; }
        public string MCounters_Id { get; set; }
        public string Channel { get; set; }
        public BookingPaymentDetailModel[] BookingPaymentDetailModels { get; set; }
    }
}
