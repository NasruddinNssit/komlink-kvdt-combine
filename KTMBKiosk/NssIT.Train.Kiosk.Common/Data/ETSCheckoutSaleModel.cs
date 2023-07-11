using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class ETSCheckoutSaleModel : IDisposable
    {
        public GetPassengerModel GetPassengerResult { get; set; }
        public CheckoutBookingModel CheckoutBookingResult { get; set; }

        public void Dispose()
        {
            GetPassengerResult = null;
            CheckoutBookingResult = null;
        }
    }
}
