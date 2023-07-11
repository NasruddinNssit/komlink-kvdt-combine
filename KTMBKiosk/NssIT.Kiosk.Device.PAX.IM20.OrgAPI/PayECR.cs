using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base;
using NssIT.Kiosk.Log.DB.StatusMonitor;
using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;

namespace NssIT.Kiosk.Device.PAX.IM20.OrgAPI
{
	public class PayECR : IDisposable
	{
		public delegate void DisallowStopDelg();
		public delegate void ShowInProgressDelg(InProgressEventArgs args);
		public delegate bool ProcessHasDisposedDelg();
		public delegate bool ComputeLRCforByteDelg(byte[] data);
		public delegate bool CheckProcessHasStopDelg();

		private const string LogChannel = "PAX_IM20_API";
		private DbLog _log = null;

		private static string _gs_com;
		private static int _gi_baudRate;
		private static int _gs_databits;

		private static Parity _g_parity;
		private static StopBits _g_stopbits;

		private static int _gi_timeout;
		private int _gi_buff_size;
		private PayECRComPort _go_SerialPort;

		private string _lastProcessID = "-";

		//private static String _g_LogPath;
		//private static String _g_ErrorLogPath;
		//private static bool _g_Do_Log;
		//private static bool _g_Do_Error_Log;

		private const byte _STXCode = 2;
		private const byte _ENQCode = 5;
		private const byte _EOTCode = 4; /*End of Transmission*/
		////for background worker
		//private Byte[] _g_AbortBytes = System.Text.Encoding.ASCII.GetBytes("ABORT");

		private bool _allowStopFlag = true;

		public event EventHandler<InProgressEventArgs> OnInProgress;

		private bool _stop = false;

		private ProcessHasDisposedDelg _processHasDisposedDelgHandle = null;
		private DisallowStopDelg _disallowStopDelgHandle = null;
		private ComputeLRCforByteDelg _computeLRCforByteDelgHandle = null;
		private CheckProcessHasStopDelg _checkProcessHasStopDelgHandle = null;
		private ShowInProgressDelg _showInProgressDelgHandle = null;

		public bool Stop
		{
			get => _stop;
            set
            {
				if (value == false)
					_stop = false;
				else
                {
					if (_allowStopFlag)
						_stop = true;
				}
			}
		}

		private void DisallowStop()
        {
			_allowStopFlag = false;
			_stop = false;
		}

		private bool CheckProcessHasStop()
        {
			return Stop;
		}

		/// <summary>
		/// This function only for internal system purpose. NOT for end user.
		/// </summary>
		public void ForceToStop()
        {
			_stop = true;
		}

		public DbLog Log
		{
			get
			{
				return _log ?? (_log = DbLog.GetDbLog());
			}
		}

		#region -- Constructor --

		public PayECR(string com, int baudrate, int databits, string parity, string stopbits, int timeout, int receive_buffer_size) /*, string log_path, bool log, bool error_log)*/
		{
			_lastProcessID = "-";
			//log_path = log_path ?? "";
			//if ((log_path.Length > 0) && (log_path.Substring((log_path.Length - 1), 1).Equals(@"\") == false))
			//	log_path = log_path + @"\";

			if (com != null && com != "" && com.ToUpper().Contains("COM"))
			{
				_gs_com = com;
			}
			else
			{
				_gs_com = "COM1";
			}

			parity = (parity ?? "").Trim().ToUpper();
			if (parity.Length > 0)
			{
				if (parity.Equals("NONE"))
					_g_parity = Parity.None;
				else if (parity.Equals("EVEN"))
					_g_parity = Parity.Even;
				else if (parity.Equals("MARK"))
					_g_parity = Parity.Mark;
				else if (parity.Equals("ODD"))
					_g_parity = Parity.Odd;
				else if (parity.Equals("SPACE"))
					_g_parity = Parity.Space;
				else
					_g_parity = Parity.None;
			}
			else
			{
				_g_parity = Parity.None;
			}

			stopbits = (stopbits ?? "").Trim().ToUpper();
			if (stopbits.Length > 0)
			{
				if (stopbits.Equals("NONE"))
					_g_stopbits = StopBits.None;
				else if (stopbits.Equals("ONE"))
					_g_stopbits = StopBits.One;
				else if (stopbits.Equals("ONEPOINTFIVE"))
					_g_stopbits = StopBits.OnePointFive;
				else if (stopbits.Equals("TWO"))
					_g_stopbits = StopBits.Two;
				else
					_g_stopbits = StopBits.None;
			}
			else
			{
				_g_stopbits = StopBits.None;
			}

			if (databits > 0)
			{
				_gs_databits = databits;
			}
			else
				_gs_databits = 8;

			if (baudrate > 0)
			{
				_gi_baudRate = baudrate;
			}
			else
				_gi_baudRate = 9600;

			if (timeout > 0)
			{
				_gi_timeout = timeout;
			}
			else
				_gi_timeout = 120000;

			if (receive_buffer_size > 0)
			{
				_gi_buff_size = receive_buffer_size;
			}
			else
				_gi_buff_size = 1024;

			_processHasDisposedDelgHandle = new ProcessHasDisposedDelg(ProcessHasDisposed);
			_disallowStopDelgHandle = new DisallowStopDelg(DisallowStop);
			_computeLRCforByteDelgHandle = new ComputeLRCforByteDelg(computeLRCforByte);
			_checkProcessHasStopDelgHandle = new CheckProcessHasStopDelg(CheckProcessHasStop);
			_showInProgressDelgHandle = new ShowInProgressDelg(ShowInProgressWorking);

			//if (log_path != "")
			//{
			//	if (IsWrite(log_path))
			//	{
			//		_g_LogPath = log_path;
			//		_g_ErrorLogPath = log_path;
			//		_g_Do_Log = log;
			//		_g_Do_Error_Log = error_log;
			//	}
			//	else
			//	{
			//		_g_Do_Log = false;
			//		_g_Do_Error_Log = false;
			//	}
			//}
			//else
			//{
			//	_g_Do_Log = false;
			//	_g_Do_Error_Log = false;
			//}
		}

