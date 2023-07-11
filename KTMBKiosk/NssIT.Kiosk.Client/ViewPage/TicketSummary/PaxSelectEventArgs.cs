using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.TicketSummary
{
    public class PaxSelectEventArgs : EventArgs
    {
        public int NumberOfPax { get; private set; }

        public PaxSelectEventArgs (int numberOfPax)
        {
            NumberOfPax = numberOfPax;
        }
    }
}
