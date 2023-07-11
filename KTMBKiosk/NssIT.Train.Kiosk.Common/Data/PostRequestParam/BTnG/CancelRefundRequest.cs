using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.PostRequestParam.BTnG
{
    [Serializable]
    public class CancelRefundRequest : IPostRequestParam
    {
        public string DeviceId { get; set; }
        public BTnGCancelRefundReqInfo CancelRefundRequestInfo { get; set; }
    }
}
