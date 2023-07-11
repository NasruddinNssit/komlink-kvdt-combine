using NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base.ExpectReading
{
    public class ExpectSTXStringData : ITransExpectData
    {
        private const string LogChannel = "PAX_IM20_API";

        public string ExpectedDataTag { get; private set; }
        public int ExpectedDataLength => -1;
        /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        private DbLog _log = null;
        private ASCIIEncoding _ascii = new ASCIIEncoding();

        private const byte _STXCode = 2;

        private PayECRComPort _goSerialPort;
        private string _processId = "*";

        public ExpectSTXStringData(string dataTag)
        {
            _log = DbLog.GetDbLog();

            ExpectedDataTag = dataTag;
        }

        public void Dispose()
        {
            _ascii = null;
            _goSerialPort = null;
            _log = null;
        }

        public void EndDispose()
        {
            Dispose();
        }

        public void InitReader(PayECRComPort goSerialPort, string processId)
        {
            _goSerialPort = goSerialPort;
            _processId = processId;
        }

        public bool IsFound(dynamic data)
        {
            bool rIsFound = false;

            if (data is byte[] bytArr)
                rIsFound = IsFoundFirstByteIn(bytArr);

            else if (data is string str)
                rIsFound = IsFoundFirstByteIn(_ascii.GetBytes(str));

            if (rIsFound)
            {
                _log?.LogText(LogChannel, _processId, $@"Received-Process :<{ExpectedDataTag}>", "A10", "ExpectSTXStringData.IsFound");
            }

            return rIsFound;
        }

        private bool IsFoundFirstByteIn(byte[] data)
        {
            // Note : Because this is a string data. So the string should has more then 2 chars (like "<STX>A<ETX>" )
            if (data?.Length > 2)
            {
                return data[0] == _STXCode;
            }

            return false;
        }
    }
}