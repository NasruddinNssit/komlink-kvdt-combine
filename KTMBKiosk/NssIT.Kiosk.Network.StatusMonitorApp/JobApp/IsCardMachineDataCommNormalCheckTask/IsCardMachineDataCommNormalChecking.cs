using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus;
using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus.UIx;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Log.DB.StatusMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.StatusMonitorApp.JobApp.IsCardMachineDataCommNormalCheckTask
{
    /// <summary>
	/// ClassCode:EXIT65.10; Status Enum using KioskCommonStatus
	/// </summary>
    public class IsCardMachineDataCommNormalChecking : IStatusCheckingTask
    {
		private const string LogChannel = "StatusMonitor_App";

		private const int NormalIntervalCount = 5;
		private const int ErrorIntervalCount = 9;
		private const int MinimumErrorCount = 3;

		private static SemaphoreSlim _manLock = new SemaphoreSlim(1);
		private static IsCardMachineDataCommNormalChecking _auditor = null;

		private DbLog _log = null;
		private DbLog Log => (_log ?? (_log = DbLog.GetDbLog()));

		public KioskCheckingCode ThisCheckingCode { get; } = KioskCheckingCode.IsCardMachineDataCommNormal;

		/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		private int _normalReportedCount = 0;
		private int _errorReportedCount = 0;

		private KioskStatusData _lastReportedStatus = null;

		private IsCardMachineDataCommNormalChecking() { }

		public static IsCardMachineDataCommNormalChecking GetStatusChecker()
		{
			if (_auditor == null)
			{
				try
				{
					_manLock.WaitAsync().Wait();
					if (_auditor == null)
					{
						_auditor = new IsCardMachineDataCommNormalChecking();
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
			KioskCommonStatus aStatus = KioskCommonStatus.Yes;

			if (rem.Equals(StatusMonitorJob.SystemStartedTag, StringComparison.InvariantCultureIgnoreCase))
			{
				rem = $@"{rem} - App. Restarted";
				aStatus = KioskCommonStatus.Yes;
			}

			KioskStatusData stt = new KioskStatusData()
			{
				CheckingGroup = StatusCheckingGroup.CreditCard,
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

			///// Increment related counter and reset the other Counter on status changed
			if (inComingNewStatusData != null)
			{
				if (((KioskCommonStatus)inComingNewStatusData.Status) == KioskCommonStatus.No)
				{
					_normalReportedCount = 0;
					_errorReportedCount++;
				}
				else if (((KioskCommonStatus)inComingNewStatusData.Status) == KioskCommonStatus.Yes)
				{
					_normalReportedCount++;
					_errorReportedCount = 0;
				}
			}
		
			// Report 1st, 2nd and Modular Equal To Zero times for normal status 
			bool isNormalTimesToReport = ((_normalReportedCount % NormalIntervalCount) == 1) || (_normalReportedCount == 2);

			// Report error start from 2nd error status 
			//		Note : When Error accur, normally system will receive 2 status data per 1 card transaction.
			bool isErrorTimesToReport = 
				(((_errorReportedCount % ErrorIntervalCount) == 0) && (_errorReportedCount > 0)) 
				|| (_errorReportedCount == MinimumErrorCount) 
				|| (_errorReportedCount == (MinimumErrorCount + 3));

			KioskStatusData outstandingStatus = inComingNewStatusData;

			/////-----------------------------------------------------------------------------------------------------------
			///// Times to log
			if (((isNormalTimesToReport) || (isErrorTimesToReport)) && (outstandingStatus != null))
			{
				returnStatusData = outstandingStatus.Duplicate();
			}
			/////-----------------------------------------------------------------------------------------------------------
			///// Not times to log
			else
			{
				returnStatusData = null;
			}
			/////-----------------------------------------------------------------------------------------------------------
			///// Add Additional note to error remark
			if (((KioskCommonStatus)returnStatusData?.Status) == KioskCommonStatus.No)
			{
				returnStatusData.Remark = $@"{returnStatusData.Remark} (Check Count:{_errorReportedCount})";
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
			
			_lastReportedStatus = sentStatus.Duplicate();
		}
	}
}
