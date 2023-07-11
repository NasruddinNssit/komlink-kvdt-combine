using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komlink.Views
{
    public class OnFailureScanEventArgs : EventArgs
    {
      
       public int retryCount { get; set; }

      public OnFailureScanEventArgs(int retryCount)
        {
          
            this.retryCount = retryCount;
        }
    }
}
