using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NssIT;

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
    public class PassengerInfo
    {
        /// <summary>
        /// PassengerInfoIndex will align with CustSeatDetail[] (Depart & Return) array index, and Passenger Info Entry Layout Record Index.
        /// </summary>
        public int PassengerInfoIndex { get; set; } = 0;
        public string Name { get; set; } = null;
        public string Id { get; set; } = null;
        public string Contact { get; set; } = null;
        public string Gender { get; set; } = "M";
        /// <summary>
        /// This is Seat Type Id Like Adult, Child or ets. This Id is Maintain in a Master file
        /// </summary>
        public string TicketType { get; set; }
        public string DepartPromoCode { get; set; } = null;
        public string ReturnPromoCode { get; set; } = null;
        public string PNR { get; set; } = null;
    }
}
