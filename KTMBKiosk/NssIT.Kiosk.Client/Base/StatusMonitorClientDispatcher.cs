using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Log.DB.StatusMonitor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Base
{
	/// <summary>
	/// ClassCode:EXIT80.10
	/// </summary>
	public class StatusMonitorClientDispatcher : IDisposable
	{
		private const string _repeatedTag = "(Repeated)";
		private const string LogChannel = "StatusMonitor_Client";

		private Thread _dispatcherThread = null;

		private static SemaphoreSlim _manLock = new SemaphoreSlim(1);
		private static StatusMonitorClientDispatcher _statusDispatcher = null;
		private DbLog _log = null;
		private StatusHub _statusHub = null;

		public static StatusMonitorClientDispatcher GetDispatcher()
		{
			if (_statusDispatcher == null)
			{
				try
				{
					_manLock.WaitAsync().Wait();
					if (_statusDispatcher == null)
					{
						_statusDispatcher = new StatusMonitorClientDispatcher();
					}
					return _statusDispatcher;
				}
				finally
				{
					if (_manLock.CurrentCount == 0)
						_manLock.Release();
				}
			}
			else
				return _statusDispatcher;
		}

		private StatusMonitorClientDispatcher()
		{
			_log = DbLog.GetDbLog();
			_statusHub = StatusHub.GetStatusHub();
			Init();
		}

		private void Init()
		{
			try
			{
				_dispatcherThread = new Thread(StatusDispatcherThreadWorking);
				_dispatcherThread.IsBackground = true;
				_dispatcherThread.Start();
			}
			catch (Exception ex)
			{
				string byPassMsg = ex.Message;
			}
		}

		/// <summary>
		/// FuncCode:EXIT80.1005
		/// </summary>
		private void StatusDispatcherThreadWorking()
		{
			KioskStatusData inforMsg = null;

			///// Default Status Configuration (Default Status means UI Display Status)
			int sendDefaultStatusIntervalMinute = 4;
			KioskStatusData lastSentDefaultStatus = null;               /* Last Already Sent Default Status; Default Status means UI Display Status */
			KioskStatusData lastOutstandingDefaultStatus = null;        /* Last Outstanding Default Status; Default Status means UI Display Status  */
			DateTime nextReportPeriodicalDefaultStatusTime = DateTime.Now.AddMinutes(sendDefaultStatusIntervalMinute);
			/////---------------------------------------------------------------------------------------------------------

			while (_disposed == false)
			{
				try
				{
					inforMsg = _statusHub.GetNextStatusInfo();

					/////--------------------------------------------------------------------------------------------------------------------
					///// Create new Default Status upon expired of next periodical time
					if ((inforMsg is null) && (nextReportPeriodicalDefaultStatusTime.Ticks < DateTime.Now.Ticks))
                    {
						if (GetPermissionSendingDefaultStatus(timeExpired: true, lastSentDefaultStatus, lastOutstandingDefaultStatus, out KioskStatusData sendAbleStatusData) == true)
                        {
							inforMsg = sendAbleStatusData;
						}
					}

					///// Check Permission to send Default Status even not yet timeout
					else if ((inforMsg != null) && (inforMsg.CheckingCode == KioskCheckingCode.IsUIDisplayNormal))
					{
						if (GetPermissionSendingDefaultStatus(timeExpired: false, lastSentDefaultStatus, inforMsg, out KioskStatusData sendAbleStatusData) == true)
						{
							inforMsg = sendAbleStatusData;
						}
						else
                        {
							lastOutstandingDefaultStatus = inforMsg.Duplicate();
							inforMsg = null;
						}
					}
					/////--------------------------------------------------------------------------------------------------------------------
					///// Send Status to Local Server
					if (inforMsg != null)
					{
						if ((SendToLocalServer(inforMsg) == true) && (inforMsg.CheckingCode == KioskCheckingCode.IsUIDisplayNormal))
                        {
							lastOutstandingDefaultStatus = null;
							lastSentDefaultStatus = inforMsg.Duplicate();
							nextReportPeriodicalDefaultStatusTime = DateTime.Now.AddMinutes(sendDefaultStatusIntervalMinute);
						}
					}
					else
                    {
						Thread.Sleep(10 * 1000);
                    }
					/////--------------------------------------------------------------------------------------------------------------------
				}
				catch (Exception ex)
				{
					_log?.LogError(LogChannel, "*", new WithDataException(ex.Message, ex, inforMsg.Duplicate()), "EX01", "StatusMonitorClientDispatcher.StatusDispatcherThreadWorking");
				}
			}

			return;
			/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

			/// <summary>
			/// FuncCode:EXIT80.108A
			/// </summary>
			bool SendToLocalServer(KioskStatusData msg)
			{
				bool isSuccess = false;
				try
				{
					App.NetClientSvc?.StatusMonitorService?.SendStatusRequest(msg.Duplicate());
					isSuccess = true;
				}
				catch (Exception ex)
				{
					_log?.LogError(LogChannel, "*", new WithDataException(ex.Message, ex, msg.Duplicate()), "EX01", "StatusMonitorClientDispatcher.StatusDispatcherThreadWorking");
				}

				return isSuccess;
			}
		}

		private bool _disposed = false;
		public void Dispose()
		{
			if (_disposed == false)
			{
				_disposed = true;
				_log = null;
			}
		}

		private bool GetPermissionSendingDefaultStatus(bool timeExpired, KioskStatusData lastSentDefaultStatus, KioskStatusData lastOutstandingDefaultStatus, out KioskStatusData sendAbleStatusData)
        {
			sendAbleStatusData = null;
			bool retPermission = false;

			if (timeExpired)
            {
				retPermission = true;
				if ((lastOutstandingDefaultStatus is null) && (lastSentDefaultStatus is null))
				{
					sendAbleStatusData = new KioskStatusData()
					{
						CheckingGroup = StatusCheckingGroup.BasicKioskClient,
						CheckingCode = KioskCheckingCode.IsUIDisplayNormal,
						Remark = "Abnormal UI status condition; UI is working but fail to report status",
						Status = (int)KioskCommonStatus.No
					};
					sendAbleStatusData.MachineLocalTime = KioskStatusDataUniqueTime.GetNewMachineLocalTime();
				}
				else if ((lastOutstandingDefaultStatus is null) && (lastSentDefaultStatus != null))
				{
					sendAbleStatusData = lastSentDefaultStatus.Duplicate();
					sendAbleStatusData.MachineLocalTime = KioskStatusDataUniqueTime.GetNewMachineLocalTime();

					if ((lastSentDefaultStatus.RemarkObj is null) && ((string.IsNullOrWhiteSpace(lastSentDefaultStatus.Remark)) || (lastSentDefaultStatus.Remark?.Contains(_repeatedTag) == false)))
					{
						string remark = string.IsNullOrWhiteSpace(lastSentDefaultStatus.Remark) ? $@"{_repeatedTag}" : $@"{lastSentDefaultStatus.Remark}; {_repeatedTag}";
						sendAbleStatusData.Remark = remark;
					}
				}
				else if ((lastOutstandingDefaultStatus != null) && (lastSentDefaultStatus is null))
				{
					sendAbleStatusData = lastOutstandingDefaultStatus.Duplicate();
				}
				else /* if ((lastOutstandingDefaultStatus != null) && (lastSentDefaultStatus != null)) */
				{
					sendAbleStatusData = lastOutstandingDefaultStatus.Duplicate();
				}
			}
			else /* if (timeExpired == false) */
			{
				// Note : When timeExpired is false, possible send only when lastOutstandingDefaultStatus is not null.

				if (lastOutstandingDefaultStatus is null)
				{
					retPermission = false;
				}
				else /* if (lastOutstandingDefaultStatus != null) */
				{
					if (lastSentDefaultStatus != null)
                    {
						if ((((KioskCommonStatus)lastSentDefaultStatus.Status) == KioskCommonStatus.No)
							&&
							(((KioskCommonStatus)lastOutstandingDefaultStatus.Status) == KioskCommonStatus.No)
							)
                        {
							retPermission = true;
							sendAbleStatusData = lastOutstandingDefaultStatus.Duplicate();
						}
						else if ((((KioskCommonStatus)lastSentDefaultStatus.Status) == KioskCommonStatus.No)
							&&
							(((KioskCommonStatus)lastOutstandingDefaultStatus.Status) == KioskCommonStatus.Yes)
							)
						{
							retPermission = true;
							sendAbleStatusData = lastOutstandingDefaultStatus.Duplicate();
						}
						else if ((((KioskCommonStatus)lastSentDefaultStatus.Status) == KioskCommonStatus.Yes)
							&&
							(((KioskCommonStatus)lastOutstandingDefaultStatus.Status) == KioskCommonStatus.No)
							)
						{
							retPermission = true;
							sendAbleStatusData = lastOutstandingDefaultStatus.Duplicate();
						}
						else /* (lastSentDefaultStatus.Status == KioskCommonStatus.Yes) && (lastOutstandingDefaultStatus.Status == KioskCommonStatus.Yes) */
						{
							retPermission = false;
						}
                    }
					else /* if (lastSentDefaultStatus is null) */
					{
						retPermission = true;
						sendAbleStatusData = lastOutstandingDefaultStatus.Duplicate();
					}
				}
			}

			return retPermission;
		}
	}
}
