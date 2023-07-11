using System.Collections.Generic;

namespace kvdt_kiosk.Models
{
    public class AFCAddOn
    {
        public bool Status { get; set; }
        public List<string> Messages { get; set; }
        public object Code { get; set; }
        public List<AFCAddOnDetails> Data { get; set; }
    }

    public class AFCAddOnDetails
    {
        public bool IsConcession { get; set; }
        public bool IsSinglePassengerOnly { get; set; }
        public string AddOnId { get; set; }
        public string AddOnName { get; set; }
        public object AddOnCount { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
