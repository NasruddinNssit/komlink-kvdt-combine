using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base
{
    interface IIM30TransactionProcess
    {
        IIM30TransResult FinalResult { get; }
        bool IsCurrentWorkingEnded { get; }
        bool IsShutdown { get; }

        //IM30DataModel GetNewIM30Data();
        //bool StartTransaction(out Exception error);
    }
}
