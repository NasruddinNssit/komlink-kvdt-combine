using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using NssIT.Kiosk.Device.PAX.IM20.AccessSDK;
using NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base;
using NssIT.Kiosk.Log;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Log.DB.StatusMonitor;

namespace NssIT.Kiosk.Device.PAX.IM30.IM30PayApp
{
	public class PayWaveSettlementScheduler : IDisposable
	{
        private const string LogChannel = "IM30_APP";

        private string _paywWaveCOM = "";
		private string _procIdPreFix = null;
		private string _dailyMaintenanceTime = "00:00";

		private bool _isValidSchedule = false;

		//
		// (2021-03-19) private int _settlementPeriodHours = 4; // Valid time period (in hours) for carring out settlement procedure after the time of maintenance (_dailyMaintenanceTime).
		//CYA-TEST private int _settlementPeriodHours = 1;
		//-----------------------------------------

		private static CultureInfo _dateProvider = CultureInfo.InvariantCulture;
		private DateTime? _nextSettleTime = null;
		private int _failRetryDelay = 10;       /* in seconds */
		private bool _hasPermissionToDoSettlement = false;
		private bool _rebootRequestAfterSettlement = false;
		private int _settlementCount = 0;

		private Thread _worker;

		public event EventHandler<SettlementDoneEventArgs> OnSettlementDone;
		public event EventHandler OnRequestSettlement;

		private PayWaveSettlement.RequestOutstandingSettlementInfo _requestOutstandingSettlementInfoHandle = null;
		private PayWaveSettlement.UpdateSettlementInfo _updateSettlementInfoHandle = null;

		public PayWaveSettlementScheduler(string paywWaveCOM, string settlementTimeString)
		{
			// Set to Default
			_isValidSchedule = true;
			_procIdPreFix = DateTime.Now.ToString("yyyyMMddHHmm") + "-";
			//-------------------------------------

			_paywWaveCOM = (paywWaveCOM ?? "").Trim();
			_dailyMaintenanceTime = settlementTimeString;
			_nextSettleTime = GetNextSettlementTime(null);			
			
			if (_paywWaveCOM.Length == 0)
				throw new Exception("Error in PayWaveSettlementScheduler; Invalid COM port specification to PayWave Settlement.");
		}

		public void Load(PayWaveSettlement.RequestOutstandingSettlementInfo requestOutstandingSettlementInfoHandle,
			PayWaveSettlement.UpdateSettlementInfo updateSettlementInfoHandle)
        {
			_requestOutstandingSettlementInfoHandle = requestOutstandingSettlementInfoHandle;
			_updateSettlementInfoHandle = updateSettlementInfoHandle;

			NssIT.Kiosk.AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

			if (setting.NoCardSettlement == false)
            {
				_worker = new Thread(new ThreadStart(SchecduleThreadWorking));
				_worker.IsBackground = true;
				_worker.Start();
			}			
		}

		private DbLog _schdLog = null;
		private DbLog Log
		{
			get => _schdLog ?? (_schdLog = DbLog.GetDbLog());
		}

		/// <summary>
		/// </summary>
		/// <param name="justFinishedSettlement">To indicate system has just finished settlement process base on the _nextSettleTime.</param>
		public void UpdateSchedule(bool justFinishedSettlement = false)
        {
			DateTime currentTime = DateTime.Now;
			//Expire time Checking for Last Settlement DateTime
			if (_nextSettleTime.HasValue == false)
			{
				_nextSettleTime = GetNextSettlementTime(null);
				Log.LogText(LogChannel, "*", $@"Created Next Settlement Time : {_nextSettleTime.Value.ToString("yyyy/MM/dd HH:mm")}", "A02"
					, "PayWaveSettlementScheduler.UpdateSchedule2");
			}
			else if ((justFinishedSettlement == false) && (CheckIsValidTimeForSettlement(currentTime, _nextSettleTime.Value, out _, out _) == true))
            {
				/* By Pass */
            }
			else
			{
				Log.LogText(LogChannel, "*", $@"Create Next Settlement Time Base on Last Settlement Time; Last Settlement Time : {_nextSettleTime.Value.ToString("yyyy/MM/dd HH:mm")}", "A03"
					, "PayWaveSettlementScheduler.UpdateSchedule2");

				DateTime nextSettDateTime = GetNextSettlementTime(_nextSettleTime.Value);
				_nextSettleTime = nextSettDateTime;

				Log.LogText(LogChannel, "*", $@"New Settlement Time : {_nextSettleTime.Value.ToString("yyyy/MM/dd HH:mm")}", "A04"
					, "PayWaveSettlementScheduler.UpdateSchedule2");
			}
		}

