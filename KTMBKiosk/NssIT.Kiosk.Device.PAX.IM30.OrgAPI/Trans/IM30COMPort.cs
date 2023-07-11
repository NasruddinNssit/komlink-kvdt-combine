using NssIT.Kiosk.AppDecorator.DomainLibs.IM30;
using NssIT.Kiosk.AppDecorator.Log;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans
{
	/// <summary>
	/// This class used as COM port wrapper. The main purpose is to keep incomming data into a collection list. This allow caller to read data pack by pack.
	/// </summary>
	public class IM30COMPort : IDisposable
	{
		public delegate void OnDataReceivedNoteDelg();

		private const string _logChannel = "IM30_API";

		//private DbLog _log = null;

		private const int _defaultBufferSize = 1024;
		private const int _defaultReadTimeout = 1000 * 60 * 5;
		private const int _defaultWriteTimeout = 1000 * 60 * 2;
		private SerialPort _comPort = null;

		private byte[] _currDataPack = null;

        private DbLog _log = null;
        private ConcurrentQueue<byte[]> _dataList = new ConcurrentQueue<byte[]>();

		public string ComPortStr { get; private set; } = "COM0";
		private string _currProcessId = "-";

		private bool _isReadyToWork = false;
		private DataPack _dataPack = new DataPack();
		private Guid _latestNetProcessId = Guid.Empty;

		private OnDataReceivedNoteDelg _onDataReceivedNotificationHandle = null;


		//private const int _baudRate = 9600;
		private const int _baudRate = 115200;
		private const int _databits = 8;

		private static Parity _parity = Parity.None;
		private static StopBits _stopbits = StopBits.One;

		private ShowMessageLogDelg _logStateHandle = null;

        public DbLog Log
        {
            get
            {
                return _log ?? (_log = DbLog.GetDbLog());
            }
        }

        public IM30COMPort(string comPortString, string processId, OnDataReceivedNoteDelg onDataReceivedNoteHandle, ShowMessageLogDelg logStateHandle)
			: this(comPortString, _baudRate, _parity, _databits, _stopbits, processId, onDataReceivedNoteHandle, logStateHandle)
		{ }

		private IM30COMPort(string comPortString, int baudRate, Parity parity, int dataBits, StopBits stopBit, string processId,
			OnDataReceivedNoteDelg onDataReceivedNoteHandle, ShowMessageLogDelg logStateHandle)
		{
			_comPort = new SerialPort(comPortString, baudRate, parity, dataBits, stopBit);

			ComPortStr = (comPortString ?? "").Trim();
			ComPortStr = (ComPortStr.Length == 0) ? "COM0" : ComPortStr;
			_currProcessId = (processId ?? "").Trim();
			_currProcessId = (_currProcessId.Length == 0) ? "-" : _currProcessId;
			_onDataReceivedNotificationHandle = onDataReceivedNoteHandle;
			_logStateHandle = logStateHandle;

			_dataPack.CurrProcessId = _currProcessId;

			_comPort.DataReceived += DataReceivedEventHandle;
			_comPort.ErrorReceived += ErrorReceivedEventHandle;
		}

		public void SetProcessId(string processId)
		{
            _currProcessId = (processId ?? "").Trim();
            _currProcessId = (_currProcessId.Length == 0) ? "-" : _currProcessId;
        }

		public bool PortIsOpen
		{ get { return _comPort.IsOpen; } }

		public int BytesToRead
		{
			get
			{
				if ((_currDataPack == null) || (_currDataPack.Length == 0))
				{
					while ((_dataList.Count > 0)
							&& ((_currDataPack == null) || (_currDataPack.Length == 0))
							)
					{
						_currDataPack = null;
						_dataList.TryDequeue(out _currDataPack);
					}

					if ((_currDataPack != null) && (_currDataPack.Length > 0))
						return _currDataPack.Length;

					_currDataPack = null;
					return 0;
				}
				else
					return _currDataPack.Length;
			}
		}

		public int ReadPortTimeOut
		{
			get { return _comPort.ReadTimeout; }
			set { _comPort.ReadTimeout = _defaultReadTimeout; }
		}

		public int WritePortTimeOut
		{
			get { return _comPort.WriteTimeout; }
			set { _comPort.WriteTimeout = _defaultWriteTimeout; }
		}


		private static SemaphoreSlim _lockPortOpen = new SemaphoreSlim(1);
		public void OpenPort()
		{
			try
			{
				_lockPortOpen.WaitAsync().Wait();

				Log.LogText(_logChannel, _currProcessId, $@"Try to open {ComPortStr} Port; Thread Hash Code : {Thread.CurrentThread.GetHashCode()};", "A01", "IM30COMPort.OpenPort");
				_comPort.Open();
				Log.LogText(_logChannel, _currProcessId, $@"{ComPortStr} Port has open; Thread Hash Code : {Thread.CurrentThread.GetHashCode()};", "A03", "IM30COMPort.OpenPort");

				// Below sleep is to allow historical rubbish data to be received.
				Thread.Sleep(70);

				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
				// Clear historical "rubbish data"
				CleanUp();
				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

				_isReadyToWork = true;
			}
			finally
			{
				if (_lockPortOpen.CurrentCount == 0)
					_lockPortOpen.Release();
			}
		}

		//_onDataReceivedNoteHandle
		private void TriggerDataReceivedNoteHandle()
        {
			if (_onDataReceivedNotificationHandle != null)
            {
				try
				{
					_onDataReceivedNotificationHandle?.Invoke();
				}
				catch (Exception ex)
				{
					Log.LogError(_logChannel, _currProcessId, new Exception($@"{ex.Message}; COM Port: {ComPortStr}", ex), "EX01", "IM30COMPort.TriggerDataReceivedNoteHandle");
                }
			}
		}

		public void CleanUp()
		{
			// Clear historical "rubbish data"
			string hisDataStr = "";
			while (BytesToRead > 0)
			{
				byte[] buffer = DeQueueData();

				try
				{
					if (buffer.Length > 0)
					{
						Log.LogText(_logChannel, _currProcessId, $@"COM Port: {ComPortStr}; Historical rubbish found. Bytes Count :" + buffer.Length.ToString(), "A05", "IM30COMPort.CleanUp");

						hisDataStr = System.Text.ASCIIEncoding.ASCII.GetString(buffer);

						Log.LogText(_logChannel, _currProcessId, $@"COM Port: {ComPortStr}; Clear Rubbish-Read in Hex :" + (BitConverter.ToString(buffer) ?? ""), "DATA_AS_HEX", "IM30COMPort.CleanUp");
						Log.LogText(_logChannel, _currProcessId, $@"COM Port: {ComPortStr}; Clear Rubbish-Read in Txt :" + IM30COMPort.AsciiOctets2String((System.Text.Encoding.UTF8.GetBytes(hisDataStr.Replace("\0", String.Empty).Trim()))), 
							"DATA_AS_STRING", "IM30COMPort.CleanUp");

						// Below sleep is to allow historical "rubbish data" to be received.
						if (BytesToRead == 0)
							Thread.Sleep(100);
					}
				}
				catch { string byPass = "Don't Care."; }

			}

			_comPort?.DiscardOutBuffer();
			_comPort?.DiscardInBuffer();
			Thread.Sleep(10);

			while (_dataList.TryDequeue(out _) == true)
			{ Thread.Sleep(10); }
			_dataPack.ResetDataPackage();
		}

		private SemaphoreSlim _dataReceivedEventLock = new SemaphoreSlim(1);
		private void DataReceivedEventHandle(object sender, SerialDataReceivedEventArgs e)
		{
			//Guid pId = Guid.NewGuid();

			int dataLen = 0;
			byte[] data = new byte[0];
			string dataStr = "";
			string asciiOctetStr = "";
			//int dataTriggerTimes = 0;
			try
			{
                ShowLogMsg($@"..... COM port data found");
				Log.LogText(_logChannel, _currProcessId,
					$@"COM Port: {ComPortStr}; ..... try to enter DataReceivedEventHandle ....; ----- ThreadId : {Thread.CurrentThread.ManagedThreadId.ToString()}; ThreadHashCode : {Thread.CurrentThread.GetHashCode().ToString()}",
					"A00", "IM30COMPort.DataReceivedEventHandle");

				_dataReceivedEventLock.WaitAsync().Wait();

				Log.LogText(_logChannel, _currProcessId,
					$@"COM Port: {ComPortStr}; Start Read Data;  ----- ThreadId : {Thread.CurrentThread.ManagedThreadId.ToString()}; ThreadHashCode : {Thread.CurrentThread.GetHashCode().ToString()}",
					"A01", "IM30COMPort.DataReceivedEventHandle");

				// Wait for the tail of outstanding data bytes
				//CYA-DEBUG...
				////Thread.Sleep(300);
				//-------------------
				Thread.Sleep(70);
				//===================================================

				dataLen = _comPort.BytesToRead;
				data = new byte[dataLen];
				if (_comPort.Read(data, 0, dataLen) > 0)
				{
					if (data.Length > 1)
                    {
						///// .. ACK(6) or NAK(21)
						if ((data[0] == 6) || (data[0] == 21) && (_dataPack.IsExpectingData == false))
                        {
							byte[] singleByte = new byte[] { data[0] };
							byte[] tmpData = new byte[data.Length - 1];

							Array.Copy(data, 1, tmpData, 0, tmpData.Length);
							data = tmpData;
							dataLen = data.Length;

                            _dataList.Enqueue(singleByte);
							TriggerDataReceivedNoteHandle();
						}
					}

					Log.LogText(_logChannel, _currProcessId, $@"COM Port: {ComPortStr}; End Read Data", "A03", "IM30COMPort.DataReceivedEventHandle");

					dataStr = System.Text.ASCIIEncoding.ASCII.GetString(data);
					Log.LogText(_logChannel, _currProcessId, $@"COM Port: {ComPortStr}; ReadCOM_Event:-Read in Hex :" + (BitConverter.ToString(data) ?? ""),
						"A05:DATA_AS_HEX", "IM30COMPort.DataReceivedEventHandle");

					asciiOctetStr = AsciiOctets2String((System.Text.Encoding.UTF8.GetBytes(dataStr.Replace("\0", String.Empty).Trim())));
					Log.LogText(_logChannel, _currProcessId, $@"COM Port: {ComPortStr}; ReadCOM_Event:-Read in Txt :" + asciiOctetStr,
						"A06:DATA_AS_STRING", "IM30COMPort.DataReceivedEventHandle");

					if (_isReadyToWork)
					{
						// Pack Data
						if (_dataPack.IsDataPackage(data, 30) || _dataPack.IsExpectingData)
						{
							byte[] resData = _dataPack.PackingData(data);

							if (resData != null)
							{
								_dataList.Enqueue(resData);
								TriggerDataReceivedNoteHandle();

								dataStr = System.Text.ASCIIEncoding.ASCII.GetString(resData);
								Log.LogText(_logChannel, _currProcessId, $@"COM Port: {ComPortStr}; ReadCOM_Event:-Received-Process Data Pack in Hex :" + (BitConverter.ToString(resData) ?? ""),
									"A09:DATA_AS_HEX", "IM30COMPort.DataReceivedEventHandle");
								Log.LogText(_logChannel, _currProcessId, $@"COM Port: {ComPortStr}; ReadCOM_Event:-Received-Process Data Pack in Txt :" + IM30COMPort.AsciiOctets2String((System.Text.Encoding.UTF8.GetBytes(dataStr.Replace("\0", String.Empty).Trim()))),
									"A10:DATA_AS_STRING", "IM30COMPort.DataReceivedEventHandle");
							}
						}

						// Expecting Single Byte ..... 
						else if (data.Length > 0)
						{
							if (data.Length == 1)
                            {
								_dataList.Enqueue(data);
								TriggerDataReceivedNoteHandle();
							}
							else
                            {
                                string dataStrX = System.Text.ASCIIEncoding.ASCII.GetString(data);
                                Log.LogText(_logChannel, _currProcessId, $@"COM Port: {ComPortStr};!!Unknown Data!!; ReadCOM_Event:-Received-Process Data Pack in Hex :" + (BitConverter.ToString(data) ?? ""),
                                    "A19:DATA_AS_HEX", "IM30COMPort.DataReceivedEventHandle", AppDecorator.Log.MessageType.Error);
                                Log.LogText(_logChannel, _currProcessId, $@"COM Port: {ComPortStr};!!Unknown Data!!; ReadCOM_Event:-Received-Process Data Pack in Txt :" + IM30COMPort.AsciiOctets2String((System.Text.Encoding.UTF8.GetBytes(dataStrX.Replace("\0", String.Empty).Trim()))),
                                    "A20:DATA_AS_STRING", "IM30COMPort.DataReceivedEventHandle", AppDecorator.Log.MessageType.Error);
                            }
						}
					}
					else
					{
						// Collect "rubbish data" if system not ready.
						_dataList.Enqueue(data);
					}
				}
				else
				{
					Log.LogText(_logChannel, _currProcessId, $@"COM Port: {ComPortStr}; End Read with no data found", "A13", "IM30COMPort.DataReceivedEventHandle", AppDecorator.Log.MessageType.Error);
				}

                ShowLogMsg("..... COM port ending data reading");
                Log.LogText(_logChannel, _currProcessId, $@"COM Port: {ComPortStr}; ..... COM port ending data reading", "A15", "IM30COMPort.DataReceivedEventHandle");
            }
			catch (Exception ex)
			{
				Log.LogFatal(_logChannel, _currProcessId, new Exception($@"{ex.Message}; COM Port: {ComPortStr}", ex), "EX01", "IM30COMPort.DataReceivedEventHandle");
			}
			finally
			{
                ShowLogMsg($@"..... COM port data End receiving");

				if (_dataReceivedEventLock.CurrentCount == 0)
					_dataReceivedEventLock.Release();

				Log.LogText(_logChannel, _currProcessId, $@"COM Port: {ComPortStr}; ..... End of DataReceivedEventHandle.. Quit Lock ....", "B100",
						"IM30COMPort.DataReceivedEventHandle");
			}
		}

		private void ShowLogMsg(string msg)
		{
			_logStateHandle?.Invoke(msg);
        }

		private void ErrorReceivedEventHandle(object sender, SerialErrorReceivedEventArgs e)
		{
			Log.LogText(_logChannel, _currProcessId, new WithDataException($@"COM Port: {ComPortStr}; COM Port Error :{e.ToString()}", new Exception("Error when receiving COM Port Data"), e), "EX01",
				"IM30COMPort.ErrorReceivedEventHandle", AppDecorator.Log.MessageType.Error);
		}

		private byte[] DeQueueData()
		{
			byte[] dataBytes = null;
			if (BytesToRead > 0)
			{
				dataBytes = _currDataPack;
				_currDataPack = null;
				return dataBytes;
			}
			return new byte[0];
		}

		public byte[] ReadPort(int timeOutInSeconds = 10)
		{
			int maxWaitMilliSec = timeOutInSeconds * 1000;
			byte[] dataBytes = null;

			DateTime timeOut = DateTime.Now.AddMilliseconds(maxWaitMilliSec);

			while ((timeOut.Subtract(DateTime.Now).TotalSeconds >= 0))
			{
				if (BytesToRead > 0)
				{
					dataBytes = DeQueueData();
					if (dataBytes.Length > 0)
					{
                        ShowLogMsg($@"COM Port Read (Hex):{BitConverter.ToString(dataBytes)}");
						return dataBytes;
					}
				}
				Thread.Sleep(20);
			}
			throw new SystemTimeOutException("System Timeout at section-RDT_5A. Fail to receive from I/O.");
		}

		public void WriteDataPort(byte[] writedata, string tag)
        {
			if (writedata?.Length > 0)
            {
                ShowLogMsg($@"COM Port Write (Hex):{BitConverter.ToString(writedata)}; Tag: {tag}");

                Log.LogText(_logChannel, _currProcessId, $@"COM Port: {ComPortStr}; WritePort(Hex.):" + (BitConverter.ToString(writedata) ?? ""), $@"A01:{tag}", "IM30COMPort.WriteDataPort");
                Log.LogText(_logChannel, _currProcessId, $@"COM Port: {ComPortStr}; WritePort(Text):" + AsciiOctets2String(writedata), $@"A02:{tag}", "IM30COMPort.WriteDataPort");
				_comPort.Write(writedata, 0, writedata.Length);
				// Apply Write Latency Time
				Thread.Sleep(10);
				// ----------------------------
				Log.LogText(_logChannel, _currProcessId, $@"COM Port: {ComPortStr}; WritePort:-End", $@"A10:{tag}", "IM30COMPort.WriteDataPort");
			}
			else
			{
                Log.LogText(_logChannel, _currProcessId, $@"COM Port: {ComPortStr}; !!Bytes length is 0!!", $@"A10:{tag}", "IM30COMPort.WriteDataPort", MessageType.Error);
            }
		}

		//public void WritePort(string writedata)
		//{
		//	//Log.LogText(LogChannel, _currProcessId, "WritePort:-Start :" + AsciiOctets2String(System.Text.Encoding.UTF8.GetBytes(writedata)), "A01", "IM30COMPort.WritePort");
		//	_comPort.Write(writedata);
		//	// Apply Write Latency Time
		//	Thread.Sleep(50);
		//	// ----------------------------
		//	//Log.LogText(LogChannel, _currProcessId, "WritePort:-End", "A10", "IM30COMPort.WritePort");
		//}

		//public void WritePort(string writedata, string tag)
		//{
		//	string tt1 = AsciiOctets2String(System.Text.Encoding.UTF8.GetBytes(writedata));
		//	//Log.LogText(LogChannel, _currProcessId, $@"#{tag};WritePort:-Start :" + AsciiOctets2String(System.Text.Encoding.UTF8.GetBytes(writedata)), "A01", "IM30COMPort.WritePort");
		//	_comPort.Write(writedata);
		//	// Apply Write Latency Time
		//	Thread.Sleep(10);
		//	// ----------------------------
		//	//Log.LogText(LogChannel, _currProcessId, $@"#{tag};WritePort:-End", "A10", "IM30COMPort.WritePort");
		//}

		//public int ReadPortChar()
		//{
		//	byte[] buffer = ReadPort(3);
		//	if (buffer.Length > 0)
		//	{
		//		return buffer[0];
		//	}
		//	return 0;
		//}

		public void ClosePort()
        {
            try
            {
				_comPort.DiscardOutBuffer();
				_comPort.DiscardInBuffer();
				Thread.Sleep(10);

				_comPort.Close();
			}
			catch(Exception ex)
            {
				Log.LogError(_logChannel, _currProcessId, new Exception($@"COM Port: {ComPortStr}; {ex.Message}", ex), "EX01", "IM30COMPort.ClosePort");
			}
		}

		private bool _isDisposed = false;
		public void Dispose()
		{
			if (_comPort != null)
			{
				try
				{
					try
					{
						if (_comPort != null)
						{
							_comPort.DataReceived -= DataReceivedEventHandle;
							_comPort.ErrorReceived -= ErrorReceivedEventHandle;

							if (_onDataReceivedNotificationHandle != null)
								_onDataReceivedNotificationHandle = null;
							
							ClosePort();

							Log.LogText(_logChannel, _currProcessId, $@"COM Port: {ComPortStr}; COM Port Closed", "A03", "IM30COMPort.Dispose");
						}
						else
						{
							Log.LogText(_logChannel, _currProcessId, $@"COM Port: {ComPortStr}; Error : Serial Port Instant not created.", "EX01", "IM30COMPort.Dispose", AppDecorator.Log.MessageType.Error);
						}
					}
					catch (Exception ex)
					{
						Log.LogError(_logChannel, _currProcessId, new Exception($@"COM Port: {ComPortStr}; Error when closing COM Port", ex), "EX03", "IM30COMPort.Dispose");
					}

					try
					{
						if (_comPort != null)
						{
							_comPort.Dispose();
							Log.LogText(_logChannel, _currProcessId, $@"COM Port: {ComPortStr}; COM Port Disposed", "A05", "IM30COMPort.Dispose");
						}
					}
					catch (Exception ex)
					{
						Log.LogError(_logChannel, _currProcessId, new Exception($@"COM Port: {ComPortStr}; Disposing COM Port", ex), "EX05", "IM30COMPort.Dispose");
					}
				}
				catch { }

				_comPort = null;
				_logStateHandle = null;
				_onDataReceivedNotificationHandle = null;
				_isDisposed = true;
            }
		}

		public static string AsciiOctets2String(byte[] bytes)
		{
			try
			{
				StringBuilder sb = new StringBuilder(bytes.Length);
				foreach (char c in System.Text.Encoding.UTF8.GetString(bytes).ToCharArray())
				{
					switch (c)
					{
						case '\u0000': sb.Append("<NUL>"); break;
						case '\u0001': sb.Append("<SOH>"); break;
						case '\u0002': sb.Append("<STX>"); break;
						case '\u0003': sb.Append("<ETX>"); break;
						case '\u0004': sb.Append("<EOT>"); break;
						case '\u0005': sb.Append("<ENQ>"); break;
						case '\u0006': sb.Append("<ACK>"); break;
						case '\u0007': sb.Append("<BEL>"); break;
						case '\u0008': sb.Append("<BS>"); break;
						case '\u0009': sb.Append("<HT>"); break;
						case '\u000A': sb.Append("<LF>"); break;
						case '\u000B': sb.Append("<VT>"); break;
						case '\u000C': sb.Append("<FF>"); break;
						case '\u000D': sb.Append("<CR>"); break;
						case '\u000E': sb.Append("<SO>"); break;
						case '\u000F': sb.Append("<SI>"); break;
						case '\u0010': sb.Append("<DLE>"); break;
						case '\u0011': sb.Append("<DC1>"); break;
						case '\u0012': sb.Append("<DC2>"); break;
						case '\u0013': sb.Append("<DC3>"); break;
						case '\u0014': sb.Append("<DC4>"); break;
						case '\u0015': sb.Append("<NAK>"); break;
						case '\u0016': sb.Append("<SYN>"); break;
						case '\u0017': sb.Append("<ETB>"); break;
						case '\u0018': sb.Append("<CAN>"); break;
						case '\u0019': sb.Append("<EM>"); break;
						case '\u001A': sb.Append("<SUB>"); break;
						case '\u001B': sb.Append("<ESC>"); break;
						case '\u001C': sb.Append("<FS>"); break;
						case '\u001D': sb.Append("<GS>"); break;
						case '\u001E': sb.Append("<RS>"); break;
						case '\u001F': sb.Append("<US>"); break;
						case '\u007F': sb.Append("<DEL>"); break;
						default:
							if (c > '\u007F')
							{
								sb.AppendFormat(@"\u{0:X4}", (ushort)c); // in ASCII, any octet in the range 0x80-0xFF doesn't have a character glyph associated with it
							}
							else
							{
								sb.Append(c);
							}
							break;
					}
				}
				return sb.ToString();

			}
			catch (Exception e)
			{
				return "";
			}
		}

		/// <summary>
		/// Use class to calibrate multiper bytes array into one package data.
		/// Note-1 : A complete package data will start with [STX] , end with [ETX] then finally with a byte of {LRC} ; [STX] ..... [ETX]{LRC} ;
		/// Note-2 : When a STX found, use SetTimeout(..) to set the working timeout to limit the time waiting for data packing.
		/// </summary>
		class DataPack : IDisposable
		{
			private const byte _STXCode = 2;
			private const byte _ETXCode = 3;

			private const string LogChannel = "PAX_IM30_API";
			private DateTime _dataPackingTimeout = DateTime.Now;

			private Queue<byte> _currentDataPack = new Queue<byte>();

			// _STXFound always initiate to false at the beginning a data packing.
			private bool _STXFound = false;
			// _ETXFound is set to false when still not found a ETX byte in data packing.
			private bool _ETXFound = false;

			private bool _disposed = false;

			private int _im30DataTotalLength = 0;
			private byte[] _im30DataLLLLData = new byte[2] { 0, 0};

			public DataPack()
			{ }

			public string CurrProcessId { get; set; } = "-";

			//private DbLog _log = null;
			//public DbLog Log
			//{
			//	get
			//	{
			//		return _log ?? (_log = DbLog.GetDbLog());
			//	}
			//}

			/// <summary>
			/// Need to set timeout if STX found.
			/// </summary>
			/// <param name="toleranceDelaySec">Default to 30 seconds</param>
			private void SetTimeout(int toleranceDelaySec = 30)
			{
				if (_disposed) return;

				_dataPackingTimeout = DateTime.Now.AddSeconds(toleranceDelaySec);
			}

			/// <summary>
			/// Check if a byte array expected for packing a data package.
			/// </summary>
			public bool IsExpectingData
			{
				get
				{
					if (_disposed) return false;

					return (_currentDataPack.Count > 0);
				}
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="checkData"></param>
			/// <param name="stxPostInx">Output an integer index about position of STX in checkData array. If STX not found, output a null value</param>
			/// <param name="dataCollectionToleranceDelaySec">Used to set Timeout waiting for data pack; Default to 30 seconds</param>
			/// <returns></returns>
			public bool IsDataPackage(byte[] checkData, int dataCollectionToleranceDelaySec = 30)
			{
				int? stxPostInx = null;

				if ((_disposed) || (checkData == null) || (checkData.Length == 0))
					return false;

				for (int inx = 0; inx < checkData.Length; inx++)
				{
					byte dtByte = checkData[inx];

					if (dtByte == _STXCode)
					{
						stxPostInx = inx;
						break;
					}

					// Note : when inx more than 16 should not have any STX
					if (inx > 16)
						break;
				}

				bool ret = stxPostInx.HasValue;

				if (ret)
					SetTimeout(dataCollectionToleranceDelaySec);

				return ret;

			}

			/// <summary></summary>
			/// <param name="data"></param>
			/// <param name="stxInxPosition">Position of STX.</param>
			/// <returns>
			/// Array of byte will be returned if finished packing data. Else NUll will be returned.
			/// </returns>
			public byte[] PackingData(byte[] data)
			{
				if (_disposed)
					return null;

				if ((data == null) || (data.Length == 0))
					return null;

				bool isFinishPacking = false;
				//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
				//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
				foreach (byte dtByte in data)
				{
					if (_STXFound == false)
					{
						if (dtByte == _STXCode)
						{
							// Append STX
							_currentDataPack.Enqueue(dtByte);
							_STXFound = true;
						}
					}
					else if (_ETXFound == false)
					{
						if ((dtByte == _ETXCode) && (_im30DataTotalLength > 0) && ((_im30DataTotalLength - 2) == _currentDataPack.Count))
						{
							// Append ETX
							_currentDataPack.Enqueue(dtByte);
							_ETXFound = true;
						}
						else
						{
							// Append Data
							_currentDataPack.Enqueue(dtByte);
						}
					}
					else
					{
						// Append LRC / Last Byte
						_currentDataPack.Enqueue(dtByte);
						isFinishPacking = true;
						break;
					}

					if (_im30DataTotalLength == 0)
                    {
						if (_currentDataPack.Count == 2)
                        {
							_im30DataLLLLData[0] = dtByte;
						}
						else if (_currentDataPack.Count == 3)
						{
							_im30DataLLLLData[1] = dtByte;

							string msgDataHexLenLLLL = BitConverter.ToString(_im30DataLLLLData).Replace("-", "");

							if (int.TryParse(msgDataHexLenLLLL, out int LLLL))
                            {
								///// .. _im30DataTotalLength = LLLL + 1 (for STX) + 2 (for LLLL) + 1 (for ETX) + 1 (LRC)
								_im30DataTotalLength = LLLL + 5;
							}
							else
                            {
								_im30DataTotalLength = 9;
								throw new Exception($@"Invalid IM30 LLLL value; LLLL in Hex.String:{msgDataHexLenLLLL}");
							}

							///// .. (msgDataHexLenLLLL(to int) + 1 (for STX) + 2 (for LLLL) + 1 (for ETX) + 1 (LRC))
						}
					}
				}

				//-----------------------------------------------------------------------
				// Timeout Return OR Finish packing Return 
				bool isTimeOut = (_dataPackingTimeout.Subtract(DateTime.Now).TotalSeconds <= 0);

				if ((isTimeOut)
					|| (isFinishPacking))
				{
					//if (isTimeOut)
					//	Log.LogError(_logChannel, CurrProcessId, new Exception("Data Packaging Timeout"), "X01", "DataPack.PackingData");

					byte[] retData = _currentDataPack.ToArray();

					// Reset data and flag after finish packing
					ResetDataPackage();
					//----------------------

					return retData;
				}

				// Pending for data packing return
				else
					return null;
			}

			public void ResetDataPackage()
			{
				_currentDataPack.Clear();
				_STXFound = false;
				_ETXFound = false;
				_im30DataTotalLength = 0;
				_im30DataLLLLData[0] = 0;
				_im30DataLLLLData[1] = 0;
			}

			public void Dispose()
			{
				_disposed = true;
				_currentDataPack?.Clear();
			}
		}
	}
}