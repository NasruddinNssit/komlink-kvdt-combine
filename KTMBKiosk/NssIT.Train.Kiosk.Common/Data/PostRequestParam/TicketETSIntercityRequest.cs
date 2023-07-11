using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.PostRequestParam
{
    [Serializable]
    public class TicketETSIntercityRequest : IPostRequestParam
    {
        public string BookingNo { get; set; }
    }
}
