using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.PostRequestParam
{
    [Serializable]
    public class PassengerPNRTicketTypeRequest : IPostRequestParam
    {
        public string Booking_Id { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string MCounters_Id { get; set; } = string.Empty;
        public string PNRNo { get; set; } = string.Empty;
        public string IdentityNo { get; set; } = string.Empty;
        public Guid[] TripScheduleSeatLayoutDetails_Ids { get; set; } = new Guid[0];
    }
}
