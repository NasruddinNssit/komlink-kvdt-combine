using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NssIT.Kiosk.Device.PAX.IM20.AccessSDK;
//////using NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base;
using NssIT.Kiosk.Log;
using NssIT.Kiosk.Log.DB;
using System.Collections.Concurrent;
using System.Net.Http;
//////using NssIT.Kiosk.Device.PAX.IM20.OrgAPI;
using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;

namespace NssIT.Kiosk.Device.PAX.IM20.PayECRApp
{
	public class PayWaveSettlement : IDisposable
	{
		private const string LogChannel = "PAX_IM20_APP";

		public delegate string[] RequestOutstandingSettlementInfo(out bool isRequestSuccessful);
		/// <summary>
		/// Return true when update DB successful.
		/// </summary>
		/// <param name="processId"></param>
		/// <param name="responseInfo"></param>
		/// <returns></returns>
		public delegate bool UpdateSettlementInfo(string processId, ResponseInfo responseInfo);

		private object _dataLock = new object();
		private ConcurrentQueue<HostInfo> _hostList = new ConcurrentQueue<HostInfo>();

		private int _hostCount = 0;
		private bool _endWorking = false;
		private bool _isBusyFlag = false;

		//private string _receiptPath = @"C:\eTicketing_Log\ECR_Receipts\";
		private string _lastProcessHostNo = "";
		private string _paywWaveCOM = "";

		private PayECRAccess _payECRAcc = null;
		private Thread _threadWorker;
		private ResponseInfo _lastResponseInfo = null;

		public string LastProcessMessage { get; private set; }

		public event EventHandler<TrxCallBackEventArgs> OnSettlementDoneCallback;

		public PayWaveSettlement(string paywWaveCOM)
		{
			_paywWaveCOM = (paywWaveCOM ?? "").Trim();

			if (_paywWaveCOM.Length == 0)
				throw new Exception("Invalid COM port specification to PayWave Settlement.");

			_threadWorker = new Thread(new ThreadStart(SettlementThreadWorking));
			_threadWorker.IsBackground = true;
			_threadWorker.Start();
		}

		private DbLog _schdLog = null;
		private DbLog Log
		{
			get => _schdLog ?? (_schdLog = DbLog.GetDbLog());
		}

