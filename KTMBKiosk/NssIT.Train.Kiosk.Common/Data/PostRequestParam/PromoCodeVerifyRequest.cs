using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.PostRequestParam
{
    [Serializable]
    public class PromoCodeVerifyRequest : IPostRequestParam
    {
        public string TrainSeatModel_Id { get; set; }
        public Guid? TripBookingDetails_Id { get; set; } = Guid.Empty;
        public Guid SeatLayoutModel_Id { get; set; } = Guid.Empty;
        public string TicketTypes_Id { get; set; } = string.Empty;
        public string PromoCode { get; set; } = string.Empty;
        public string PassengerIC { get; set; } = string.Empty;
        public string Channel { get; set; }
    }
}