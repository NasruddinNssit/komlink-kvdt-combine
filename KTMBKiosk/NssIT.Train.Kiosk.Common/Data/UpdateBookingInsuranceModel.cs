using NssIT.Train.Kiosk.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class UpdateBookingInsuranceModel
    {
        public string Booking_Id { get; set; }
        public DateTime BookingExpiredDateTime { get; set; }
        public double BookingRemainingInSecond { get; set; }
        public string MCurrencies_Id { get; set; } = "";
        public decimal PayableAmount { get; set; } = 0;

        /// <summary>
        /// Refer to YesNo
        /// </summary>
        public string Error { get; set; } = YesNo.No;
        public string ErrorMessage { get; set; } = "";
    }
}
