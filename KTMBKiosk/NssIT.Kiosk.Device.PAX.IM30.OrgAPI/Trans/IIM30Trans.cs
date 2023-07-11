using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans
{
    public interface IIM30Trans
    {
        Guid WorkingId { get; }
        string COMPort { get; }
        TransactionTypeEn TransactionType { get; }
        bool? IsTransStartSuccessful { get; }
        bool IsCurrentWorkingEnded { get; }
        bool IsTransEndDisposed { get; }
        bool IsPerfectCompleteEnd { get; }
        IIM30TransResult FinalResult { get; }
        bool StartTransaction(out Exception error);
        void ShutdownX();
        IIM30TransResult NewErrFinalResult(string errorMessage);
    }
}
