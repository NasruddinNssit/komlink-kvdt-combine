using kvdt_kiosk.Views.Kvdt.SeatingScreen.New;
using System;

namespace kvdt_kiosk.Views.SeatingScreen.New.Kvdt
{
    public class TicketSelectionChangedEventArgs : EventArgs, IDisposable
    {

        public KomuterTicket[] TicketList { get; private set; }

        public string KomuterPackageId { get; set; }
        public string KomuterPackageDescription { get; set; }

        public string KomuterPackageDuration { get; set; }


        public TicketSelectionChangedEventArgs(string komuterPackageId, string komuterPackageDescription, string komuterPackageDuration, KomuterTicket[] ticketList)
        {
            TicketList = ticketList;
            KomuterPackageId = komuterPackageId;
            KomuterPackageDescription = komuterPackageDescription;
            KomuterPackageDuration = komuterPackageDuration;
        }
        public void Dispose()
        {
            TicketList = null;
        }
    }
}