		public PayECR(string com, int timeout, int receive_buffer_size) /*, string log_path, bool log, bool error_log)*/
			: this(com, 9600, 8, "NONE", "ONE", timeout, receive_buffer_size) /*, log_path, log, error_log)*/
		{ }

		#endregion

		#region -- Public Functions --
		private bool _sendReceiveInProgress = false;
		/*
         * status code 0 - success,1 - ENQ failed, 2 - Write timeout, 3 - Read timeout,4- Null COM,5 Receive Wrong LRC
         * view windows event log for more details
         * log path c:/ECR_LOG/         
         */
		public void SendReceive(string processId, string writedata, ref string readdata, ref int status_code, ref string status_remark, int receive_buffer_size, int defaultWaitingTimeSec = 300, int defaultFirstWaitingTimeSec = 60)
		{
			try
			{
				_allowStopFlag = true;
				_lastProcessID = processId ?? "-";
				_sendReceiveInProgress = true;

				_go_SerialPort = PayECRComPort.GetPayECRComPort(_gs_com, _gi_baudRate, _g_parity, _gs_databits, _g_stopbits, processId); 

				int li_lrc = 0;
				string ls_send = "";
				ls_send = "\x02";
				status_code = 0;
				writedata = writedata + "\x03";
				li_lrc = computeLRC(writedata);
				ls_send = ls_send + writedata + Convert.ToChar(li_lrc);

				try
				{
					Log.LogText(LogChannel, processId, "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX Entering Serial Port working XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", "A01", "PayECR.SendReceive");

					if (!_go_SerialPort.PortIsOpen)
						_go_SerialPort.OpenPort();

					if (_go_SerialPort.PortIsOpen)
						_go_SerialPort.CleanUp();

					if (Enquiry(processId, ref status_code, ref status_remark, out bool isPortGotResponse))
					{
						if (WriteToCOM(processId, ref status_code, ref status_remark, ls_send))
						{
							ReadFromCOM(processId, ref status_code, ref status_remark, ref readdata, defaultWaitingTimeSec, defaultFirstWaitingTimeSec);
						}
					}

					if (isPortGotResponse)
						StatusHub.GetStatusHub().zNewStatus_IsCardMachineDataCommNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Normal Data Communication");

					else
						StatusHub.GetStatusHub().zNewStatus_IsCardMachineDataCommNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Machine fail to respond");
				}
				catch (SystemTimeOutException ex)
				{
					status_code = 4;
					status_remark = ex.Message;
					Log.LogError(LogChannel, processId, new Exception("System Timeout Exception. ", ex), "EX01", "PayECR.SendReceive");
					StatusHub.GetStatusHub().zNewStatus_IsCardMachineDataCommNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Machine fail to respond");
				}
				catch (TimeoutException ex)
				{
					status_code = 4;
					status_remark = ex.Message;
					Log.LogError(LogChannel, processId, new Exception("Timeout Exception. ", ex), "EX02", "PayECR.SendReceive");
					StatusHub.GetStatusHub().zNewStatus_IsCardMachineDataCommNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Machine fail to respond");
				}
			}
			catch (Exception ex)
			{
				status_code = 4;
				status_remark = ex.Message;
				Log.LogError(LogChannel, processId, ex, "EX03", "PayECR.SendReceive");
				StatusHub.GetStatusHub().zNewStatus_IsCardMachineDataCommNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Machine fail to respond");
			}
			finally
			{
				_allowStopFlag = true;
				_sendReceiveInProgress = false;

				Log.LogText(LogChannel, processId, "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX End of Serial Port working XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", "A10", "PayECR.SendReceive");
			}
		}

		//PayCommand payCommand
		public void SendReceiveX(string command, string processId, string writedata, ref string readdata, ref int status_code, ref string status_remark, int receive_buffer_size, int defaultWaitingTimeSec = 300, int defaultFirstWaitingTimeSec = 60)
		{
			try
			{
				_allowStopFlag = true;
				_lastProcessID = processId ?? "-";
				_sendReceiveInProgress = true;

				_go_SerialPort = PayECRComPort.GetPayECRComPort(_gs_com, _gi_baudRate, _g_parity, _gs_databits, _g_stopbits, processId);

				int li_lrc = 0;
				string ls_send = "";
				ls_send = "\x02";
				status_code = 0;
				writedata = writedata + "\x03";
				li_lrc = computeLRC(writedata);
				ls_send = ls_send + writedata + Convert.ToChar(li_lrc);

				try
				{
					Log.LogText(LogChannel, processId, "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX Entering Serial Port working XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", "A01", "PayECR.SendReceive");

					if (!_go_SerialPort.PortIsOpen)
						_go_SerialPort.OpenPort();

					if (_go_SerialPort.PortIsOpen)
						_go_SerialPort.CleanUp();

					if (Enquiry(processId, ref status_code, ref status_remark, out bool isPortGotResponse))
					{
						if (WriteToCOM(processId, ref status_code, ref status_remark, ls_send))
						{
							_go_SerialPort.WritePortTimeOut = _gi_timeout;
							_go_SerialPort.ReadPortTimeOut = _gi_timeout;
							ITransProtocol trans = GetTransactionProtocol(command, _go_SerialPort, processId);

							if (trans != null)
							{
								try
                                {
									trans.Run(defaultWaitingTimeSec, defaultFirstWaitingTimeSec);
									status_code = trans.ResultStatusCode;
									status_remark = trans.ResultstatusRemark;
									readdata = trans.ResultReadData;
								}
								finally
                                {
									if (trans != null)
										try
										{
											trans.EndDispose();
										}
										catch (Exception ex)
										{
											string m = ex.Message;
										}
								}
							}
							else
								ReadFromCOM(processId, ref status_code, ref status_remark, ref readdata, defaultWaitingTimeSec, defaultFirstWaitingTimeSec);
						}
					}

					if (isPortGotResponse)
						StatusHub.GetStatusHub().zNewStatus_IsCardMachineDataCommNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Normal Data Communication");

					else
						StatusHub.GetStatusHub().zNewStatus_IsCardMachineDataCommNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Machine fail to respond");
				}
				catch (SystemTimeOutException ex)
				{
					status_code = 4;
					status_remark = ex.Message;
					Log.LogError(LogChannel, processId, new Exception("System Timeout Exception. ", ex), "EX01", "PayECR.SendReceive");
					StatusHub.GetStatusHub().zNewStatus_IsCardMachineDataCommNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Machine fail to respond");
				}
				catch (TimeoutException ex)
				{
					status_code = 4;
					status_remark = ex.Message;
					Log.LogError(LogChannel, processId, new Exception("Timeout Exception. ", ex), "EX02", "PayECR.SendReceive");
					StatusHub.GetStatusHub().zNewStatus_IsCardMachineDataCommNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Machine fail to respond");
				}
			}
			catch (Exception ex)
			{
				status_code = 4;
				status_remark = ex.Message;
				Log.LogError(LogChannel, processId, ex, "EX03", "PayECR.SendReceive");
				StatusHub.GetStatusHub().zNewStatus_IsCardMachineDataCommNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, "Machine fail to respond");
			}
			finally
			{
				_allowStopFlag = true;
				_sendReceiveInProgress = false;

				Log.LogText(LogChannel, processId, "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX End of Serial Port working XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", "A10", "PayECR.SendReceive");
			}

