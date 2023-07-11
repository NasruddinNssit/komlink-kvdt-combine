using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.Response
{
    [Serializable]
    public class UpdatePassengerCompiledResult : IDisposable
    {
        public UpdatePassengerResult UpdatePassengerResult { get; set; }
        public GetPassengerModel GetPassengerResult { get; set; }
        public CheckoutBookingModel CheckoutBookingResult { get; set; }

        public void Dispose()
        {
            UpdatePassengerResult = null;
            GetPassengerResult = null;
            CheckoutBookingResult = null;
        }
    }
}
