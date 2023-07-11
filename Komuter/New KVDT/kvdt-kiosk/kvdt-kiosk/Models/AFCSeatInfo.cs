using System.Collections.Generic;

namespace kvdt_kiosk.Models
{
    public class AFCSeatInfo
    {
        public bool Status { get; set; }
        public List<string> Messages { get; set; }
        public object Code { get; set; }
        public SeatDetails Data { get; set; }
    }

    public class AddOn
    {
        public bool IsConcession { get; set; }
        public bool IsSinglePassengerOnly { get; set; }
        public string AddOnId { get; set; }
        public string AddOnName { get; set; }
        public object AddOnCount { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class SeatDetails
    {
        public int MaximumPassengerCount { get; set; }
        public object MCurrencies_Id { get; set; }
        public object AFCServiceHeaders_Id { get; set; }
        public object RouteMapUrl { get; set; }
        public List<object> Routes { get; set; }
        public List<object> Stations { get; set; }
        public List<object> Packages { get; set; }
        public List<TicketType> TicketTypes { get; set; }
        public List<AddOn> AddOns { get; set; }
    }

    public class TicketType
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