			return;
			/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

			ITransProtocol GetTransactionProtocol(string commandX, PayECRComPort goSerialPortX, string processIdX)
            {
				if (commandX.Equals(PayCommand.Norm_Sale, StringComparison.InvariantCultureIgnoreCase))
                {
					return new PayECRReadProtocolxSale(goSerialPortX, processIdX, _processHasDisposedDelgHandle, _disallowStopDelgHandle,
									_showInProgressDelgHandle, _computeLRCforByteDelgHandle, _checkProcessHasStopDelgHandle);
				}

				return null;
			}
		}

		#endregion

		#region -- Private Functions --

		private void ShowInProgressWorking(InProgressEventArgs args)
        {
            try
            {
				OnInProgress?.Invoke(null, args);
			}
			catch (Exception ex)
            {
				_log.LogError(LogChannel, "*", ex, "EX01", "PayECR.ShowInProgressWorking");
            }
		}

		private bool Enquiry(string processId, ref int status_code, ref string status_remark, out bool isResponseFound)
		{
			isResponseFound = false;

			bool lb_success = false;
			int li_retry = 0, li_response = 0;
			int maxRetry = 3;
			bool isEOTSent = false;

			status_code = 99;

			_go_SerialPort.WritePortTimeOut = 2000;
			_go_SerialPort.ReadPortTimeOut = 2000;

			Log.LogText(LogChannel, processId, "***** Begin Enquiry *****", "A01", "PayECR.Enquiry");

			while (li_retry < maxRetry)
			{
				isEOTSent = false;
				li_retry += 1;
				try
				{
					// .. reset 
					status_code = 99;
					status_remark = "";

					//Send ENQ
					_go_SerialPort.WritePort2("\x05", "WriteENQ");

					if (this.Stop)
					{
						_go_SerialPort.WritePort2("\x04", "StopAbort_After_WriteENQ");
						isEOTSent = true;
						break;
					}

					//..expecting "ACK" (\x06)
					li_response = 0;
					li_response = _go_SerialPort.ReadPortChar();
					//..if "ACK" (\x06) found..
					if (li_response == 6)
					{
						isResponseFound = true;
						lb_success = true;
						status_code = 0;
						status_remark = "ENQ Success";
						Log.LogText(LogChannel, processId, "#Received-Process :<ACK>; " + status_remark + "; TryCount : " + li_retry.ToString(), "A05", "PayECR.Enquiry");
						li_retry = maxRetry;
						break;
					}
					//..if negative ack (NAK \x15) found..
					else if (li_response == 21)
					{
						isResponseFound = true;
						status_code = 1;
						status_remark = "ENQ failed";
						Log.LogText(LogChannel, processId, "#Received-Process :<NAK>; " + status_remark + "; TryCount : " + li_retry.ToString(), "A07", "PayECR.Enquiry");
					}
					else
					{
						if (li_response != 0)
							isResponseFound = true;

						_go_SerialPort.WritePort2("\x04", $@"UnknownAnswer({li_response})_After_WriteENQ");
						isEOTSent = true;

						Log.LogText(LogChannel, processId, "#Unknown Response :" + PayECRComPort.AsciiOctets2String(System.Text.Encoding.UTF8.GetBytes(li_response.ToString()))
							+ "; TryCount : " + li_retry.ToString(), "A09", "PayECR.Enquiry");
					}
				}
				catch (SystemTimeOutException ex)
				{
					if (isEOTSent == false)
						_go_SerialPort.WritePort2("\x04", "SystemTimeout_After_WriteENQ");
					isEOTSent = true;

					status_code = 1;
					status_remark = ex.Message;
					Log.LogError(LogChannel, processId, new Exception("Enquiry System Timeout;", ex), "EX01", "PayECR.Enquiry");
				}
				catch (TimeoutException ex)
				{
					if (isEOTSent == false)
						_go_SerialPort.WritePort2("\x04", "Timeout_After_WriteENQ");
					isEOTSent = true;

					status_code = 1;
					status_remark = ex.Message;
					Log.LogError(LogChannel, processId, new Exception("Enquiry Timeout;", ex), "EX02", "PayECR.Enquiry");
				}
				//-----------------------------------
				//..if third (last) attempt;
				if (li_retry >= maxRetry)
				{
					if (isEOTSent == false)
						_go_SerialPort.WritePort2("\x04", "FailToGetACK_After_WriteENQ");
					isEOTSent = true;

					status_code = 1;
					status_remark = "ENQ failed";
					Log.LogText(LogChannel, processId, "Enquiry ENQ:-" + status_remark + "; TryCount : " + li_retry.ToString(), "B05", "PayECR.Enquiry");

					throw new Exception(status_remark);
				}
			}
			return lb_success;
		}

