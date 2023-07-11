using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CardReader;
using NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.CardReader.Business
{
    public interface ICardReaderBusiness : IDisposable 
    {
        event EventHandler<TrxCallBackEventArgs> OnCompleteCallback;
        event EventHandler<InProgressEventArgs> OnInProgressCall;
        void Pay(string ProcessId, string amount, AccountType accType, string qrId = null, string qrNo = null, string docNumbers = null);
        void SettlePayment(string ProcessId, string host);
        void CancelRequest();
        void ForceToCancel();
        void SoftShutDown();
    }
}
