using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kvdt_kiosk.Models
{
    public class AFCCheckOutModel
    {
        public string Booking_Id { get; set; }

        public string PNR { get; set; }

        public string PassengerICNo { get; set; }
        public string PassengerPassportNo { get; set; }

        public string MCounters_Id { get; set; }

        public string CounterUserId { get; set; }

        public string HandheldUserId { get; set; }

        public string PurchaseStationId { get; set; }

        public string Channel { get; set; }

        public IList<PaymentOptionModel> PaymentOptionModels { get; set; } = new List<PaymentOptionModel>();

    }
}
