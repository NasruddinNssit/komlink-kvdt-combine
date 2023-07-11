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
    public class PassengerPNRTicketTypeModel
    {
        public string Booking_Id { get; set; }
        public string BookingNo { get; set; } = "";
        public string Error { get; set; } = YesNo.No;
        public string ErrorMessage { get; set; } = "";
        public string PNRNo { get; set; } = string.Empty;
        public PassengerTicketTypeModel[] PassengerTicketTypeModels { get; set; } = new PassengerTicketTypeModel[0];
    }
}
