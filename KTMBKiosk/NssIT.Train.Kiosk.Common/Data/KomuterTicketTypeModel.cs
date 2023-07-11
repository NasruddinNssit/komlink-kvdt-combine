using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class KomuterTicketTypeModel
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public bool IsConcession { get; set; }
        public bool VerifyMalaysianKomuter { get; set; }
        public bool IsVerifyAgeKomuterRequired { get; set; }
        public int AgeMinKomuter { get; set; }
        public int AgeMaxKomuter { get; set; }
        public bool IsVerifyPeopleSoftRequired { get; set; }
        public decimal Amount { get; set; }
        public decimal TicketBasePrice { get; set; }
        public decimal TicketDiscountPercentage { get; set; }
    }
}