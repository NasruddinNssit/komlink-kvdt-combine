using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Server.AccessDB.AxCommand;
using NssIT.Kiosk.Tools.ThreadMonitor;
using NssIT.Train.Kiosk.Common.Data;
using NssIT.Train.Kiosk.Common.Data.Response;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.StatusMonitorApp.JobApp
{
	/// <summary>
	/// ClassCode:EXIT65.06; Used for sending status list to KTMBWebAPI
	/// </summary>
	public class StatusMonitorStatusDispatcher
    {
		private const string LogChannel = "StatusMonitor_App";
		private List<KioskStatusData> _outstandingStatusList = new List<KioskStatusData>();
		private string _machineId = null;

		private static SemaphoreSlim _manLock = new SemaphoreSlim(1);
		private static StatusMonitorStatusDispatcher _statusDispatcher = null;

		private DbLog _log = DbLog.GetDbLog();

		public static StatusMonitorStatusDispatcher GetStatusDispatcher()
		{
			if (_statusDispatcher == null)
			{
				try
				{
					_manLock.WaitAsync().Wait();
					if (_statusDispatcher == null)
					{
						_statusDispatcher = new StatusMonitorStatusDispatcher();
						_statusDispatcher._machineId = Setting.GetSetting().KioskId;
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

		private StatusMonitorStatusDispatcher()
		{ }

		public bool DispatchStatus(List<KioskStatusData> newStatusList, bool isCleanupExistingMachineStatus)
        {
			List<KioskStatusData> dispatchStatusList = MergeOutstandingStatusList(newStatusList);

			if (DoDispatchStatus(dispatchStatusList, isCleanupExistingMachineStatus))
            {
				_outstandingStatusList.Clear();
				_outstandingStatusList = new List<KioskStatusData>();

				return true;
			}
			else
            {
				_outstandingStatusList.Clear();
				_outstandingStatusList = dispatchStatusList;

				return false;
			}

			////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
			List<KioskStatusData> MergeOutstandingStatusList(List<KioskStatusData> newStatusListX)
            {
				if (newStatusListX?.Count > 0)
				{
					//----------------------------------------------
					// Remove previous Status that is same with current Status 
					List<KioskStatusData> retList = new List<KioskStatusData>();
					foreach (KioskStatusData stt in newStatusListX)
					{
						KioskStatusData previousData = (from hisStt in _outstandingStatusList
														where hisStt.CheckingCode == stt.CheckingCode
														select hisStt).FirstOrDefault();
						if (previousData != null)
						{
							_outstandingStatusList.Remove(previousData);
						}

						retList.Add(stt.Duplicate());
					}
					//----------------------------------------------
					// Add outstanding Status to return list
					if (_outstandingStatusList.Count > 0)
					{
						foreach (var stt in _outstandingStatusList)
						{
							retList.Add(stt.Duplicate());
						}
					}
					//----------------------------------------------
					return retList;
				}
				else
				{
					List<KioskStatusData> retList = new List<KioskStatusData>();
					foreach(var stt in _outstandingStatusList)
                    {
						retList.Add(stt.Duplicate());
					}

					return retList;
				}
			}

			bool DoDispatchStatus(List<KioskStatusData> newStatusListX, bool isCleanupExistingMachineStatusX)
            {
				try
                {
					return SentStatusToWebApi("*", Guid.NewGuid(), isCleanupExistingMachineStatusX, newStatusListX.ToArray());
				}
				catch (Exception ex)
                {
					string errMsg = ex.Message;
                }
				return false;
            }
		}

		/// <summary>
		/// FuncCode:EXIT65.0607
		/// </summary>
		private bool SentStatusToWebApi(string processId, Guid? netProcessId, bool isCleanupExistingMachineStatus, KioskStatusData[] statusList)
		{
			List<KioskLatestStatusModel> latestStatusList = new List<KioskLatestStatusModel>();
			UpdateKioskStatusResult<BaseCommonObj> axResult = null;
			Exception axError = null;

			foreach (KioskStatusData stt in statusList)
			{
				KioskLatestStatusModel tmpStt = new KioskLatestStatusModel()
				{
					CheckingCode = stt.CheckingCode,
					MachineLocalDateTime = stt.MachineLocalTime,
					MachineId = _machineId,
					Remark = stt.Remark,
					RemarkType = KioskStatusRemarkType.String,
					Status = stt.Status,
					StatusName = stt.CheckingCode.GetStatusName(stt.Status),
					CheckingName = stt.CheckingCode.ToString(),
					CheckingDescription = stt.CheckingCode.GetDescription()
				};

				if (stt.RemarkObj is IStatusRemark)
				{
					tmpStt.Remark = JsonConvert.SerializeObject(stt.RemarkObj);
					tmpStt.RemarkType = KioskStatusRemarkType.JSon;
				}

				latestStatusList.Add(tmpStt);
			}

			if (latestStatusList.Count == 0)
				return false;

			AxUpSertKioskStatus axUpSert = new AxUpSertKioskStatus("*", netProcessId, latestStatusList.ToArray(), isCleanupExistingMachineStatus, new AppCallBackEvent(ExecCallBackEvent));

			IAx ax = (IAx)axUpSert;
			RunThreadMan tWorker = new RunThreadMan(new ThreadStart(ax.Execute), $@"StatusMonitorJob.SentStatusToWebApi; (EXIT60.0607.A02)", (60 + 30), LogChannel);
			tWorker.WaitUntilCompleted();

			if (axError != null)
			{
				_log?.LogError(LogChannel, "*", new WithDataException(axError.Message, axError, latestStatusList.ToArray()), "EX11", "StatusMonitorStatusDispatcher.SentStatusToWebApi");
				return false;
			}
			else if (axResult == null)
			{
				_log?.LogError(LogChannel, "*", new WithDataException("Timeout; (EXIT60.0607.A05)", new Exception("Timeout; (EXIT60.0607.A05)"), latestStatusList.ToArray()), "EX13", "StatusMonitorStatusDispatcher.SentStatusToWebApi");
				return false;
			}
			else
			{
				return true;
			}
			/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

			/// <summary>
			/// FuncCode:EXIT65.069A
			/// </summary>
			void ExecCallBackEvent(UIxKioskDataAckBase accessResult)
			{
				if (accessResult is UIxGnAppAck<UpdateKioskStatusResult<BaseCommonObj>> uIxResult)
				{
					if (uIxResult.IsDataReadSuccess)
					{
						axResult = uIxResult.Data;
					}
					else
					{
						if (uIxResult.Error != null)
							axError = uIxResult.Error;
						else
							axError = new Exception("Unknown access error in StatusMonitorStatusDispatcher.ExecCallBackEvent; (EXIT65.069A.X01)");
					}
				}
			}
		}
	}
}
