using NssIT.Train.Kiosk.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class PassengerDetailErrorModel
    {
        public Guid SeatLayoutModel_Id { get; set; } = Guid.Empty;
        public string PassengerName { get; set; }
        public string PassengerIC { get; set; }
        public string Gender { get; set; }
        public string PNR { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public string TicketTypes_Id { get; set; }
        public string Penalties_Id { get; set; } = string.Empty;
        public string BookingType { get; set; }
        public string PromoCode { get; set; } = string.Empty;
        public string PromoError { get; set; } = YesNo.No;
        public string PromoErrorMessage { get; set; } = string.Empty;
    }
}
