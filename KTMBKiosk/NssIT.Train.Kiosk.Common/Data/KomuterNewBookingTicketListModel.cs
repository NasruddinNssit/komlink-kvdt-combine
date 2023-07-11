using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class KomuterNewBookingTicketListModel
    {
        public string TicketTypeId { get; set; }
        public int Quantity { get; set; }
        public KomuterNewBookingTicketDetailListModel[] DetailList { get; set; }
    }
}
