using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class PassengerTicketTypeModel
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsDutyPass { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsFreePass { get; set; }
        public bool IsLedger { get; set; }
        public bool IsPNRRequired { get; set; }
        public decimal KomuterDiscount { get; set; }
    }
}

