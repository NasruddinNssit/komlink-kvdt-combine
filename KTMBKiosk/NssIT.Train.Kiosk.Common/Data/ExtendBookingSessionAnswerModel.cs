using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class ExtendBookingSessionAnswerModel
    {
        public string Booking_Id { get; set; } = "";
        public DateTime BookingExpiredDateTime { get; set; }
        public double BookingRemainingInSecond { get; set; } = 0;
        public string Error { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
    }
}