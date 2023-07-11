using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfBoostTouchNGoSample.Data.Request
{
    public class SnRTestMessageReq
    {
        public string DestinationId { get; set; } = "";
        public string Message { get; set; } = "";
        public string DestinationMethodName { get; set; } = "";
    }
}
