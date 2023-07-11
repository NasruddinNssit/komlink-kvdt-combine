using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class CompleteKomuterPaymentCompiledModel
    {
        public KomuterPaymentModel KomuterBookingPaymentResult { get; set; }
        public KomuterPrintTicketModel[] KomuterTicketPrintList { get; set; }
    }
}
