using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData
{
    public class CardTransResponseEventArgs : EventArgs, IDisposable
    {
        public TransactionTypeEn TransType { get; private set; } = TransactionTypeEn.Unknown;
        public TransEventCodeEn EventCode { get; private set; } = TransEventCodeEn.Unknown;
        public Exception Error { get; private set; } = null;
        public string Message { get; private set; } = null;
        public ICardResponse ResponseInfo { get; private set; } = null;

        /// <summary>
        /// Remaining seconds; CountdownNo >= 0 when EventCode is Countdown
        /// </summary>
        public int? CountdownNo { get; private set; } = 0;

        /// <summary>
        /// Success Constructor; Normally for TransEventCode = CardInfoResponse / Success
        /// </summary>
        /// <param name="transType"></param>
        /// <param name="eventCode"></param>
        /// <param name="ResponseInfo"></param>
        public CardTransResponseEventArgs(TransactionTypeEn transType, TransEventCodeEn eventCode, ICardResponse responseInfo, string message = null)
        {
            TransType = transType;
            EventCode = eventCode;
            ResponseInfo = responseInfo;

            if (string.IsNullOrWhiteSpace(message) == false)
                Message = message;
        }

        /// <summary>
        /// Messaging/Action Request Constructor; Normally for TransEventCode = Message / AcquireCustomerAction / Countdown
        /// </summary>
        /// <param name="transType"></param>
        /// <param name="eventCode"></param>
        /// <param name="message"></param>
        /// <param name="countdown"></param>
        public CardTransResponseEventArgs(TransactionTypeEn transType, TransEventCodeEn eventCode, string message = null, int? countdown = null)
        {
            TransType = transType;
            EventCode = eventCode;

            if (string.IsNullOrWhiteSpace(message) == false)
                Message = message;

            CountdownNo = countdown;
        }

        /// <summary>
        /// Fail/Error Constructor; Normally for TransEventCode = FailWithError / ErrorFound
        /// </summary>
        /// <param name="transType"></param>
        /// <param name="error"></param>
        /// <param name="ResponseInfo"></param>
        /// <param name="eventCode"></param>
        public CardTransResponseEventArgs(TransactionTypeEn transType, Exception error, ICardResponse responseInfo = null,
            TransEventCodeEn eventCode = TransEventCodeEn.ErrorFound)
        {
            TransType = transType;
            EventCode = eventCode;

            if (error?.Message?.ToString().Length > 0)
                Error = error;
            else
                Error = new Exception("--Unknown card exception; Card Trans--");

            ResponseInfo = responseInfo;
        }

        public void Dispose()
        {
            Error = null;
            ResponseInfo = null;
            Message = null;
            CountdownNo = null;
        }
    }
}
