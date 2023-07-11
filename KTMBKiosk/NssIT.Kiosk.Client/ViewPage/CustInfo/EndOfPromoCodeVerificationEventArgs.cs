using NssIT.Kiosk.Client.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
    public class EndOfPromoCodeVerificationEventArgs : EventArgs, IDisposable
    {
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; } = null;

        /// <summary>
        /// Success Result constructor
        /// </summary>
        public EndOfPromoCodeVerificationEventArgs()
        {
            IsSuccess = true;
        }

        /// <summary>
        /// Fail Result constructor
        /// </summary>
        /// <param name="errorMessage"></param>
        public EndOfPromoCodeVerificationEventArgs(string errorMessage)
        {
            IsSuccess = false;
            ErrorMessage = errorMessage;
        }

        public void Dispose()
        { }
    }
}
