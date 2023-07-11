using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NssIT.Kiosk.Device.PosiFlex.StatusIndicator1.OrgAPI
{
    public class StatusTowerIndicator : IDisposable 
    {
        //---------------------------------------------------------------
        private const string LogChannel = "Status_Indicator_API";
        private DbLog _log = null;

        private SerialPort _comPort = null;
        private string _initErrorMessage = null;

        // Light String Code --------------------------------------------
        // Public colorSI_Red As String    = Chr(&H1D) + "LDCO"   + Chr(&H13) + Chr(&H0)  + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0)
        // Public colorSI_Orange As String = Chr(&H1D) + "LDCO"   + Chr(&H10) + Chr(&H8)  + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0)
        // Public colorSI_Green As String  = Chr(&H1D) + "LDCO"   + Chr(&H0)  + Chr(&H13) + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0)
        // Public colorSI_STOP As String   = Chr(&H1D) + "LDSTOP" + Chr(&H0)  + Chr(&H0)  + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0) + Chr(&H0)

        public static string Color_Red = $@"{0x1D}LDCO{0x13}{0x00}{0x00}{0x00}{0x00}{0x00}{0x00}{0x00}{0x00}{0x00}{0x00}";
        public static byte[] ColorBytes_Red     = new byte[16] {0x1D, Convert.ToByte('L'), Convert.ToByte('D'), Convert.ToByte('C'), Convert.ToByte('O'), 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
        public static byte[] ColorBytes_Green   = new byte[16] {0x1D, Convert.ToByte('L'), Convert.ToByte('D'), Convert.ToByte('C'), Convert.ToByte('O'), 0x00, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
        public static byte[] ColorBytes_Blue    = new byte[16] {0x1D, Convert.ToByte('L'), Convert.ToByte('D'), Convert.ToByte('C'), Convert.ToByte('O'), 0x00, 0x00, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

        public static byte[] ColorBytes_Aqua    = new byte[16] {0x1D, Convert.ToByte('L'), Convert.ToByte('D'), Convert.ToByte('C'), Convert.ToByte('O'), 0x00, 0x13, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
        public static byte[] ColorBytes_Yellow  = new byte[16] {0x1D, Convert.ToByte('L'), Convert.ToByte('D'), Convert.ToByte('C'), Convert.ToByte('O'), 0x13, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
        public static byte[] ColorBytes_Magenta = new byte[16] {0x1D, Convert.ToByte('L'), Convert.ToByte('D'), Convert.ToByte('C'), Convert.ToByte('O'), 0x13, 0x00, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

        public static byte[] ColorBytes_White   = new byte[16] {0x1D, Convert.ToByte('L'), Convert.ToByte('D'), Convert.ToByte('C'), Convert.ToByte('O'), 0x13, 0x13, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
        public static byte[] ColorBytes_Stop    = new byte[16] {0x1D, Convert.ToByte('L'), Convert.ToByte('D'), Convert.ToByte('S'), Convert.ToByte('T'), Convert.ToByte('O'), Convert.ToByte('P'), 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

        public static byte ColorByte_Blink = 0x02;

        // COM Setting --------------------------------------------------
        private string _comPortName = "COM1";
        private int _comBaudRate = 115200;
        private Parity _comParity = Parity.None;
        private int _comDataBits = 8;
        private StopBits _comStopBits = StopBits.One;
        private bool _comDiscardNull = true;
        private int _comReceivedBytesThreshold = 16;
        private Handshake _comHandshake = Handshake.None;
        private int _comReadTimeout = 3000;
        private int _comWriteTimeout = 3000;
        //---------------------------------------------------------------

        public StatusTowerIndicator(string serialPortName)
        {
            if (serialPortName?.Trim().Length > 0)
            {
                _comPortName = serialPortName.Trim().ToUpper();

                string[] portList = SerialPort.GetPortNames();
                var portQuery = (from port in portList
                           where port?.Trim().ToUpper().Equals(_comPortName) == true
                           select port).ToArray();

                if (portQuery?.Length == 1)
                { /* Port Found; By Pass */ }
                else
                    throw new Exception("Serial Port Name of Status-Indicator has not found");
            }
            else
                throw new Exception("Invalid Serial Port Name of Status-Indicator");
        }

        private DbLog Log
        {
            get
            {
                return _log ?? (_log = DbLog.GetDbLog());
            }
        }

        private SerialPort COMPort
        {
            get
            {
                try
                {
                    if ((_comPort is null) && (_initErrorMessage is null))
                    {
                        _comPort = new SerialPort(_comPortName);
                        _comPort.BaudRate = _comBaudRate;
                        _comPort.Parity = _comParity;
                        _comPort.DataBits = _comDataBits;
                        _comPort.StopBits = _comStopBits;
                        _comPort.Handshake = _comHandshake;
                        _comPort.DiscardNull = _comDiscardNull;
                        _comPort.ReceivedBytesThreshold = _comReceivedBytesThreshold;
                        _comPort.ReadTimeout = _comReadTimeout;
                        _comPort.WriteTimeout = _comWriteTimeout;
                        _comPort.DataReceived += _comPort_DataReceived;

                        _comPort.Open();
                        Thread.Sleep(300);

                        try
                        {
                            _comPort.DiscardOutBuffer();
                        }
                        catch { }
                        Thread.Sleep(300);

                        try
                        {
                            _comPort.DiscardInBuffer();
                        }
                        catch { }
                        Thread.Sleep(300);

                        if (_comPort.IsOpen == false)
                            throw new Exception("Unable to open Serial Port of Status-Indicator");
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, "*", ex, "EX01", "StatusIndicator.COMPort");
                    if (_initErrorMessage is null)
                    {
                        _initErrorMessage = ex.Message;
                    }
                }

                return _comPort;
            }
        }

        private void _comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string msg = "_comPort_DataReceived";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arr16ByteColor">Refer to StatusTowerIndicator.ColorBytes_.. </param>
        public void ShowLight(byte[] arr16ByteColor)
        {
            SerialPort comPort = COMPort;

            if ((comPort != null) && (_initErrorMessage is null))
            {
                //string stringCode = Encoding.ASCII.GetString(ColorBytes_Red);
                string stringCode = Encoding.ASCII.GetString(arr16ByteColor);
                comPort.Write(stringCode);
                Thread.Sleep(300);
            }
            else
            {
                if (_initErrorMessage != null)
                    throw new Exception(_initErrorMessage);
                else
                    throw new Exception("Fail to open Serial Port of Status-Indicator");
            }
        }

        public void SwitchOffLight()
        {
            SerialPort comPort = COMPort;

            if ((comPort != null) && (_initErrorMessage is null))
            {
                try
                {
                    ShowLight(ColorBytes_Stop);
                }
                catch { }
            }
        }

        public void CleanUp()
        {
            try
            {
                _comPort.DiscardOutBuffer();
            }
            catch { }
            Thread.Sleep(300);

            try
            {
                _comPort.DiscardInBuffer();
            }
            catch { }
            Thread.Sleep(300);
        }

        public void Dispose()
        {
            if (_comPort != null)
            {
                if (_comPort.IsOpen == true)
                {
                    try
                    {
                        try
                        {
                            ShowLight(ColorBytes_Stop);
                        }
                        catch { }

                        try
                        {
                            _comPort.DiscardOutBuffer();
                        }
                        catch { }
                        Thread.Sleep(300);

                        try
                        {
                            _comPort.DiscardInBuffer();
                        }
                        catch { }
                        Thread.Sleep(300);

                        _comPort.DataReceived -= _comPort_DataReceived;

                        try
                        {
                            _comPort.Close();
                        }
                        catch { }

                        Log.LogText(LogChannel, "*", "Close Status Indicator COM port", "E100", "StatusIndicator.Dispose");

                        Thread.Sleep(300);
                    }
                    catch { }
                }

                try
                {
                    _comPort.Dispose();
                    Thread.Sleep(100);
                }
                catch { }

                _comPort = null;
            }
        }
    }
}
