using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class BTnGCleanUpSalesResultCollectionModel
    {
        public int OutStandingSalesCount { get; set; } = 0;
        public int ErrorCount { get; set; } = 0;
        public string FirstErrorMessage { get; set; } = null;
        public BTnGCleanUpSalesResultModel[] CleanUpSalesResultList { get; set; }
    }
}
