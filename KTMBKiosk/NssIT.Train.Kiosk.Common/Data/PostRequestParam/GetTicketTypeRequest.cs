using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.PostRequestParam
{
    [Serializable]
    public class GetTicketTypeRequest : IPostRequestParam
    {
        public string TrainService { get; set; } = string.Empty;
        public string MCounters_Id { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
    }
}
