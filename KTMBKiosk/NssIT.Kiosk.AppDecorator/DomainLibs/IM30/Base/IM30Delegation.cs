using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base
{
    //public delegate void OnTransactionFinishedDelg();
    public delegate void OnTransactionFinishedDelg(IIM30TransResult transResult);
    public delegate void OnCardDetectedDelg(IM30DataModel cardInfo);
}