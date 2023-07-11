using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.PostRequestParam
{
    [Serializable]
    public class CheckOutstandingCardSettlementRequest : IPostRequestParam
    {
        public string MachineId { get; set; }
    }
}
