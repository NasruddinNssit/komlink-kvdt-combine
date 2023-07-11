using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus;
using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus.UIx;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Log.DB.StatusMonitor;
using NssIT.Kiosk.Network.StatusMonitorApp.JobApp.IsUIDisplayNormalCheckTask;
using NssIT.Kiosk.Server.AccessDB.AxCommand;
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
	/// ClassCode:EXIT65.03
	/// </summary>
	public class StatusMonitorJob : IDisposable 
	{
		public const string SystemStartedTag = "System Started";

		private const string LogChannel = "StatusMonitor_App";

		private StatusMonitorCheckerCollection _statusMonitorCheckerCollection = null;

        private Thread _logThread = null;
		private DbLog _log = null;
		private DbLog Log => (_log ?? (_log = DbLog.GetDbLog()));

		private static SemaphoreSlim _manLock = new SemaphoreSlim(1);
        private static StatusMonitorJob _monitorJob = null;
		private StatusMonitorStatusDispatcher _statusDispatcher = StatusMonitorStatusDispatcher.GetStatusDispatcher();

		private string _machineId = null;
		
		private StatusHub _statusHub = null;

		//------------------------------------------------------------------------
		// Default Status is refer to IsUIDisplayNormalChecking
		private IsUIDisplayNormalChecking _defaultStatusChecker = null;
		//------------------------------------------------------------------------

		/// <summary>
		/// FuncCode:EXIT65.0302
		/// </summary>
		private StatusMonitorJob()
		{
			_statusHub = StatusHub.GetStatusHub();
			_statusMonitorCheckerCollection = StatusMonitorCheckerCollection.GetStatusMonitorCheckerCollection();
			_machineId = Setting.GetSetting().KioskId;
			Init();
		}

		private bool _disposed = false;

		/// <summary>
		/// FuncCode:EXIT65.0399
		/// </summary>
		public void Dispose()
		{
			if (_disposed == false)
			{
				_disposed = true;
				_statusHub = null;
				_statusDispatcher = null;
			}
		}

		/// <summary>
		/// FuncCode:EXIT65.0303
		/// </summary>
		public static StatusMonitorJob GetStatusMonitorJob()
		{
			if (_monitorJob == null)
			{
				try
				{
					_manLock.WaitAsync().Wait();
					if (_monitorJob == null)
					{
						_monitorJob = new StatusMonitorJob();
					}
					return _monitorJob;
				}
				finally
				{
					if (_manLock.CurrentCount == 0)
						_manLock.Release();
				}
			}
			else
				return _monitorJob;
		}

		/// <summary>
		/// FuncCode:EXIT65.0304
		/// </summary>
		public void SetupCheckingTask(KioskCheckingCode checkCode, IStatusCheckingTask checkingTask)
        {
			_statusMonitorCheckerCollection?.SetupCheckingTask(checkCode, checkingTask);

			if (checkingTask is IsUIDisplayNormalChecking task1)
				_defaultStatusChecker = task1;
		}

		/// <summary>
		/// FuncCode:EXIT65.0305
		/// </summary>
		private void Init()
		{
			try
			{
				_logThread = new Thread(ManageStatusThreadWorking);
				_logThread.IsBackground = true;
				_logThread.Start();
			}
			catch (Exception ex)
			{
				string byPassMsg = ex.Message;
			}
		}

		/// <summary>
		/// FuncCode:EXIT65.0306
		/// </summary>
		private void ManageStatusThreadWorking()
		{
			// Bellow Sleep allow _statusMonitorTaskList to fill in the items.
			Thread.Sleep(10 * 1000);

			KioskStatusData statusData = null;
			List<KioskStatusData> statusResultList = new List<KioskStatusData>();
			bool isStartUp = true;
			DateTime nowTime = DateTime.Now;
			bool cleanupExistingMachineStatus = true;

			while (_disposed == false)
			{
				try
				{
					nowTime = DateTime.Now;

					if (_defaultStatusChecker == null)
                    {
						Thread.Sleep(10 * 1000);
						continue;
					}

					//-------------------------------------------------------------------------------------------------------------
					//Send all Default Status to KTMBCTS for first time after Local Service has started. 
					else if (isStartUp)
                    {
						IStatusCheckingTask[] allTaskList = _statusMonitorCheckerCollection.GetAllStatusChecker();
						foreach(IStatusCheckingTask tk in allTaskList)
                        {
							KioskStatusData tmpStatus = tk.GetDefaultStatus(SystemStartedTag, null);
							if (tmpStatus != null)
								statusResultList = AppendStatusResultList(tmpStatus.Duplicate(), statusResultList);
						}

						isStartUp = false;
						continue;
					}

					statusData = null;
					statusData = _statusHub?.GetNextStatusInfo();

					//-------------------------------------------------------------------------------------------------------------
					// Status Checking
					if (statusData != null)
					{
						KioskStatusData result = null;

						if (_statusMonitorCheckerCollection?.GetStatusChecker(statusData.CheckingCode) is IStatusCheckingTask checkingTask)
                        {
							result = checkingTask.CheckInNewStatus(statusData);
						}

						//---------------------------------------------------------------------------
						// Collect Status into a result list.
						if (result != null)
						{
							statusResultList = AppendStatusResultList(result, statusResultList);
						}
						//---------------------------------------------------------------------------
					}

					//-------------------------------------------------------------------------------------------------------------
					// Send Status to WebAPI
					else if (_disposed == false)
                    {
						nowTime = DateTime.Now;
						//---------------------------------------------------------------------------
						// Get All Outstanding Status
						if (statusResultList.Count == 0)
                        {
							KioskStatusData[] outStdArr = _statusMonitorCheckerCollection.GetAllOustandingStatus();

							foreach (KioskStatusData tmpStt in outStdArr)
							{
								statusResultList = AppendStatusResultList(tmpStt, statusResultList);
							}
						}
						//---------------------------------------------------------------------------
						// Send Statuses to KtmbWebApi
						if (statusResultList.Count > 0)
                        {
							try
                            {
								if (_statusDispatcher?.DispatchStatus(statusResultList, cleanupExistingMachineStatus) == true)
                                {
									_statusMonitorCheckerCollection.ReportSentStatusList(statusResultList.ToArray());

									cleanupExistingMachineStatus = false;

									Log.LogText(LogChannel, "*", statusResultList.ToArray(), "B01", "StatusMonitorJob.ManageStatusThreadWorking");
								}
								else if (_statusDispatcher != null)
                                {
									Log.LogError(LogChannel, "*", new WithDataException("Error!!", new Exception("Fail to send status to WebAPI"), statusResultList.ToArray()) , "X21", "StatusMonitorJob.ManageStatusThreadWorking");
								}
							}
							catch (Exception ex)
                            {
								Log.LogError(LogChannel, "*", new WithDataException(ex.Message, ex, statusResultList.ToArray()) , "EX11", "StatusMonitorJob.ManageStatusThreadWorking");
							}
                            finally
                            {
								statusResultList.Clear();
							}
						}
						//---------------------------------------------------------------------------
						Thread.Sleep(10 * 1000);
					}
				}
				catch (Exception ex)
				{
					string byPassMsg = ex.Message;
				}
			}

			return;
			/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

			List<KioskStatusData> AppendStatusResultList(KioskStatusData statusDataX, List<KioskStatusData> statusResultListX)
            {
				if (statusDataX is null)
					return statusResultListX;

				KioskStatusData previousResultX = (from prvStt in statusResultListX
												   where prvStt.CheckingCode == statusDataX.CheckingCode
												  select prvStt).FirstOrDefault();

				if (previousResultX != null)
				{
					statusResultListX.Remove(previousResultX);
				}

				statusResultListX.Add(statusDataX.Duplicate());

				return statusResultListX;
			}
		}
	}
}