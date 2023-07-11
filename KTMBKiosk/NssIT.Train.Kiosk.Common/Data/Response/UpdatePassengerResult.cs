using NssIT.Train.Kiosk.Common.Common.WebApi;
using NssIT.Train.Kiosk.Common.Helper;
using NssIT.Train.Kiosk.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.Response
{
    [Serializable]
    public class UpdatePassengerResult : IDisposable
    {
        public string Booking_Id { get; set; }
        public DateTime BookingExpiredDateTime { get; set; }
        public double BookingRemainingInSecond { get; set; }
        public string MCurrencies_Id { get; set; }
        public decimal PayableAmount { get; set; }
        public string IsAllowInsurnace { get; set; } = YesNo.No;
        public string Error { get; set; }
        public string ErrorMessage { get; set; }
        public PassengerDetailErrorModel[] PassengerDetailErrorModels { get; set; }

        public void Dispose()
        {
            PassengerDetailErrorModels = null;
        }
    }
}
