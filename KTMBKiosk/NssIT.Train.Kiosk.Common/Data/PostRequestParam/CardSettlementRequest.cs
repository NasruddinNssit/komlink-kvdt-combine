using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.PostRequestParam
{
    [Serializable]
    public class CardSettlementRequest : IPostRequestParam
    {
        public string HostNo { get; set; }
        public string BatchNumber { get; set; }
        public string BatchCount { get; set; }
        public decimal BatchCurrencyAmount { get; set; }
        public string StatusCode { get; set; }
        public string MachineId { get; set; }
        public string ErrorMessage { get; set; }
    }
}