using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kvdt_kiosk.Models
{
    public class AFCBookingDetailModel
    {
        public string PNR { get; set; } = string.Empty;

        public string PassengerName { get; set; }

        public string PassengerIC { get; set; }

        public string TicketTypes_Id { get; set; }


       
    }
}
