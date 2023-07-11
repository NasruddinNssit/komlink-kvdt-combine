using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.Response
{
    [Serializable]
    public class BookingPaymentResult
    {
        public string Booking_Id { get; set; }
        public string BookingNo { get; set; }
        /// <summary>
        /// Refer to YesNo Constant
        /// </summary>
        public string Error { get; set; }
        public string ErrorMessage { get; set; }
    }
}