		private DateTime GetNextSettlementTime(DateTime? lastSettlement)
		{
			CultureInfo provider = CultureInfo.InvariantCulture;

			DateTime nextSettleTime;

			if (lastSettlement.HasValue == false)
				nextSettleTime = DateTime.Now;
			else
				nextSettleTime = GetNextDaySettlementTime();

			return nextSettleTime;

			//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

			DateTime GetNextDaySettlementTime()
			{
				DateTime currentTime = DateTime.Now;
				DateTime resultTime = currentTime;

				int advanceDayCount = 0;
				do
				{
					DateTime nextDay = currentTime.AddDays(advanceDayCount);
					string dateStr = $@"{nextDay.ToString("yyyy/MM/dd")} {_dailyMaintenanceTime}";
					resultTime = DateTime.ParseExact(dateStr, "yyyy/MM/dd HH:mm", provider);
					advanceDayCount++;
				} while (resultTime.Subtract(DateTime.Now).TotalSeconds <= 0);

				return resultTime;
			}
		}

		/// <summary>
		/// When return false, mean the checkSettlementTime is lagging currentTime and is not valid time for Settlement.
		/// </summary>
		/// <param name="currentTime"></param>
		/// <param name="checkSettlementTime"></param>
		/// <param name="isTimeForSettlement"></param>
		/// <returns></returns>
		private bool CheckIsValidTimeForSettlement(DateTime currentTime, DateTime checkSettlementTime, out bool isTimeForSettlement, out bool isSettlementTimeUnreachable)
		{
			isTimeForSettlement = false;
			isSettlementTimeUnreachable = false;

			// checkSettlementTime is lagging current time ..
			if (checkSettlementTime.Subtract(currentTime).TotalSeconds < 0)
			{
				isTimeForSettlement = true;
				return true;

				//// Start from 2021-03-18 ----- Everyday must do settlement.
				////// checkSettlementTime is lagging but within _maintenancePeriodHours
				////if (currentTime.Subtract(checkSettlementTime).TotalHours < _settlementPeriodHours)
				////{
				////	isTimeForSettlement = true;
				////	return true;
				////}
				////// checkSettlementTime is lagging and outside _maintenancePeriodHours
				////else
				////{
				////	isSettlementTimeUnreachable = true;
				////	return false;
				////}
			}

			// checkSettlementTime is leading current time ..
			else
			{
				return true;
			}
		}

        private object _schecduleWorkingLock = new object();
		private bool _startScheduleWorking = false;

