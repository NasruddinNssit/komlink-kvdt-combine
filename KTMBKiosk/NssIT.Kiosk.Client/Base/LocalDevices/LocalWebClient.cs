using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Base.LocalDevices
{
    public class LocalWebClient : System.Net.WebClient
    {
        public int TimeoutMilliSec { get; set; }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest lWebRequest = base.GetWebRequest(uri);
            lWebRequest.Timeout = TimeoutMilliSec;
            ((HttpWebRequest)lWebRequest).ReadWriteTimeout = TimeoutMilliSec;
            return lWebRequest;
        }
    }
}