		private string _lastProcessId = "--";
		private void SettlementThreadWorking()
		{
			int checkInterval = 2 * 1000;

			ResponseInfo sttRespInfo = null;
			HostInfo hsif = null;
			string lastHsno = null;
			string procId = "-";

			Log.LogText(LogChannel, procId, $@"Start settlement working", "START", "PayWaveSettlement.SettlementThreadWorking");

			while (!_endWorking)
			{
				hsif = null;
				sttRespInfo = null;

				_lastProcessHostNo = "";
				_lastResponseInfo = null;
				procId = "-";
				_lastProcessId = procId;
				//----------------------------------------------------
				// Settlement
				try
				{
					hsif = GetNextSettlementHost();
					if (hsif != null)
					{
						if (lastHsno == null)
							LastProcessMessage = $@"{DateTime.Now.ToString()} - Start Settlement for Host ({hsif.HostNo}) .." + "\r\n";
						else
							LastProcessMessage += $@"{DateTime.Now.ToString()} - Start Settlement for Host ({hsif.HostNo}) .." + "\r\n";

						procId = Guid.NewGuid().ToString();
						_lastProcessId = procId;

						lastHsno = hsif.HostNo;

						_lastProcessHostNo = hsif.HostNo;

						sttRespInfo = DoSettlement(procId, _lastProcessHostNo);

						LastProcessMessage += $@"{DateTime.Now.ToString()} - Settlement Done with Status Code : {sttRespInfo.StatusCode}; ErrMsg : {sttRespInfo.TrimErrorMsg()}" + "\r\n";
						string chkHostNo = sttRespInfo.HostNo;
						hsif.LastSettlementTime = DateTime.Now;
						hsif.LastSettlementStatus = sttRespInfo.StatusCode;
						hsif.LastSettlementErrMsg = (sttRespInfo.TrimErrorMsg().Length == 0) ? null : sttRespInfo.TrimErrorMsg();

					}
					else
					{
						lastHsno = null;
					}
				}
				catch (Exception ex)
				{
					hsif.LastSettlementTime = DateTime.Now;
					hsif.LastSettlementStatus = "99";
					hsif.LastSettlementErrMsg = $@"Error occur when Settlement. ProcId:{procId}; Error : {ex.Message}";

					LastProcessMessage += $@"{DateTime.Now.ToString()} - {hsif.LastSettlementErrMsg}" + "\r\n";
					Log.LogError(LogChannel, procId, ex, "Error when doing settlement", "PayWaveSettlement.SettlementThreadWorking");
				}
				finally
				{
					if (hsif != null)
					{
						LastProcessMessage += $@"{DateTime.Now.ToString()} - UpdateSettlementHost.." + "\r\n";
						// Below Sleep is a latency time for Machine to complete outstanding work.
						Thread.Sleep(3 * 1000);
					}
				}
				//----------------------------------------------------
				// Save Settlement Infor to Web Server
				if (sttRespInfo != null)
				{
					try
					{
						LastProcessMessage += $@"{DateTime.Now.ToString()} - Start UpdateToServer .." + "\r\n";

						if ((sttRespInfo.HostNo != null) && (sttRespInfo.HostNo.Equals("99")))
						{
							sttRespInfo.ErrorMsg = (sttRespInfo.ErrorMsg ?? "") + ";Executing System Initialization;";
							if (sttRespInfo.ErrorMsg.Length > 290)
							{
								sttRespInfo.ErrorMsg = sttRespInfo.ErrorMsg.Substring(0, 290);
							}
						}

						LastProcessMessage += $@"{DateTime.Now.ToString()} - End UpdateToServer." + "\r\n";

						hsif.LastWebTransTime = DateTime.Now;
						hsif.LastWebTransIsSuccess = true;
						hsif.LastWebTransErrMsg = null;
					}
					catch (Exception ex)
					{
						hsif.LastWebTransTime = DateTime.Now;
						hsif.LastWebTransIsSuccess = true;
						hsif.LastWebTransErrMsg = $@"Error occur when UpdateToServer. Error : {ex.Message}";

						LastProcessMessage += $@"{DateTime.Now.ToString()} - {hsif.LastWebTransErrMsg}" + "\r\n";
						Log.LogError(LogChannel, procId, ex, "Error when Update card response to DB", "PayWaveSettlement.SettlementThreadWorking");
					}
				}

				//----------------------------------------------------
				if (hsif == null)
				{
					lock (_hostList)
					{
						if (_hostList.Count == 0)
							_isBusyFlag = false;
					}
				}
			}

			Log.LogText(LogChannel, "-", "End of Settlement Working", "END", "PayWaveSettlement.SettlementThreadWorking");
		}

		public void SettleHost(string host)
		{
			if ((host ?? "").Trim().Length == 0)
				return;

			lock (_hostList)
			{
				_isBusyFlag = true;
				_hostCount++;
				_hostList.Enqueue(new HostInfo() { Inx = _hostCount, HostNo = host.Trim(), RequestToSettlement = true });

				Monitor.PulseAll(_hostList);
			}
		}

		public bool IsSystemBusy
		{
			get
			{
				return _isBusyFlag;
			}
		}

		private TimeSpan _MaxWaitPeriod = new TimeSpan(0, 0, 1);
		private HostInfo GetNextSettlementHost()
		{
			HostInfo retHsif = null;

			lock (_hostList)
			{
				if (_hostList.Count == 0)
				{
					Monitor.Wait(_hostList, _MaxWaitPeriod);
				}

				if (_hostList.TryDequeue(out retHsif))
					return retHsif;
			}
			return retHsif;
		}

		private PayECRAccess PayWaveAccs
		{
			get
			{
				return _payECRAcc ?? (_payECRAcc = NewPayECRAccess());

				PayECRAccess NewPayECRAccess()
				{
					PayECRAccess payWvAccs = PayECRAccess.GetPayECRAccess(_paywWaveCOM, PayECRAccess.SaleMaxWaitingSec); 
					payWvAccs.OnCompleteCallback += PayECROnCompleteCallBack;
					payWvAccs.OnInProgressCall += PayECRInProgressCall;
					return payWvAccs;
				}
			}
		}