		private long _processInx = 0;
		private List<OutstandingHostList> _failHostList = new List<OutstandingHostList>();
		private bool _isScheduleWorkingContinue = true;
		private void SchecduleThreadWorking()
		{
			//int minScheduleDelay = 5 * 1000;
			//int waitTofinishDelay = 5 * 1000;
			//int reDoDelay = _failRetryDelay * 1000;

			//Below allow system to stablelized before start schedule.
			Thread.Sleep(3 * 1000);
			/////

			DateTime? startTime = null;
			int failCycleCount = 0;
			int minScheduleDelay = 3 * 1000;
			int waitTofinishDelay = 3 * 1000;
			int reDoDelay = _failRetryDelay * 1000;
			int numberOfHost = 0;

			DateTime? lastRequestSettlementTime = null;
			DateTime? lastWaitSettlementTime = null;
			string procId = "-";

			while (_isScheduleWorkingContinue)
			{
				if (!_isValidSchedule)
				{
					_isScheduleWorkingContinue = false;
					Log.LogText(LogChannel, "-", "Invalid Paywave Schedule", "A05", "PayWaveSettlementScheduler.SchecduleWorking", AppDecorator.Log.MessageType.Fatal);
					break;
				}

				try
				{
					//----- Start --------------------------------
					if (_hasPermissionToDoSettlement 
						/* && IsTimeForSettlement() */
						)
					{
						_rebootRequestAfterSettlement = false;

						if (startTime.HasValue == false)
						{
							startTime = DateTime.Now;
							StatusHub.GetStatusHub().zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, $@"Settlement in progress; Start Time: {startTime:yyyy/MM/dd HH:mm:ss}");
							StatusHub.GetStatusHub().zNewStatus_IsCreditCardSettlementDone(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, $@"Start Settlement; Settlement in progress; Time: {startTime:yyyy/MM/dd HH:mm:ss}");
						}

						_startScheduleWorking = true;
						_processInx += 1;
						procId = $@"{_procIdPreFix}{_processInx.ToString()}";

						Log.LogText(LogChannel, procId, "Start Settlement", "Init");

						PayWaveSettlement payWaveSettlement = null;
						try
						{
							if (_settlementCount == 0)
							{
								//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
								// Below coding block is used to allow IM20 to have enough time to init the reader before start. 
								Thread.Sleep(3 * 1000);
								//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
							}

							payWaveSettlement = new PayWaveSettlement(_paywWaveCOM);
							payWaveSettlement.OnSettlementDoneCallback += SettlementCompletedCallBack;

							List<string> outStandingList = null;

							// Get Outstanding Hosts
							if (_failHostList?.Count > 0)
							{
								Log.LogText(LogChannel, procId, $@"Fail host count ({_failHostList.Count.ToString()})", "FailSettlementHostFound");

								// Get from previous fail list
								outStandingList = new List<string>();
								foreach (OutstandingHostList outH in _failHostList)
								{
									outStandingList.Add(outH.Host);
									Log.LogText(LogChannel, procId, $@"Add Fail host:{outH.Host}", "FailSettlementHostFound");
								}

								if (failCycleCount > 3)
                                {
									if (failCycleCount <= 5)
										StatusHub.GetStatusHub().zNewStatus_IsCreditCardSettlementDone(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, $@"Fail Settlement. Time: {startTime:yyyy/MM/dd HH:mm:ss}; Loop Count: {failCycleCount}");
									
									else if ((failCycleCount % 10) == 0)
										StatusHub.GetStatusHub().zNewStatus_IsCreditCardSettlementDone(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, $@"Fail Settlement. Time: {startTime:yyyy/MM/dd HH:mm:ss}; Loop Count: {failCycleCount}");
								}

								failCycleCount++;
								_failHostList.Clear();
							}
							else
							{
								//-------------------------------------------------------------------------------------------------------------------------
								// Below code used to add redundant host in settlement initially for temporarey remove the "Access to the COM port is denied" issue.
								outStandingList = new List<string>() { "00", "00", "00", "00" };

								//if (_settlementCount == 0)
								//{
								//	outStandingList.Add("00");
								//	outStandingList.Add("00");
								//	outStandingList.Add("00");
								//	outStandingList.Add("00");
								//}
								numberOfHost = 4;

								Log.LogText(LogChannel, procId, $@"Host count:{outStandingList.Count.ToString()}", "OutStandingSettlementHostFound");
							}

							_settlementCount++;

							// Send Outstanding Host for settlement 
							bool hasRequestSettlement = false;
							foreach (string host in outStandingList)
							{
								Log.LogText(LogChannel, procId, $@"Send Host {host} for settlement.", "StartHostSettlement");
								payWaveSettlement.SettleHost(host);
								Log.LogText(LogChannel, procId, $@"End Host ({host}) settlement.", "EndHostSettlement");
								hasRequestSettlement = true;
							}

							// Wait for all settlement working to finish.
							//lastWaitSettlementTime
							if (hasRequestSettlement)
							{
								while (payWaveSettlement.IsSystemBusy)
								{
									PeriodicLogWrite(procId, $@"Wait Settlement for all host to finish. Sleep period{waitTofinishDelay.ToString()}.", 
										"WaitingSettlementResult", 
										ref lastWaitSettlementTime, 30);
									Thread.Sleep(waitTofinishDelay);
								}
							}
						}
						finally
						{
							if (payWaveSettlement != null)
							{
								payWaveSettlement.OnSettlementDoneCallback -= SettlementCompletedCallBack;
								payWaveSettlement.Dispose();
								payWaveSettlement = null;
							}
						}
					}
					else if (IsTimeForSettlement(out bool isSettlementTimeUnreachable))
					{
						startTime = null;
						failCycleCount = 0;
						_processInx += 1;
						procId = $@"{_procIdPreFix}{_processInx.ToString()}";
						OnRequestSettlement?.Invoke(this, new EventArgs());
						PeriodicLogWrite(procId, "Requesting Settlement", "-", ref lastRequestSettlementTime, 30);
					}
                    else if (isSettlementTimeUnreachable == true)
                    {
						startTime = null;
						failCycleCount = 0;
						UpdateSchedule(justFinishedSettlement: false);
					}
				}
				catch (Exception ex)
				{
					Log.LogError(LogChannel, procId, ex, "S01", "PayWaveSettlementScheduler.SchecduleWorking");
				}
				finally
				{
					if (_startScheduleWorking)
					{
						// if has fail outstanding host wait for "reDoDelay" in seconds to restart again
						if (_failHostList.Count > 0)
							Thread.Sleep(reDoDelay);

						// Success .. plan for the next settlement time
						else
						{
							// Reset _hasPermissionToDoSettlement flag after finish all settlement.
							_hasPermissionToDoSettlement = false;
							failCycleCount = 0;

							lock (_schecduleWorkingLock)
							{
								UpdateSchedule(justFinishedSettlement: true);
								Log.LogText(LogChannel, procId, $@"Next Settlement Time : {_nextSettleTime.Value.ToString("yyyy/MM/dd HH:mm:ss")}", "B01", "PayWaveSettlementScheduler.SchecduleWorking");
							}

							//Delay disabled because Revenue do not need this. "Wait 3 minutes to allow IM20 to Update Terminal Application (download latest app from Paysys) after finished settlement"
							/////Thread.Sleep(3 * 60 * 1000);
                            Thread.Sleep(20 * 1000);

                            StatusHub.GetStatusHub().zNewStatus_IsCreditCardSettlementDone(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, $@"End Settlement; Number of Host: {numberOfHost}; Time: {startTime:yyyy/MM/dd HH:mm:ss}");

							OnSettlementDone?.Invoke(this, new SettlementDoneEventArgs(_rebootRequestAfterSettlement));
							//CYA-TEST .. Overwrite for debug.. //_nextSettleTime = DateTime.Now.AddSeconds(30); //---------------------------------------------------
							_startScheduleWorking = false;
							startTime = null;
							numberOfHost = 0;
						}
					}
					// Sleep and wait for next settlement Schedule
					if ((_nextSettleTime.HasValue) && (_nextSettleTime.Value.Subtract(DateTime.Now).TotalHours > 1))
					{
						// If Next schedule time more than 1 hour, then sleep 10 minutes.
						Thread.Sleep(10 * 60 * 1000);
						//CYA-TEST -- Thread.Sleep(30 * 1000);
					}
					else
					{
						Thread.Sleep(minScheduleDelay);
						//CYA-TEST -- Thread.Sleep(20 * 1000);
					}
				}
			}
		}

