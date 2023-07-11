using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.Response
{
    [Serializable]
    public class CompletePaymentCompiledResult : IDisposable
    {
        public BookingPaymentResult BookingPaymentResult { get; set; }
        public IntercityETSTicketModel[] IntercityETSTicketListResult { get; set; }

        public void Dispose()
        {
            BookingPaymentResult = null;
            IntercityETSTicketListResult = null;
        }
    }
}
