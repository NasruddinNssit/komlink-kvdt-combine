using NssIT.Kiosk.Client.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
    public class PromoCodeVerificationEventArgs : EventArgs, IDisposable
    {
        public string TrainSeatModelId { get; private set; }
        public Guid SeatLayoutModelId { get; private set; }
        public string PromoCode { get; private set; }
        public string TicketTypeId { get; private set; }
        public string PassengerIC { get; private set; }

        public PromoCodeVerificationEventArgs(string trainSeatModelId, Guid seatLayoutModelId, string ticketTypeId, string passengerIC, string promoCode)
        {
            TrainSeatModelId = trainSeatModelId;
            SeatLayoutModelId = seatLayoutModelId;
            TicketTypeId = ticketTypeId;
            PassengerIC = passengerIC;
            PromoCode = promoCode;
        }

        public void Dispose()
        { }
    }
}
