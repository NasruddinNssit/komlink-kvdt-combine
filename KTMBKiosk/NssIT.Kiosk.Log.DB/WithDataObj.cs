using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Log.DB
{
    public class WithDataObj : IDisposable 
    {
        public string DataType => "WithDataObj";
        public string Message { get; private set; }
        public object DataX { get; private set; }
        public WithDataObj(string msg, object data)
        {
            Message = msg;
            DataX = data;
        }

        public void Dispose()
        {
            DataX = null;
        }
    }
}
