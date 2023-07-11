using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using NssIT.Kiosk.Log.DB;
using System.Collections.Generic;

namespace NssIT.Kiosk.Device.PAX.IM20.OrgAPI
{
	/// <summary>
	/// This class used as COM port wrapper. The main purpose is to keep incomming data into a collection list. This allow caller to read data pack by pack.
	/// </summary>
	public class PayECRComPort : IDisposable
	{
		private const string LogChannel = "PAX_IM20_API";

		private DbLog _log = null;

		private const int _defaultBufferSize = 1024;
		private const int _defaultReadTimeout = 1000 * 60 * 5;
		private const int _defaultWriteTimeout = 1000 * 60 * 2;
		private SerialPort _comPort = null;

		private byte[] _currDataPack = null;
		private ConcurrentQueue<byte[]> _dataList = new ConcurrentQueue<byte[]>();

		//private string _logPath = @"C:\eTicketing_Log\ECR_LOG";
		private string _comPortStr = null;
		private string _currProcessId = "-";

		private bool _isReadyToWork = false;
		private DataPack _dataPack = new DataPack();
		private Guid _latestNetProcessId = Guid.Empty;

		public DbLog Log
		{
			get
			{
				return _log ?? (_log = DbLog.GetDbLog());
			}
		}

		private static PayECRComPort _payECRComPort = null;
		public static PayECRComPort GetPayECRComPort(string comPortString, int baudRate, Parity parity, int dataBits, StopBits stopBit, string processId)
        {
			if (_payECRComPort == null)
				_payECRComPort = new PayECRComPort(comPortString, baudRate, parity, dataBits, stopBit, processId);
			else
			{
				_payECRComPort._currProcessId = processId;
				_payECRComPort._dataPack.CurrProcessId = processId;
			}

			return _payECRComPort;
		}

