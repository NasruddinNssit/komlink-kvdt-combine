using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class KomuterPackageModel
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Duration { get; set; }

        public DateTime TravelStartDateUtc { get; set; }
        public DateTime TravelEndDateUtc { get; set; }
        public KomuterTicketTypeModel[] TicketTypes { get; set; }
    }
}