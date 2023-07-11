using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Common.WebApi
{
    public interface iWebApiResult
    {
        bool Status { get; }
        IList<string> Messages { get; set; }
        string Code { get; }
        string MessageString();
    }
}
