using NssIT.Train.Kiosk.Common.Common.WebApi;
using NssIT.Train.Kiosk.Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.Response
{
    [Serializable]
    public class BookingResult : iWebApiResult, IDisposable
    {
        public bool Status { get; set; }
        public BookingModel Data { get; set; }
        public IList<string> Messages { get; set; }
        public string Code { get; set; }

        public string MessageString()
        {
            string msgStr = null;
            foreach (string msg in Messages)
                msgStr += $@"{msg}; ";
            return msgStr;
        }

        public void Dispose()
        {
            Data = null;
            Messages = null;
        }
    }
}
