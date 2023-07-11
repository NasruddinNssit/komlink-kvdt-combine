using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.PostRequestParam
{
    [Serializable]
    public class UpdateBookingInsuranceRequest : IPostRequestParam
    {
        public string Booking_Id { get; set; }
        public string MInsuranceHeaders_Id { get; set; }
        public string MCounters_Id { get; set; }
        public string CounterUserId { get; set; }
        public string HandheldUserId { get; set; }
        public string PurchaseStationId { get; set; }
        public string Channel { get; set; }
    }
}
