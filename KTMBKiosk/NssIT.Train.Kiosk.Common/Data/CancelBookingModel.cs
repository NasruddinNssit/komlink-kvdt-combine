using NssIT.Train.Kiosk.Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NssIT.Train.Kiosk.Common.Constants;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class CancelBookingModel
    {
        public string Booking_Id { get; set; }
        public DateTime BookingExpiredDateTime { get; set; } = DateTimeHelper.GetMinDate();
        public double BookingRemainingInSecond { get; set; } = 0;
        public string Error { get; set; } = YesNo.No;

        /// <summary>
        /// 0 for no error. 1 for has error.
        /// </summary>
        public int ErrorCode { get => (((Error ?? YesNo.No).Equals(YesNo.Yes)) ? 1 : 0); }
        public string ErrorMessage { get; set; } = "";
    }
}
