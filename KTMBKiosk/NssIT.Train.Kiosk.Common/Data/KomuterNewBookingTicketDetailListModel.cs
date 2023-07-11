using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class KomuterNewBookingTicketDetailListModel
    {
        public string TicketTypeId { get; set; }
        public string Name { get; set; } = null;
        public string MyKadId { get; set; } = null;
    }
}