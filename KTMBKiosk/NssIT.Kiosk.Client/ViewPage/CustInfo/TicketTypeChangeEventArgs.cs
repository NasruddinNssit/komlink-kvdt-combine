using NssIT.Kiosk.Client.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
    public class TicketTypeChangeEventArgs : EventArgs
    {
        public TicketTypeChangeEventArgs(string ticketTypeId, string ticketTypeDescription)
        {
            TicketTypeId = ticketTypeId;
            TicketTypeDescription = ticketTypeDescription;
        }

        public string TicketTypeId
        {
            get; private set;
        }

        public string TicketTypeDescription
        {
            get; private set;
        }
    }
}
