﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komlink.Views
{
    public class ScanEventArgs : EventArgs
    {
       public bool isRetryScan { get; set; }
       public int retryCount { get; set; }

      public ScanEventArgs(bool isRetryScan, int retryCount)
        {
            this.isRetryScan = isRetryScan;
            this.retryCount = retryCount;
        }
    }
}
