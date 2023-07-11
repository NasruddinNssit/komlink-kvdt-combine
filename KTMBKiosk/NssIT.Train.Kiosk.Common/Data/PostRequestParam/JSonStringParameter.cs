using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.PostRequestParam
{
    [Serializable]
    public class JSonStringParameter : IPostRequestParam
    {
        public string JSonString { get; set; }
    }
}
