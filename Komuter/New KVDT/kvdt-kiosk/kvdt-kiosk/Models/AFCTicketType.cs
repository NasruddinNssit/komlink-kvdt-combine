using System.Collections.Generic;

namespace kvdt_kiosk.Models
{
    public class AFCTicketType
    {
        public bool Status { get; set; }
        public List<string> Messages { get; set; }
        public object Code { get; set; }
        public List<AFCTicketTypeDetails> Data { get; set; }
    }

    public class AFCTicketTypeDetails
    {
        public bool IsConcession { get; set; }
        public bool IsSinglePassengerOnly { get; set; }
        public string TicketTypeId { get; set; }
        public string TicketTypeName { get; set; }
        public string TicketCount { get; set; }
        public decimal UnitPrice { get; set; }
        public bool VerifyMalaysianAFC { get; set; }
        public bool IsVerifyAgeAFCRequired { get; set; }
        public int AgeMinAFC { get; set; }
        public int AgeMaxAFC { get; set; }
        public bool IsVerifyPeopleSoftRequired { get; set; }
        public string IsPNRAFCRequired { get; set; }
    }
}
