using NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base.ExpectReading
{
    public class ExpectSingleByteData : ITransExpectData, IDisposable 
    {
        private const string LogChannel = "PAX_IM20_API";

        public string ExpectedDataTag { get; private set; }
        public int ExpectedDataLength => 1;
        /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        private DbLog _log = null;
        private ASCIIEncoding _ascii = new ASCIIEncoding();

        public byte ExpectedData { get; private set; }

        private string _answerUponDataFound = null;
        private bool _answerWithCleanCOMBuffer = false;
        private PayECRComPort _goSerialPort;
        private string _processId = "*";

        public ExpectSingleByteData(byte expectedData, string dataTag, string answerUponDataFound = null, bool answerWithCleanCOMBuffer = true)
        {
            _log = DbLog.GetDbLog();
            
            _answerWithCleanCOMBuffer = answerWithCleanCOMBuffer;
            _answerUponDataFound = string.IsNullOrWhiteSpace(answerUponDataFound) ? null : answerUponDataFound;

            ExpectedData = expectedData;
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

            if (data is byte byt)
                rIsFound = IsFoundIn(byt);

            else if (data is byte[] bytArr)
                rIsFound = IsFoundFirstByteIn(bytArr);

            else if (data is string str)
                rIsFound = IsFoundFirstByteIn(str);

            if (rIsFound)
            {
                _log?.LogText(LogChannel, _processId, $@"Received-Process :<{ExpectedDataTag}>", "A10", "ExpectSingleByteData.IsFound");

                if ((string.IsNullOrWhiteSpace(_answerUponDataFound) == false) && (_goSerialPort != null))
                {
                    try
                    {
                        if ((_answerWithCleanCOMBuffer) && (_goSerialPort.BytesToRead > 0))
                            throw new Exception($@"Unexpected data; Data Length: {_goSerialPort.BytesToRead}");

                        _goSerialPort.WritePort2(_answerUponDataFound, $@"##{ExpectedDataTag}");
                    }
                    catch (Exception ex)
                    {
                        _log?.LogError(LogChannel, _processId, ex, $@"EX01:{ExpectedDataTag}", "ExpectSingleByteData.IsFound");
                    }
                }
            }
            

            return rIsFound;
        }

        private bool IsFoundIn(byte data)
        {
            return ExpectedData == data;
        }

        private bool IsFoundFirstByteIn(byte[] data)
        {
            bool retVal = false;

            if (data?.Length > 0)
                retVal = ExpectedData == data[0];

            return retVal;
        }

        private bool IsFoundFirstByteIn(string dataStr)
        {
            if (dataStr == null)
                return false;

            bool retVal = false;

            byte[] data = _ascii.GetBytes(dataStr);

            if (data?.Length > 0)
                retVal = ExpectedData == data[0];

            return retVal;
        }

        
    }
}