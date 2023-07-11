using NssIT.Train.Kiosk.Common.Common.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.Response
{
    [Serializable]
    public class UpdateKioskStatusResult<T> : BaseServiceResult<T>, iWebApiResult
        where T : BaseCommonObj
    {
        public override T Data { get; set; }

        public UpdateKioskStatusResult() { }
    }
}