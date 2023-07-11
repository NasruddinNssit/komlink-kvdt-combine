using NssIT.Train.Kiosk.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kvdt_kiosk.Models
{
    public class CompletePaymentBookingModel
    {

        public KomuterPaymentModel AFCBookingPaymentResult { get; set; }
        public AFCTicketPrintModel[] AFCTicketPrintList { get; set; }
    }
}
