using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class KomuterPaymentModel
    {
        public string Booking_Id { get; set; }
        public string BookingNo { get; set; }

        public string Error { get; set; }
        public string ErrorMessage { get; set; }
    }
}
