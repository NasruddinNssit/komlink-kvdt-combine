using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class KomuterBookingModel
    {
        public string Booking_Id { get; set; }
        public string BookingNo { get; set; }
        public string MCurrencies_Id { get; set; }

        public decimal TotalAmount { get; set; }
        public DateTime BookingExpiredDateTime { get; set; }
        public double BookingRemainingInSecond { get; set; }
        public string Error { get; set; }
        public string ErrorMessage { get; set; }
    }
}