		private void PeriodicLogWrite(string processId, string msg, string refLocationTag, ref DateTime? lastLogTime, int intervalSec)
		{
			if ((!lastLogTime.HasValue) || (lastLogTime.Value.AddSeconds(intervalSec).Subtract(DateTime.Now).TotalSeconds <= 0))
			{
				Log.LogText(LogChannel, processId, msg, refLocationTag, "PayWaveSettlementScheduler.PeriodicLogWrite");
				lastLogTime = DateTime.Now;
			}
		}

		private void SettlementCompletedCallBack(object sender, TrxCallBackEventArgs e)
		{
			if (!e.IsSuccess)
			{
				if (e.Result.StatusCode.Equals(ResponseCodeDef.TransactionNotAvailable))
				{
                    Log.LogText(LogChannel, e.ProcessId, $@"Start - UpdateSettlementInfoHandle;", "B01", "PayWaveSettlementScheduler.SettlementCompletedCallBack");
                    bool isUpdateSuccess = _updateSettlementInfoHandle(e.ProcessId, e.Result);
                    Log.LogText(LogChannel, e.ProcessId, $@"End  - UpdateSettlementInfoHandle; isUpdateSuccess : {isUpdateSuccess}", "B02", "PayWaveSettlementScheduler.SettlementCompletedCallBack");
                }
				else
				{
                    string failMsg = "";

                    if (e.Result.TrimErrorMsg().Length > 0)
                        failMsg = e.Result.TrimErrorMsg();
                    else
                        failMsg = e.Error.Message ?? "Unknown error";

                    _failHostList.Add(new OutstandingHostList() { Host = e.Result.HostNo, LastFailSettlementTime = DateTime.Now, SuccessSettlement = false, LastFailSettlementMsg = failMsg });
                }
			}
			else
            {
				// Save To Database
				Log.LogText(LogChannel, e.ProcessId, $@"Start - UpdateSettlementInfoHandle;", "B01", "PayWaveSettlementScheduler.SettlementCompletedCallBack");
				bool isUpdateSuccess = _updateSettlementInfoHandle(e.ProcessId, e.Result);
				Log.LogText(LogChannel, e.ProcessId, $@"End  - UpdateSettlementInfoHandle; isUpdateSuccess : {isUpdateSuccess}", "B02", "PayWaveSettlementScheduler.SettlementCompletedCallBack");
			}
		}

