using System;
using System.Collections.Generic;

namespace NssIT.Kiosk.Client.ViewPage.Komuter
{
    public class KomuterTicket : IDisposable
    {
        private string _logChannel = "ViewPage";
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

        public void InitTicketDetail()
        {
            if (DetailList != null)
                DetailList.Clear();
            else
                DetailList = new List<KomuterTicketDetail>();

            for (int inx = 0; inx < SelectedNoOfPax; inx++)
            {
                DetailList.Add(new KomuterTicketDetail());
            }
        }

        public void UpdateDetailList(KomuterTicketDetail[] ticketDetailList)
        {
            try
            {
                if ((ticketDetailList != null) && (DetailList?.Count > 0))
                {
                    if (ticketDetailList.Length == DetailList.Count)
                    {
                        for (int inx = 0; inx < ticketDetailList.Length; inx++)
                        {
                            KomuterTicketDetail det = ticketDetailList[inx];
                            DetailList[inx].Name = det.Name;
                            DetailList[inx].MyKadId = det.MyKadId;
                        }
                    }
                    else
                        throw new Exception("Komuter detailList count mismatched; (EXIT10001201)");
                }
                else
                    throw new Exception("Invalid Komuter detailList parameter; (EXIT10001201)");
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "*", ex, "EX01", "KomuterTicket.UpdateDetailList");
            }
        }

        public void Dispose()
        {
            DetailList = null;
        }
    }


}
