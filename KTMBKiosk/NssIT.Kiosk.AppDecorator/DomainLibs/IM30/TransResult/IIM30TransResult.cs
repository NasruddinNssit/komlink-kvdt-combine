using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult
{
    public interface IIM30TransResult
    {
        IM30DataModel ResultData { get; }
        Exception Error { get; }
        bool IsSuccess { get; }
        bool IsManualStopped { get; }
        bool IsTimeout { get; }
    }
}