		private bool WriteToCOM(string processId, ref int status_code, ref string status_remark, string writedata)
		{
			bool lb_success = false;
			int li_retry = 0, li_response = 0;
			int maxRetry = 3;
			bool isEOTSent = false;

			status_code = 2;
			_go_SerialPort.WritePortTimeOut = _gi_timeout;
			_go_SerialPort.ReadPortTimeOut = _gi_timeout;

			Log.LogText(LogChannel, processId, "***** Begin WriteToCOM *****", "A01", "PayECR.WriteToCOM");

			while (li_retry < maxRetry)
			{
				status_code = 2;
				isEOTSent = false;
				li_retry += 1;
				try
				{
					//------------------------
					// Send Data 
					_go_SerialPort.WritePort2(writedata, "WriteCommand");
					//------------------------
					// ..expect response
					li_response = _go_SerialPort.ReadPortChar();
					//------------------------
					// Abort
					if (this.Stop)
					{
						status_remark = "Write aborted";
						_go_SerialPort.WritePort2("\x04", "StopAborted_After_WriteCommand");
						isEOTSent = true;
						break;
					}
					//------------------------

					//if success - ACK found
					if (li_response == 6)
					{
						li_retry = maxRetry;
						lb_success = true;
						status_code = 0;
						status_remark = "Write Success";
						Log.LogText(LogChannel, processId, "#Received-Process :<ACK>; " + status_remark, "A03", "PayECR.WriteToCOM");

						_go_SerialPort.WritePort2("\x04", "EndWithSuccess_After_WriteCommand");
						isEOTSent = true;

						break;
					}
					//if negative ack
					else if (li_response == 21)
					{
						status_remark = "Write failed";
						Log.LogText(LogChannel, processId, "#Received-Process :<NAK>; " + status_remark, "A05", "PayECR.WriteToCOM");
					}
					else
					{
						_go_SerialPort.WritePort2("\x04", $@"UnknownAnswer({li_response})_After_WriteCommand");
						isEOTSent = true;

						if (li_retry < maxRetry)
							Log.LogText(LogChannel, processId, "#Unknown Response :" + PayECRComPort.AsciiOctets2String(System.Text.Encoding.UTF8.GetBytes(li_response.ToString())), "A07", "PayECR.WriteToCOM");
					}

				}
				catch (SystemTimeOutException ex)
				{
					if (isEOTSent == false)
						_go_SerialPort.WritePort2("\x04", $@"SystemTimeout_After_WriteCommand");
					isEOTSent = true;

					status_remark = ex.Message;
					Log.LogError(LogChannel, processId, new Exception("System Timeout;",  ex), "EX01", "PayECR.WriteToCOM");
				}
				catch (TimeoutException ex)
				{
					if (isEOTSent == false)
						_go_SerialPort.WritePort2("\x04", $@"Timeout_After_WriteCommand");
					isEOTSent = true;

					status_remark = ex.Message;
					Log.LogError(LogChannel, processId, new Exception("Timeout;", ex), "EX02", "PayECR.WriteToCOM");
				}

				//-----------------------------------
				//..if third (last) attempt;
				if (li_retry >= maxRetry)
				{
					status_remark = "Write failed";
					Log.LogText(LogChannel, processId, "#Try Send Status :" + status_remark + "; TryCount : " + li_retry.ToString(), "A20", "PayECR.WriteToCOM");

					if (isEOTSent == false)
						_go_SerialPort.WritePort2("\x04", $@"FailToGetACK_After_WriteCommand");
					isEOTSent = true;

					throw new Exception(status_remark);
				}
			}
			return lb_success;
		}

