using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base
{
    public interface ITransProtocol
    {
        /// <summary>
        /// 0 : Sussess
        /// </summary>
        int ResultStatusCode { get; }
        string ResultstatusRemark { get; }
        string ResultReadData { get; }

        bool Run(int defaultTransactionWaitingTimeSec = 300, int defaultFirstWaitingTimeSec = 60);

        void EndDispose();
    }
}
