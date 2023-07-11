﻿using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult
{
    public class IM30CardSaleResult : IIM30TransResult, IDisposable
    {
        public IM30DataModel ResultData { get; private set; } = null;
        public Exception Error { get; private set; }
        public bool IsSuccess { get; private set; }
        public bool IsManualStopped { get; private set; } = false;
        public bool IsTimeout { get; private set; } = false;

        public IM30CardSaleResult(IM30DataModel resultData)
        {
            ResultData = resultData;
            IsSuccess = true;
        }

        /// <summary>
        /// Stop Transaction Constructor
        /// </summary>
        /// <param name="isTransactionStopped">If isTimeout is false, isTransactionStopped is always true for stop process constructor</param>
        public IM30CardSaleResult(bool isManualStopped, bool isTimeout = false)
        {
            IsSuccess = false;

            if (isTimeout)
                IsTimeout = true;
            else
                IsManualStopped = true;
        }

        public IM30CardSaleResult(Exception error, IM30DataModel resultData)
        {
            ResultData = resultData;
            Error = error;
            IsSuccess = false;
        }

        public void Dispose()
        {
            ResultData = null;
            Error = null;
        }
    }
}