		public void AgreeForSettlement()
		{
			_hasPermissionToDoSettlement = true;
		}

		public bool IsTimeForSettlement(out bool isSettlementTimeUnreachable)
		{
			isSettlementTimeUnreachable = false;

			DateTime currentTime = DateTime.Now;

			if (CheckIsValidTimeForSettlement(currentTime, _nextSettleTime.Value, out bool isTimeForSettlement, out bool isSettlementTimeUnreachableX))
			{
				isSettlementTimeUnreachable = isSettlementTimeUnreachableX;
				return isTimeForSettlement;
			}
			else
			{
				isSettlementTimeUnreachable = isSettlementTimeUnreachableX;
				return false;
			}
		}

		public string NextSettlementTimeString
        {
            get
            {
				string retStr = "-";
				try
				{
					if (_nextSettleTime.HasValue)
                    {
						DateTime dt = _nextSettleTime.Value;

						retStr = "ST" + dt.ToString("yyyyMMdd-HHmm");
					}
					else
						retStr = "-";
				}
				catch 
				{
					retStr = "ERR";
				}

				return retStr;
			}
        }

		public void Dispose()
		{
			_isScheduleWorkingContinue = false;
			_requestOutstandingSettlementInfoHandle = null;
			_updateSettlementInfoHandle = null;
		}

		private class OutstandingHostList
		{
			public string Host = null;
			public bool? SuccessSettlement = null;
			public string LastFailSettlementMsg = null;
			public DateTime? LastFailSettlementTime = null;
		}
	}
}