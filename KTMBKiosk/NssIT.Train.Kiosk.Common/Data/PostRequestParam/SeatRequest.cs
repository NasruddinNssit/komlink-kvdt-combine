using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.PostRequestParam
{
    [Serializable]
    public class SeatRequest : IPostRequestParam
    {
        public string Id { get; set; }
        public string MCounters_Id { get; set; }
        public string Channel { get; set; }
    }
}