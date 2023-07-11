using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class InsuranceModel
    {
        public string MInsuranceHeaders_Id { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public string ShortDescription2 { get; set; }
        public string LongDescription2 { get; set; }
        public decimal Price { get; set; }
        public string PriceColumnPriceFormat { get; set; }
        public DateTime EffectiveFromDateTime { get; set; }
        public DateTime EffectiveFromLocalDateTime { get; set; }
        public string EffectiveFromLocalDateTimeFormat { get; set; }


        public string CoverageShortDescription { get; set; } //short description
        public string CoverageShortDescription2 { get; set; } //short description for bahasa
        public string CoverageLongDescription { get; set; } //long description
        public string CoverageLongDescription2 { get; set; } //long description for bahasa
        public string URL { get; set; }
        public decimal Cost { get; set; }
        public string CostFormat { get; set; }
    }
}