		private bool ReadFromCOM(string processId, ref int status_code, ref string status_remark, ref string readdata, int defaultTransactionWaitingTimeSec = 300, int defaultFirstWaitingTimeSec = 60)
		{
			CardProcessState currProcState = CardProcessState.Start;
			string cardDetectedMsg = "";
			bool lb_success = false;
			int li_retry = 0, li_response = 0;
			bool reqToRedoReadDataPackage = false;

			bool responseFound = false;
			bool localTimeout = false;
			bool abortFlag = false;

			// cancelFlagDetacted used when Stop Flag is set. But system will wait for a short period to read outstanding data.
			bool cancelFlagDetacted = false;
			// stopFlagDetacted used after Stop Flag is set and cancelFlagDetacted has detected and no outstanding data found.
			bool stopFlagDetacted = false;

			// defaultFirstWaitingTime is a period used to wait for user to tap card.
			int defaultFirstWaitingTime = defaultFirstWaitingTimeSec * 1000;

			//CYA-TEST .. defaultFirstWaitingTime = 30 * 1000;

			// defaultSuspendEndWaitingTimeSec is a period for Total Canceling & Ending Time.
			int defaultSuspendEndWaitingTimeSec = 20;
			// minWaitingTimeSec is a minimum period used to wait for response data after tap card.
			int minWaitingTimeSec = 60 + defaultSuspendEndWaitingTimeSec;

			if (defaultTransactionWaitingTimeSec < minWaitingTimeSec)
				defaultTransactionWaitingTimeSec = minWaitingTimeSec;

			// transactionWaitingTime is a maximum period used to wait for response data after tap card.
			int transactionWaitingTime = (defaultTransactionWaitingTimeSec - defaultSuspendEndWaitingTimeSec) * 1000;

			int maxStopDelayTime = 9 * 1000;
			int maxCancelDelayTime = 7 * 1000;

			status_code = 99;
			_go_SerialPort.WritePortTimeOut = _gi_timeout;
			_go_SerialPort.ReadPortTimeOut = _gi_timeout;

			string clesCode = "";
			int lootCount = 0;
			byte[] recebuff;
			bool continueLoop = true;// to start the loop//  added to test kiosk purpose
			int tmpTimeoutSec = 10;

			int actualDataLen = 0;
			InProgressEventArgs ev = null;

			DateTime timeOut = DateTime.Now;
			UpdateTimeOut(DateTime.Now.AddMilliseconds(defaultFirstWaitingTime), 
				"------------ Start Read From COM (defaultFirstWaitingTime) ------------",
				CardProcessState.Start);

			try
			{
				Log.LogText(LogChannel, processId, "***** Begin ReadFromCOM *****", "A01", "PayECR.ReadFromCOM");
				while (continueLoop)// for added to test kiosk purpose
				{
					Log.LogText(LogChannel, processId, "DEBUG::S001", "S001", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);

					lootCount += 1;
					continueLoop = false;
					status_code = 99;

					recebuff = new byte[0];

					Thread.Sleep(5);

					// Below statement used to pro-long "timeOut" in case time is not enough. 
					if (stopFlagDetacted == true)
						UpdateTimeOut(DateTime.Now.AddMilliseconds(maxStopDelayTime), "Pro-long when stopFlagDetacted == true", CardProcessState.EndTransaction);

					else if (cancelFlagDetacted == true)
						UpdateTimeOut(DateTime.Now.AddMilliseconds(maxCancelDelayTime), "Pro-long when cancelFlagDetacted == true", CardProcessState.CancelTransaction);

					//-----
					Log.LogText(LogChannel, processId, "DEBUG::S002", "S002", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);
					//---------------------------------------------------------------------------------
					// Waiting for Tapping Card / Terminal Input 
					while (_go_SerialPort.BytesToRead < 1)
					{
						if (((this.Stop) || (localTimeout))
							&& (responseFound == false))
						{
							if (cancelFlagDetacted == false)
							{
								Log.LogText(LogChannel, processId, $@"DEBUG::S002B; Stop: {this.Stop}; localTimeout: {localTimeout}; responseFound : {responseFound}",
									"S002B", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);

								cancelFlagDetacted = true;
								UpdateTimeOut(DateTime.Now.AddMilliseconds(maxCancelDelayTime), 
									$@"State01; Stop: {this.Stop}; localTimeout: {localTimeout}; responseFound: {responseFound}", 
									CardProcessState.CancelTransaction);
							}
							else if ((timeOut.Subtract(DateTime.Now).TotalSeconds <= 0) && (stopFlagDetacted == false))
							{
								Log.LogText(LogChannel, processId, $@"DEBUG::S002C; Stop: {this.Stop}; localTimeout: {localTimeout}; responseFound : {responseFound}",
									"S002C", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);

								stopFlagDetacted = true;
								UpdateTimeOut(DateTime.Now.AddMilliseconds(maxStopDelayTime), 
									$@"State02; TimeOut and (stopFlagDetacted == false)", 
									CardProcessState.EndTransaction);

								if (abortFlag == false)
								{
									abortFlag = true;
									Log.LogText(LogChannel, processId, "DEBUG::S003", "S003", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);
									_go_SerialPort.WritePort("ABORT");
									Thread.Sleep(5);

									if (this.Stop)
									{
										Log.LogText(LogChannel, processId, @"ABORT base on STOP", "B03", "PayECR.ReadFromCOM");
									}
									else
									{
										Log.LogText(LogChannel, processId, @"ABORT cause by timeout", "B05", "PayECR.ReadFromCOM");
									}
								}
							}
							else if ((cancelFlagDetacted == true) && (stopFlagDetacted == true) && (timeOut.Subtract(DateTime.Now).TotalSeconds <= 0))
							{
								Log.LogText(LogChannel, processId, $@"DEBUG::S002D; Stop: {this.Stop}; localTimeout: {localTimeout}; responseFound : {responseFound}",
									"S002D", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);

								if (this.Stop)
								{
									throw new ImmediateTerminateException("Cancel Request at Section-RFM_1");
								}
								else
								{
									throw new SystemTimeOutException("Timeout at Section-RFM_1");
								}
							}
							else
								Thread.Sleep(5);
						}
						else if ((responseFound == true) && (timeOut.Subtract(DateTime.Now).TotalSeconds <= 0))
						{
							if (abortFlag == false)
							{
								abortFlag = true;
								Log.LogText(LogChannel, processId, $@"DEBUG::S004; timeOut: {timeOut:yyyy-MM-dd HH:mm:ss}", "S004", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);
								_go_SerialPort.WritePort("ABORT");
								Thread.Sleep(5);

								throw new SystemTimeOutException("Timeout at Section-RFM_2");
							}
						}
						else if (timeOut.Subtract(DateTime.Now).TotalSeconds <= 0)
						{
							localTimeout = true;

							if (abortFlag == false)
							{
								abortFlag = true;
								Log.LogText(LogChannel, processId, "DEBUG::S005", "S005", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);
								_go_SerialPort.WritePort("ABORT");
								Thread.Sleep(5);
							}
						}
						else
							Thread.Sleep(5);
					}
					Log.LogText(LogChannel, processId, "DEBUG::S006B", "S006B", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);
					//---------------------------------------------------------------------------------
					ev = new InProgressEventArgs() { Message = $"..progressing {cardDetectedMsg} - {lootCount.ToString()}" };
					ShowInProgressWorking(ev);
					//---------------------------------------------------------------------------------
					// Read data from COM Port if found any data.
					Log.LogText(LogChannel, processId, "DEBUG::S006", "S006", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);

					li_response = 0;

					if (_go_SerialPort.BytesToRead < 1)
					{
						status_remark = "Unable to read I/O Info properly!";
						Log.LogText(LogChannel, processId, $@"I/O Error : {status_remark}", "EX11", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Error);
						throw new Exception(status_remark);
					}
					else if (_go_SerialPort.BytesToRead <= 2)
						li_response = _go_SerialPort.ReadPortChar();
					else
					{
						Log.LogText(LogChannel, processId, "Read data block without received <ENQ>", "A12", "PayECR.ReadFromCOM");

						if (responseFound == false)
						{
							responseFound = true;
							UpdateTimeOut(DateTime.Now.AddMilliseconds(transactionWaitingTime),
								$@"State03; responseFound == false",
								CardProcessState.CardProcessing);
						}
					
						tmpTimeoutSec = (int)timeOut.Subtract(DateTime.Now).TotalSeconds;

						Log.LogText(LogChannel, processId, $@"DEBUG::S007H; tmpTimeoutSec : {tmpTimeoutSec}; timeOut : {timeOut}", "S007", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);

						recebuff = _go_SerialPort.ReadPort(tmpTimeoutSec);
					}
					Log.LogText(LogChannel, processId, "DEBUG::S007", "S007", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);
					//---------------------------------------------------------------------------------
					if (((li_response > 0) || (recebuff.Length > 0))
							&& (responseFound == false))
					{
						if (li_response != 6)
                        {
							responseFound = true;
							UpdateTimeOut(DateTime.Now.AddMilliseconds(transactionWaitingTime),
								$@"State04; li_response: {li_response};  recebuff.Length: {recebuff?.Length}; responseFound: {responseFound}",
								CardProcessState.CardProcessing);
						}
					}
					//---------------------------------------------------------------------------------

					if (this.Stop)
					{
						ev = new InProgressEventArgs() { Message = $"..progressing {cardDetectedMsg} - {lootCount.ToString()} - Stop" };
						ShowInProgressWorking(ev);
					}
					else if (localTimeout)
					{
						ev = new InProgressEventArgs() { Message = $"..progressing {cardDetectedMsg} - {lootCount.ToString()} - Timeout" };
						ShowInProgressWorking(ev);
					}

					Log.LogText(LogChannel, processId, "DEBUG::S008", "S008", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);
					//---------------------------------------------------------------------------------
					// Detect CLES for SALE.
					clesCode = "";

					if (recebuff.Length == 4)
					{
						clesCode = System.Text.ASCIIEncoding.ASCII.GetString(recebuff);
						Log.LogText(LogChannel, processId, $@"4 bytes string : {clesCode}", "A15", "PayECR.ReadFromCOM");
					}
					else if (recebuff.Length == 3)
					{
						string resStr = System.Text.ASCIIEncoding.ASCII.GetString(recebuff).Trim();

						if (resStr.Equals("PIN"))
                        {
							clesCode = resStr;
							Log.LogText(LogChannel, processId, $@"3 bytes string : {clesCode}", "A15B", "PayECR.ReadFromCOM");
						}
						else if (resStr.Equals("PEF"))
						{
							clesCode = resStr;
							Log.LogText(LogChannel, processId, $@"3 bytes string : {clesCode}", "A15C", "PayECR.ReadFromCOM");
						}
					}

					if (clesCode.Equals("CLES"))
					{
						_allowStopFlag = false;
						cardDetectedMsg = "(Card tapped)";
						Log.LogText(LogChannel, processId, "Found :<CLES>; card tapped", "A17A", "PayECR.ReadFromCOM");

						ev = new InProgressEventArgs() { Message = $"..progressing - card tapped" };
						ShowInProgressWorking(ev);

						Log.LogText(LogChannel, processId, "DEBUG::S009A", "S009A", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);

						if (_go_SerialPort.BytesToRead == 0)
							_go_SerialPort.WritePort("\x06");

						if (li_retry < 3)
							continueLoop = true;
					}
					else if (clesCode.Equals("CARD"))
					{
						_allowStopFlag = false;
						cardDetectedMsg = "(Card Inserted and Verified)";
						Log.LogText(LogChannel, processId, "Found :<CARD>; Card Inserted and Verified", "A17B", "PayECR.ReadFromCOM");

						ev = new InProgressEventArgs() { Message = $"..progressing - Card Inserted and Verified" };
						ShowInProgressWorking(ev);

						Log.LogText(LogChannel, processId, "DEBUG::S009B", "S009B", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);

						if (_go_SerialPort.BytesToRead == 0)
							_go_SerialPort.WritePort("\x06");

						if (li_retry < 3)
							continueLoop = true;
					}
					else if (clesCode.Equals("MAGS"))
					{
						_allowStopFlag = false;
						cardDetectedMsg = "(Mag. Stripe and Verified)";
						Log.LogText(LogChannel, processId, "Found :<MAGS>; Mag. Stripe and Verified", "A17C", "PayECR.ReadFromCOM");

						ev = new InProgressEventArgs() { Message = $"..progressing - Mag. Stripe and Verified" };
						ShowInProgressWorking(ev);

						Log.LogText(LogChannel, processId, "DEBUG::S009C", "S009C", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);

						if (_go_SerialPort.BytesToRead == 0)
							_go_SerialPort.WritePort("\x06");

						if (li_retry < 3)
							continueLoop = true;
					}
					else if (clesCode.Equals("SCAN"))
					{
						_allowStopFlag = false;
						cardDetectedMsg = "(Barcode Scan and Verified)";
						Log.LogText(LogChannel, processId, "Found :<SCAN>; Barcode Scan and Verified", "A17D", "PayECR.ReadFromCOM");

						ev = new InProgressEventArgs() { Message = $"..progressing - Barcode Scan and Verified" };
						ShowInProgressWorking(ev);

						Log.LogText(LogChannel, processId, "DEBUG::S009D", "S009D", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);

						if (_go_SerialPort.BytesToRead == 0)
							_go_SerialPort.WritePort("\x06");

						if (li_retry < 3)
							continueLoop = true;
					}
					else if (clesCode.Equals("PIN"))
					{
						_allowStopFlag = false;
						cardDetectedMsg = "(PIN Request)";
						Log.LogText(LogChannel, processId, "Found :<PIN>; card slot in", "A17E", "PayECR.ReadFromCOM");

						ev = new InProgressEventArgs() { Message = $"..progressing - card PIN Request" };
						ShowInProgressWorking(ev);

						Log.LogText(LogChannel, processId, "DEBUG::S009E", "S009E", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);

						if (_go_SerialPort.BytesToRead == 0)
							_go_SerialPort.WritePort("\x06");

						if (li_retry < 3)
							continueLoop = true;
					}
					else if (clesCode.Equals("PEF"))
					{
						_allowStopFlag = false;
						cardDetectedMsg = "(PIN Entry Found)";
						Log.LogText(LogChannel, processId, "Found :<PEF>; card slot in", "A17F", "PayECR.ReadFromCOM");

						ev = new InProgressEventArgs() { Message = $"..progressing - card PIN Entry Found" };
						ShowInProgressWorking(ev);

						Log.LogText(LogChannel, processId, "DEBUG::S009F", "S009F", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);

						if (_go_SerialPort.BytesToRead == 0)
							_go_SerialPort.WritePort("\x06");

						if (li_retry < 3)
							continueLoop = true;
					}

					// if success - ENQ found
					else if (
						((li_response == 5) || ((li_response == 6) && (this.Stop == true))) 
						|| ((recebuff.Length > 0) && (recebuff[0] == _STXCode))
						)
					{
						if ((recebuff.Length > 0) && (recebuff[0] == _STXCode))
						{
							_allowStopFlag = false;
							cardDetectedMsg = "Card Response Data Found";
						}

						//acknowledge 
						if ((li_response == 5) || (li_response == 6))
						{
							if (li_response == 6)
							{
								Log.LogText(LogChannel, processId, "Received-Process :<ACK>", "A20B", "PayECR.ReadFromCOM");
							}
							else
							{
								Log.LogText(LogChannel, processId, "Received-Process :<ENQ>", "A20", "PayECR.ReadFromCOM");

								if (_go_SerialPort.BytesToRead == 0)
									_go_SerialPort.WritePort("\x06");
								Thread.Sleep(10);
							}
						}

						reqToRedoReadDataPackage = false;
						while (li_retry < 3)
						{
							li_retry += 1;
							status_code = 99;

                            try
                            {
                                actualDataLen = 0;

                                if (((li_response == 5) || ((li_response == 6) && (this.Stop == true))) || (reqToRedoReadDataPackage == true))
                                {
                                    if ((reqToRedoReadDataPackage == true))
                                    {
                                        Log.LogText(LogChannel, processId, "Redo read data block upon LRC Error", "EX13", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Error);
                                    }
                                    else
                                    {
                                        Log.LogText(LogChannel, processId, "Read data block upon ENQ", "A21", "PayECR.ReadFromCOM");
                                    }

                                    tmpTimeoutSec = (int)timeOut.Subtract(DateTime.Now).TotalSeconds;
                                    Log.LogText(LogChannel, processId, $@"DEBUG::S010; tmpTimeoutSec : {tmpTimeoutSec}; timeOut : {timeOut}", "S010", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);
                                    recebuff = _go_SerialPort.ReadPort(tmpTimeoutSec);
                                    actualDataLen = recebuff.Length;
                                    reqToRedoReadDataPackage = false;
                                }
                                else
                                {
                                    actualDataLen = recebuff.Length;
                                }

                                readdata = System.Text.ASCIIEncoding.ASCII.GetString(recebuff);

                                Log.LogText(LogChannel, processId, "Received-Process current block data :" + PayECRComPort.AsciiOctets2String((System.Text.Encoding.UTF8.GetBytes(readdata.Replace("\0", String.Empty).Trim()))),
                                    "A22", "PayECR.ReadFromCOM");

                                if ((recebuff.Length > 0) && (recebuff.Length <= 3))
                                {
                                    /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                                    ///// (2019-08-05; BY CYA;) Below coding block used only temporary to handle the problem may occur in iUC180B. Problem : iUC180B may send 2 times <ENQ> after card tapped.
                                    status_remark = "IM20 bug X01 occur.";
                                    status_code = 99;

                                    if (recebuff[0] == _EOTCode)
                                    {
                                        status_remark = "IM20 bug X01 occur with EOT.";
                                        li_retry = 3;
                                    }
                                    else
                                    {
                                        Log.LogText(LogChannel, processId, "DEBUG::S011", "S011", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);

                                        if (recebuff[0] == _ENQCode)
                                            _go_SerialPort.WritePort("\x06");
                                        else
                                            _go_SerialPort.WritePort("\x15");

                                        if (li_retry > 0)
                                            li_retry -= 1;

                                        //Task.Delay(100).Wait();
                                        Thread.Sleep(100);
                                    }
                                    Log.LogText(LogChannel, processId, status_remark, "A23", "PayECR.ReadFromCOM");

                                    /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                                }
                                else if (computeLRCforByte(recebuff))
                                {
                                    status_remark = "LRC OK";
                                    Log.LogText(LogChannel, processId, "#Success :" + status_remark, "A25", "PayECR.ReadFromCOM");

									_go_SerialPort.WritePort("\x06");

									Thread.Sleep(10);
									readdata = readdata.Substring(1, actualDataLen - 3);
									lb_success = true;
                                    li_retry = 3;
                                    status_code = 0;
                                    break;
                                }
                                else
                                {
                                    status_remark = "LRC Error!";
                                    Log.LogText(LogChannel, processId, "#Fail :" + status_remark, "EX15", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Error);
                                    _go_SerialPort.WritePort("\x15");
                                    Thread.Sleep(10);
                                    status_code = 5;
                                    reqToRedoReadDataPackage = true;
                                }
                            }
                            catch (SystemTimeOutException ex)
                            {
                                status_code = 1;
                                status_remark = ex.Message;
                                Log.LogError(LogChannel, processId, new Exception("System Timeout;", ex), "EX17", "PayECR.ReadFromCOM");
                            }
                            catch (TimeoutException ex)
                            {
                                status_code = 1;
                                status_remark = ex.Message;
                                Log.LogError(LogChannel, processId, new Exception("Timeout;", ex), "EX18", "PayECR.ReadFromCOM");
                            }
                        }

						//wait for Ending of Transmission
						if (status_code == 0)
						{
							try
							{
								Log.LogText(LogChannel, processId, "DEBUG::S012", "S012", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);
								int li_eot = _go_SerialPort.ReadPortChar();
								if (li_eot == 4)
								{
									status_remark = "EOT Success";
									Log.LogText(LogChannel, processId, "#Received-Process :<EOT>", "A27", "PayECR.ReadFromCOM");
								}
							}
							catch (Exception et)
							{
								status_remark = "EOT Timeout- " + et.Message;
								Log.LogError(LogChannel, processId, new Exception("#Error :" + status_remark, et), "EX20", "PayECR.ReadFromCOM");
							}
						}
					}
					// if negative ack instead of enq -- something wrong
					else if (li_response == 21)
					{
						//retry already fail this would be the 3rd try so exit the loop
						li_retry = 3;
						status_code = 3;
						status_remark = " Read NAK";
						Log.LogText(LogChannel, processId, "#Received-Process :<NAK>" + status_remark, "A30", "PayECR.ReadFromCOM");
					}

					else if ((li_retry < 3) && (abortFlag == false))
					{
						continueLoop = true;

						if (recebuff.Length > 0)
						{
							Log.LogText(LogChannel, processId, $@"Unknown Data : {PayECRComPort.AsciiOctets2String(recebuff)}", "EX22", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Error);
						}
						else
						{
							Log.LogText(LogChannel, processId, $@"Unknown Byte Response : {PayECRComPort.AsciiOctets2String(System.Text.Encoding.UTF8.GetBytes(li_response.ToString()))}"
								, "EX24", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Error);
						}

						Log.LogText(LogChannel, processId, "DEBUG::S013", "S013", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);

						if (_go_SerialPort.BytesToRead == 0)
							_go_SerialPort.WritePort("\x06");

						Thread.Sleep(50);
					}
				}
			}
			catch (ImmediateTerminateException itex)
			{
				status_code = 4;
				status_remark = $@"<ABORT> ({itex.Message})";
				lb_success = false;

				if (abortFlag == false)
				{
					Log.LogText(LogChannel, processId, "DEBUG::S014", "S014", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);
					_go_SerialPort.WritePort("ABORT");
					Thread.Sleep(5);
				}

				Log.LogError(LogChannel, processId, itex, "EX26", "PayECR.ReadFromCOM");
			}
			catch (SystemTimeOutException stex)
			{
				status_code = 4;
				status_remark = $@"<ABORT> ({stex.Message})";
				lb_success = false;

				if (abortFlag == false)
				{
					Log.LogText(LogChannel, processId, "DEBUG::S015", "S015", "PayECR.ReadFromCOM", AppDecorator.Log.MessageType.Debug);
					_go_SerialPort.WritePort("ABORT");
					Thread.Sleep(5);
				}

				Log.LogError(LogChannel, processId, stex, "EX28", "PayECR.ReadFromCOM");
			}
			finally
            {
				Log.LogText(LogChannel, processId, "***** End of ReadFromCOM *****", "A50", "PayECR.ReadFromCOM");
			}

			return lb_success;

			//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

			void UpdateTimeOut(DateTime newTimeOut, string tag, CardProcessState processState)
            {
				timeOut = newTimeOut;
				CardProcessState lastState = currProcState;
				currProcState = processState;
				string sStr = $@"Last State : {Enum.GetName(typeof(CardProcessState), lastState)}; Current State : {Enum.GetName(typeof(CardProcessState), currProcState)}";

				if (currProcState == CardProcessState.CardProcessing)
                {
					//Reset Flags
					this.Stop = false;
					localTimeout = false;
					abortFlag = false;
					cancelFlagDetacted = false;
					stopFlagDetacted = false;
					//---------------------------

					InProgressEventArgs evX = new InProgressEventArgs() { Message = $@".. card signal found;"};
					ShowInProgressWorking(evX);

					Log.LogText(LogChannel, processId, $@"New PayECR Timeout : {newTimeOut:yyyy-MM-dd HH:mm:ss} - Tag : {tag}; {sStr}", "E50A", "PayECR.UpdateTimeOut");
				}
				else
                {
					Log.LogText(LogChannel, processId, $@"New PayECR Timeout : {newTimeOut:yyyy-MM-dd HH:mm:ss} - Tag : {tag}; {sStr}", "E50B", "PayECR.UpdateTimeOut");
				}
			}
		}

