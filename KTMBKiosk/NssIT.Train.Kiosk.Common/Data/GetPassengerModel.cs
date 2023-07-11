using NssIT.Train.Kiosk.Common.Common.WebApi;
using NssIT.Train.Kiosk.Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class GetPassengerModel : IDisposable
    {
        public string Booking_Id { get; set; }
        public string Currency { get; set; }
        public decimal TotalAmount { get; set; }
        public string TotalAmountFormat { get; set; }
        public string Error { get; set; }
        public string ErrorMessage { get; set; }
        public PassengerResultDetailModel[] PassengerDetailModels { get; set; }
        public void Dispose()
        {
            PassengerDetailModels = null;
        }
    }
}

