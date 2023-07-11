using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class KomuterSummaryModel
    {
        public decimal Distance { get; set; }
        public string StationNameFrom { get; set; }
        public string StationNameTo { get; set; }
        public string MCurrencies_Id { get; set; }
        public KomuterPackageModel[] KomuterPackages { get; set; }
    }
}