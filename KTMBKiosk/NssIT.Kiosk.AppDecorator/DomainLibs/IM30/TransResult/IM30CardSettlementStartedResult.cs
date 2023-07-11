using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult
{
    public class IM30CardSettlementStartedResult : IIM30TransResult, IDisposable
    {
        public IM30DataModel ResultData { get; private set; } = null;
        public Exception Error { get; private set; }
        public bool IsSuccess { get; private set; }
        public bool IsManualStopped { get; private set; } = false;
        public bool IsTimeout { get; private set; } = false;

        public IM30CardSettlementStartedResult()
        {
            IsSuccess = true;
        }

        public IM30CardSettlementStartedResult(Exception error)        {
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
