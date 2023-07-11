using System;
using System.Collections.Generic;

namespace kvdt_kiosk.Views.Kvdt.SeatingScreen.New
{
    public class KomuterTicket : IDisposable
    {
        public int SelectedNoOfPax { get; private set; }
        public string Currency { get; private set; }
        public string TicketTypeId { get; private set; }
        public string TicketTypeDescription { get; private set; }

        public decimal TicketPrice { get; private set; }

        public bool VerifyMalaysianKomuter { get; private set; }

        public bool IsVerifyAgeKomuterRequired { get; private set; }

        public int AgeMinKomuter { get; private set; }

        public int AgeMaxKomuter { get; private set; }

        public List<KomuterTicketDetail> DetailList { get; private set; }

        public KomuterTicket(int selectedNoOfPax, string currency, string ticketTypeId, string ticketTypeDescription, decimal ticketPrice,
           bool verifyMalaysianKomuter, bool isVerifyAgeKomuterRequired, int ageMinKomuter, int ageMaxKomuter)
        {
            SelectedNoOfPax = selectedNoOfPax;
            Currency = currency;
            TicketTypeId = ticketTypeId;
            TicketTypeDescription = ticketTypeDescription;
            TicketPrice = ticketPrice;

            VerifyMalaysianKomuter = verifyMalaysianKomuter;
            IsVerifyAgeKomuterRequired = isVerifyAgeKomuterRequired;
            AgeMinKomuter = ageMinKomuter;
            AgeMaxKomuter = ageMaxKomuter;
        }

        //public void InitTicketDetail()
        //{

        //}

        public void Dispose()
        {
            DetailList = null;
        }

    }
}
