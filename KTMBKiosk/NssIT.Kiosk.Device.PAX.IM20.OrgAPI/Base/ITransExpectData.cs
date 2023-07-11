using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base
{
    public interface ITransExpectData
    {
        string ExpectedDataTag { get; }

        void InitReader(PayECRComPort goSerialPort, string processId);
        bool IsFound(dynamic data);

        /// <summary>
        /// -1 (or -ve) to indicate any variant
        /// </summary>
        int ExpectedDataLength { get; }

        void EndDispose();
    }
}