		private bool VerifyLRC(string processId, string orig_data, string rtn_data, int length)
		{
			bool lb_rtn = false;
			if (rtn_data.IndexOf("R200") >= 0 && rtn_data.IndexOf("12\u0003") >= 0)
			{
				Log.LogText(LogChannel, processId, "LRC bypassed!", null, "PayECR.VerifyLRC");
				lb_rtn = true;
			}
			else if (Convert.ToChar(orig_data.Substring(length - 1, 1)) == Convert.ToChar(computeLRC(rtn_data)))
			{
				lb_rtn = true;
			}
			else
				lb_rtn = false;

			return lb_rtn;
		}

		private int computeLRC(String str)
		{

			int LRC = 0;
			for (int i = 0; i < str.Length; i++)
				LRC = LRC ^ str[i];
			return LRC;
		}

		private bool computeLRCforByte(byte[] str)
		{
			char lc_stx = '\u0002';
			char lc_etx = '\u0003';

			int li_stx = -1;
			int li_etx = -1;

			int LRC = 0;
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] == lc_stx)
					li_stx = i;
				if (str[i] == lc_etx)
					li_etx = i;
				if (i > li_stx && li_etx < 0)
					LRC = LRC ^ str[i];

				if (i == li_etx)
				{
					LRC = LRC ^ str[li_etx];
					break;
				}
			}

			if (LRC == str[li_etx + 1])
				return true;
			else
				return false;

		}

		private bool IsWrite(string as_path)
		{
			FileIOPermission filePermss = new FileIOPermission(FileIOPermissionAccess.Write, as_path);
			try
			{
				filePermss.Demand();
				return true;
			}
			catch (SecurityException s)
			{
				return false;
			}
		}

		public bool IsReadyToShutDown
		{
			get
			{
				return (_go_SerialPort == null);
			}
		}

		private bool ProcessHasDisposed()
        {
			return _disposed;
		}

		private bool _disposed = false;
		public void Dispose()
		{
			_disposed = true;
			this.ForceToStop();

			if (_go_SerialPort != null)
			{
				Thread.Sleep(1000);

				try
				{
					_go_SerialPort?.Dispose();
				}
				catch { }

				_go_SerialPort = null;
			}

		}
		#endregion
	}

	//public class InProgressEventArgs : EventArgs
	//{
	//	public string Message { get; set; } = "Work in progress";
	//	/////public int? NewTimeRequestSec { get; set; } = null;
	//}

	enum CardProcessState
    {
		/// <summary>
		/// Going to stop transaction and allow extra card data to be received
		/// </summary>
		CancelTransaction = -1,

		/// <summary>
		/// Stop and waiting to card transaction
		/// </summary>
		EndTransaction = 0,

		/// <summary>
		/// Begining of card transaction
		/// </summary>
		Start = 1, 

		/// <summary>
		/// Processing Card data
		/// </summary>
		CardProcessing = 2
    }
}
