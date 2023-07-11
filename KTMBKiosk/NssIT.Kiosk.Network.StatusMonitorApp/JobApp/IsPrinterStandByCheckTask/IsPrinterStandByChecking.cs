using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.StatusMonitorApp.JobApp.IsPrinterStandByCheckTask
{
    /// <summary>
	/// ClassCode:EXIT65.09; Status Enum using KioskCommonStatus
	/// </summary>
    public class IsPrinterStandByChecking : IStatusCheckingTask
    {
		private const string LogChannel = "StatusMonitor_App";
		private const int ErrorPeriodDeterminationMinutes = 2;

		private static SemaphoreSlim _manLock = new SemaphoreSlim(1);
		private static IsPrinterStandByChecking _auditor = null;

		private DbLog _log = null;
		private DbLog Log => (_log ?? (_log = DbLog.GetDbLog()));

		public KioskCheckingCode ThisCheckingCode { get; } = KioskCheckingCode.IsPrinterStandBy;

		/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

		private KioskStatusData _lastStatus = null;
		private KioskStatusData _outstandingStatus = null;
		private KioskStatusData _lastReportedStatus = null;
		private KioskStatusData _firstProblemStatus = null;
		private DateTime? _nextCriticalReportTime = null;

		private IsPrinterStandByChecking() { }

		public static IsPrinterStandByChecking GetStatusChecker()
		{
			if (_auditor == null)
			{
				try
				{
					_manLock.WaitAsync().Wait();
					if (_auditor == null)
					{
						_auditor = new IsPrinterStandByChecking();
					}
					return _auditor;
				}
				finally
				{
					if (_manLock.CurrentCount == 0)
						_manLock.Release();
				}
			}
			else
				return _auditor;
		}

		public KioskStatusData GetDefaultStatus(string remark, IStatusRemark RemarkObj)
		{
			string rem = string.IsNullOrWhiteSpace(remark) ? "-Default (DF)-" : remark.Trim();

			KioskStatusData stt = new KioskStatusData()
			{
				CheckingGroup = StatusCheckingGroup.Printer,
				CheckingCode = ThisCheckingCode,
				Remark = rem,
				Status = (int)KioskCommonStatus.Yes
			};
			stt.MachineLocalTime = KioskStatusDataUniqueTime.GetNewMachineLocalTime();
			return stt;
		}

		/// <summary>
		/// Return a KioskStatusData if valid to send status data
		/// </summary>
		/// <param name="newStatusData"></param>
		/// <returns></returns>
		public KioskStatusData CheckInNewStatus(KioskStatusData newStatusData)
		{
			if (newStatusData is null)
			{
				// By Pass
			}
			else if (newStatusData.CheckingCode != ThisCheckingCode)
			{
				return null;
			}

			KioskStatusData inComingNewStatusData = newStatusData?.Duplicate();
			KioskStatusData returnStatusData = null;
			DateTime currentTime = DateTime.Now;

			bool isCriticalReportStatusTimeout = _nextCriticalReportTime.HasValue ? (_nextCriticalReportTime.Value.Ticks < currentTime.Ticks) : true;

			_outstandingStatus = (inComingNewStatusData is null) ? _outstandingStatus : inComingNewStatusData;

			/////-----------------------------------------------------------------------------------------------------------
			///// Timeout !!
			if (isCriticalReportStatusTimeout)
			{
				if (_outstandingStatus is null)
				{
					// Ignored
				}
				else
				{
					returnStatusData = _outstandingStatus.Duplicate();
				}
			}
			/////-----------------------------------------------------------------------------------------------------------
			///// Not Yet Timeout
			else
			{
				if (_lastReportedStatus is null)
				{
					if (_outstandingStatus is null)
					{
						/* Not Send */
					}
					else
					{
						returnStatusData = _outstandingStatus.Duplicate();
					}
				}

				else /* if _lastReportedStatus != null */
				{
					if (_outstandingStatus is null)
					{
						/* Not Send */
					}
					else
					{
						if (((KioskCommonStatus)_lastReportedStatus.Status) == KioskCommonStatus.Yes)
						{
							///// Note : last state YES & outstanding state NO/YES
							/* Not Send even _outstandingStatus is not nothing */
							/* If outstanding state is NO, will wait until isCriticalReportStatusTimeout in next round checking */
						}
						else /* if (((KioskCommonStatus)_lastReportedStatus.Status) == KioskCommonStatus.No) */
						{
							///// Note : last state NO & outstanding state YES
							if (((KioskCommonStatus)_outstandingStatus.Status) == KioskCommonStatus.Yes)
							{
								returnStatusData = _outstandingStatus.Duplicate();
							}

							///// Note : last state NO & outstanding state NO
							else
							{
								/* Not Send */
							}
						}
					}
				}
			}
			/////-----------------------------------------------------------------------------------------------------------
			///// Activate / Deactivate _nextCriticalReportTime
			if (_outstandingStatus != null)
			{
				///// Activate _nextCriticalReportTime if _outstandingStatus is NO
				if ((((KioskCommonStatus)_outstandingStatus.Status) == KioskCommonStatus.No) && (_nextCriticalReportTime.HasValue == false))
				{
					_nextCriticalReportTime = DateTime.Now.AddMinutes(ErrorPeriodDeterminationMinutes);
				}
				///// Deactivate _nextCriticalReportTime if _outstandingStatus is Yes
				else if (((KioskCommonStatus)_outstandingStatus.Status) == KioskCommonStatus.Yes)
				{
					_nextCriticalReportTime = null;
				}
			}
			/////-----------------------------------------------------------------------------------------------------------

			return returnStatusData;
		}

		public void ReportSentStatus(KioskStatusData[] sentStatusList)
		{
			if ((sentStatusList is null) || (sentStatusList.Length == 0))
				return;

			KioskStatusData sentStatus = (from stt in sentStatusList
										  where stt.CheckingCode == ThisCheckingCode
										  select stt).FirstOrDefault();

			if (sentStatus is null)
				return;
			/////-------------------------------------------------------------------------------------
			if (((KioskCommonStatus)sentStatus.Status) == KioskCommonStatus.No)
				_nextCriticalReportTime = DateTime.Now.AddMinutes(ErrorPeriodDeterminationMinutes);
			else
				_nextCriticalReportTime = null;

			_outstandingStatus = null;
			_lastReportedStatus = sentStatus.Duplicate();
		}
	}
}