		private ResponseInfo DoSettlement(string procId, string hostNo)
		{
			_lastResponseInfo = null;

			PayECRAccess payECRAcc = PayWaveAccs;

			int maxWaitSec = 2 * 60 * 60;

			Log.LogText(LogChannel, procId, $@"Start settle host {hostNo}", "Start", "PayWaveSettlement.DoSettlement");
			payECRAcc.SettlePayment(procId, hostNo);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(maxWaitSec);

			while ((_lastResponseInfo == null) && (endTime.Subtract(startTime).TotalSeconds > 0))
			{
				// Wait until finish work.
				Thread.Sleep(1000);
			}

			if (_lastResponseInfo == null)
			{
				_lastResponseInfo = new ResponseInfo() { HostNo = _lastProcessHostNo, StatusCode = "99", ErrorMsg = "Unable to do settlement at the moment." };
				// CYA-PENDING-Code - Need to add codes to handle this (_lastResponseInfo == null) situation.
			}
			Log.LogText(LogChannel, procId, $@"End settle host {hostNo}", "End", "PayWaveSettlement.DoSettlement");

			return _lastResponseInfo;
		}

		private void PayECRInProgressCall(object sender, InProgressEventArgs e)
		{
			string checkStr = e.Message;
		}

		private void PayECROnCompleteCallBack(object sender, TrxCallBackEventArgs e)
		{
			Log.LogText(LogChannel, _lastProcessId, "CYA - DEBUG::G001 - Start", "A01", "PayWaveSettlement.PayECROnCompleteCallBack", AppDecorator.Log.MessageType.Debug);
			_lastResponseInfo = e.Result;

			Log.LogText(LogChannel, _lastProcessId, "CYA - DEBUG::G002", "A02", "PayWaveSettlement.PayECROnCompleteCallBack", AppDecorator.Log.MessageType.Debug);
			if (!e.IsSuccess)
			{
				if ((_lastResponseInfo == null) && ((e.Error != null) && ((e.Error.Message ?? "").Trim().Length > 0)))
				{
					_lastResponseInfo = new ResponseInfo() { HostNo = _lastProcessHostNo, StatusCode = "99", ErrorMsg = e.Error.Message };
				}

				else if (_lastResponseInfo == null)
				{
					_lastResponseInfo = new ResponseInfo() { HostNo = _lastProcessHostNo, StatusCode = "99", ErrorMsg = "System encounter error (on card settlement) at the moment. Please try later." };
				}

				else if (string.IsNullOrWhiteSpace(_lastResponseInfo.HostNo) || (_lastResponseInfo.HostNo?.Equals("00") == true))
				{
					_lastResponseInfo.HostNo = "99";
				}
			}
			Log.LogText(LogChannel, _lastProcessId, "CYA - DEBUG::G003", "A03", "PayWaveSettlement.PayECROnCompleteCallBack", AppDecorator.Log.MessageType.Debug);
			if (OnSettlementDoneCallback != null)
			{
				TrxCallBackEventArgs ne = e.Duplicate();
				if (ne.Result == null) 
					ne.Result = _lastResponseInfo;

				ne.ProcessId = _lastProcessId;
				OnSettlementDoneCallback.Invoke(this, ne);
			}
			Log.LogText(LogChannel, _lastProcessId, "CYA - DEBUG::G004", "A04", "PayWaveSettlement.PayECROnCompleteCallBack", AppDecorator.Log.MessageType.Debug);
		}
		
		public void Dispose()
		{
			_endWorking = true;

			if (_hostList != null)
			{
				lock (_hostList)
				{
					HostInfo hsif = null;
					while (_hostList.TryDequeue(out hsif)) { string tt = "just to release item in the list"; }
					Monitor.PulseAll(_hostList);
				}
			}

			if (_payECRAcc != null)
			{
				_payECRAcc.OnCompleteCallback -= PayECROnCompleteCallBack;
				_payECRAcc.OnInProgressCall -= PayECRInProgressCall;
				//_payECRAcc.SoftShutDown();
				//_payECRAcc.Dispose();
				_payECRAcc = null;

			}
		}

		private class HostInfo
		{
			public int Inx;
			public string HostNo = null;
			public bool RequestToSettlement;
			public DateTime LastSettlementTime;
			public string LastSettlementStatus = null;
			public string LastSettlementErrMsg = null;

			public DateTime LastWebTransTime;
			public bool LastWebTransIsSuccess = false;
			public string LastWebTransErrMsg = null;

			public DateTime NextSettlementSchedule;
		}
	}
}