		private PayECRComPort(string comPortString, int baudRate, Parity parity, int dataBits, StopBits stopBit, string processId) 
		{
			_comPort = new SerialPort(comPortString, baudRate, parity, dataBits, stopBit);

			_comPortStr = (comPortString ?? "").Trim();
			_comPortStr = (_comPortStr.Length == 0) ? "COM0" : _comPortStr;
			_currProcessId = (processId ?? "").Trim();
			_currProcessId = (_currProcessId.Length == 0) ? "-" : _currProcessId;

			_dataPack.CurrProcessId = _currProcessId;
			//_logPath = log_path;

			_comPort.DataReceived += DataReceivedEventHandle;
			_comPort.ErrorReceived += ErrorReceivedEventHandle;
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

				Log.LogText(LogChannel, _currProcessId, $@"Try to open {_comPortStr} Port; Thread Hash Code : {Thread.CurrentThread.GetHashCode()};", "A01", "PayECRComPort.OpenPort");
				_comPort.Open();
				Log.LogText(LogChannel, _currProcessId, $@"{_comPortStr} Port has open; Thread Hash Code : {Thread.CurrentThread.GetHashCode()};", "A03", "PayECRComPort.OpenPort");

				// Below sleep is to allow historical rubbish data to be received.
				Thread.Sleep(1000);

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
						Log.LogText(LogChannel, _currProcessId, "Historical rubbish found. Bytes Count :" + buffer.Length.ToString(), "A05",
							"PayECRComPort.CleanUp");

						hisDataStr = System.Text.ASCIIEncoding.ASCII.GetString(buffer);
						Log.LogText(LogChannel, _currProcessId, "Clear Rubbish-Read in Hex :" + (BitConverter.ToString(buffer, 0, buffer.Length) ?? ""), "DATA_AS_HEX",
							"PayECRComPort.CleanUp");
						Log.LogText(LogChannel, _currProcessId, "Clear Rubbish-Read in Txt :" + PayECRComPort.AsciiOctets2String((System.Text.Encoding.UTF8.GetBytes(hisDataStr.Replace("\0", String.Empty).Trim()))), "DATA_AS_STRING",
							"PayECRComPort.CleanUp");

						// Below sleep is to allow historical "rubbish data" to be received.
						if (BytesToRead == 0)
							Thread.Sleep(500);
					}
				}
				catch { string byPass = "Don't Care."; }

			}

			_comPort?.DiscardOutBuffer();
			_comPort?.DiscardInBuffer();
			Thread.Sleep(100);

			while (_dataList.TryDequeue(out _) == true)
			{ Thread.Sleep(100); }
			_dataPack.ResetDataPackage();
		}

		private SemaphoreSlim _dataReceivedEventLock = new SemaphoreSlim(1);
		private void DataReceivedEventHandle(object sender, SerialDataReceivedEventArgs e)
		{
			int dataLen = 0;
			byte[] data = new byte[0];
			string dataStr = "";
			string asciiOctetStr = "";

			try
			{
				Log.LogText(LogChannel, _currProcessId,
					$@"..... try to enter DataReceivedEventHandle ....; ----- ThreadId : {Thread.CurrentThread.ManagedThreadId.ToString()}; ThreadHashCode : {Thread.CurrentThread.GetHashCode().ToString()}", 
					"A00", "PayECRComPort.DataReceivedEventHandle");

				_dataReceivedEventLock.WaitAsync().Wait();

				Log.LogText(LogChannel, _currProcessId, 
					$@"Start Read Data;  ----- ThreadId : {Thread.CurrentThread.ManagedThreadId.ToString()}; ThreadHashCode : {Thread.CurrentThread.GetHashCode().ToString()}", 
					"A01", "PayECRComPort.DataReceivedEventHandle");

				// Wait for the tail of outstanding data bytes
				Thread.Sleep(300);
				//===================================================

				dataLen = _comPort.BytesToRead;
				data = new byte[dataLen];
				if (_comPort.Read(data, 0, dataLen) > 0)
				{
					Log.LogText(LogChannel, _currProcessId, "End Read Data", "A03",	"PayECRComPort.DataReceivedEventHandle");

					dataStr = System.Text.ASCIIEncoding.ASCII.GetString(data);
					Log.LogText(LogChannel, _currProcessId, "ReadCOM_Event:-Read in Hex :" + (BitConverter.ToString(data, 0, dataLen) ?? ""), 
						"A05:DATA_AS_HEX", "PayECRComPort.DataReceivedEventHandle");

					asciiOctetStr = PayECRComPort.AsciiOctets2String((System.Text.Encoding.UTF8.GetBytes(dataStr.Replace("\0", String.Empty).Trim())));
					Log.LogText(LogChannel, _currProcessId, "ReadCOM_Event:-Read in Txt :" + asciiOctetStr, 
						"A06:DATA_AS_STRING", "PayECRComPort.DataReceivedEventHandle");

					if (_isReadyToWork)
					{
						// Pack Data
						if (_dataPack.IsDataPackage(data, 30) || _dataPack.IsExpectingData)
						{
							byte[] resData = _dataPack.PackingData(data);

							if (resData != null)
							{
								_dataList.Enqueue(resData);

								dataStr = System.Text.ASCIIEncoding.ASCII.GetString(resData);
								Log.LogText(LogChannel, _currProcessId, "ReadCOM_Event:-Received-Process Data Pack in Hex :" + (BitConverter.ToString(resData, 0, resData.Length) ?? ""), 
									"A09:DATA_AS_HEX", "PayECRComPort.DataReceivedEventHandle");
								Log.LogText(LogChannel, _currProcessId, "ReadCOM_Event:-Received-Process Data Pack in Txt :" + PayECRComPort.AsciiOctets2String((System.Text.Encoding.UTF8.GetBytes(dataStr.Replace("\0", String.Empty).Trim()))), 
									"A10:DATA_AS_STRING", "PayECRComPort.DataReceivedEventHandle");
							}
						}
						// Normal Reading
						else
							_dataList.Enqueue(data);
					}
					else
					{
						// Collect "rubbish data" if system not ready.
						_dataList.Enqueue(data);
					}
				}
				else
				{
					Log.LogText(LogChannel, _currProcessId, "End Read with no data found", "A13", "PayECRComPort.DataReceivedEventHandle");
				}
			}
			catch (Exception ex)
			{
				Log.LogFatal(LogChannel, _currProcessId, ex, "EX01", "PayECRComPort.DataReceivedEventHandle");
			}
			finally
			{
				if (_dataReceivedEventLock.CurrentCount == 0)
					_dataReceivedEventLock.Release();

				Log.LogText(LogChannel, _currProcessId, "..... End of DataReceivedEventHandle.. Quit Lock ....", "B100",
						"PayECRComPort.DataReceivedEventHandle");

			}
		}

		private void ErrorReceivedEventHandle(object sender, SerialErrorReceivedEventArgs e)
		{
			Log.LogText(LogChannel, _currProcessId, $@"COM Port Error :{e.ToString()}", "EX01",
				"PayECRComPort.ErrorReceivedEventHandle", AppDecorator.Log.MessageType.Error);
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
						return dataBytes;
				}
				Thread.Sleep(200);
			}
			throw new SystemTimeOutException("System Timeout at section-RDT_5A. Fail to receive from I/O.");
		}

		public void WritePort(string writedata)
		{
			Log.LogText(LogChannel, _currProcessId, "WritePort:-Start :" + AsciiOctets2String(System.Text.Encoding.UTF8.GetBytes(writedata)), "A01", "PayECRComPort.WritePort");
			_comPort.Write(writedata);
			// Apply Write Latency Time
			Thread.Sleep(50);
			// ----------------------------
			Log.LogText(LogChannel, _currProcessId, "WritePort:-End", "A10", "PayECRComPort.WritePort");
		}

		public void WritePort2(string writedata, string tag)
		{
			Log.LogText(LogChannel, _currProcessId, $@"#{tag};WritePort:-Start :" + AsciiOctets2String(System.Text.Encoding.UTF8.GetBytes(writedata)), "A01", "PayECRComPort.WritePort");
			_comPort.Write(writedata);
			// Apply Write Latency Time
			Thread.Sleep(50);
			// ----------------------------
			Log.LogText(LogChannel, _currProcessId, $@"#{tag};WritePort:-End", "A10", "PayECRComPort.WritePort");
		}

		public int ReadPortChar()
		{
			byte[] buffer = ReadPort(3);
			if (buffer.Length > 0)
			{
				return buffer[0];
			}
			return 0;
		}

		public void Dispose()
		{
			if (_comPort != null)
			{
				Thread.Sleep(2500);

				try
				{
					try
					{
						if (_comPort != null)
						{
							_comPort.DataReceived -= DataReceivedEventHandle;
							_comPort.ErrorReceived -= ErrorReceivedEventHandle;

							_comPort.DiscardOutBuffer();
							_comPort.DiscardInBuffer();
							Thread.Sleep(200);

							_comPort.Close();
							Log.LogText(LogChannel, _currProcessId, "COM Port Closed", "A03", "PayECRComPort.Dispose");
						}
						else
						{
							Log.LogText(LogChannel, _currProcessId, "Error : Serial Port Instant not created.", "EX01", "PayECRComPort.Dispose", AppDecorator.Log.MessageType.Error);
						}
					}
					catch (Exception ex)
					{
						Log.LogError(LogChannel, _currProcessId, new Exception("Error when closing COM Port", ex), "EX03", "PayECRComPort.Dispose");
					}

					try
					{
						if (_comPort != null)
						{
							_comPort.Dispose();
							Log.LogText(LogChannel, _currProcessId, "COM Port Disposed", "A05", "PayECRComPort.Dispose");
						}
					}
					catch (Exception ex)
					{
						Log.LogError(LogChannel, _currProcessId, new Exception("Disposing COM Port", ex), "EX05", "PayECRComPort.Dispose");
					}
				}
				catch { }

				_comPort = null;
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

			private const string LogChannel = "PAX_IM20_API";
			private DateTime _dataPackingTimeout = DateTime.Now;

			private Queue<byte> _currentDataPack = new Queue<byte>();

			// _STXFound always initiate to false at the beginning a data packing.
			private bool _STXFound = false;
			// _ETXFound is set to false when still not found a ETX byte in data packing.
			private bool _ETXFound = false;

			private bool _disposed = false;

			public DataPack()
			{ }

			public string CurrProcessId { get; set; } = "-";

			private DbLog _log = null;
			public DbLog Log
			{
				get
				{
					return _log ?? (_log = DbLog.GetDbLog());
				}
			}

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
						if (dtByte == _ETXCode)
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
				}

				//-----------------------------------------------------------------------
				// Timeout Return OR Finish packing Return 
				bool isTimeOut = (_dataPackingTimeout.Subtract(DateTime.Now).TotalSeconds <= 0);

				if ((isTimeOut)
					|| (isFinishPacking))
				{
					if (isTimeOut)
						Log.LogError(LogChannel, CurrProcessId, new Exception("Data Packaging Timeout"), "X01", "DataPack.PackingData");

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
			}

			public void Dispose()
			{
				_disposed = true;
				_currentDataPack?.Clear();
			}
		}
	}
}