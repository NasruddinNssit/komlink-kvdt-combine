using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.StatusCleanupSchedule.Base
{
    public static class SysInit
    {
		private const string LogChannel = "SysInit";
		public static int StatusLifeTimeDay = 7;

		public static void Start()
        {
			SysLocalParam SysParam = new SysLocalParam();
			SysParam.ReadParameters();

			NssIT.Kiosk.AppDecorator.Config.Setting _sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

			_sysSetting.ApplicationVersion = "NssIT.Train.Kiosk.StatusCleanupSchedule_x01";

			_sysSetting.WebApiURL = SysParam.PrmWebApiURL;
			_sysSetting.IsDebugMode = SysParam.PrmIsDebugMode;

			StatusLifeTimeDay = SysParam.PrmStatusLifeTimeDay;

			if (_sysSetting.IsDebugMode)
			{
				_sysSetting.IPAddress = "10.1.1.111";
				//_sysSetting.IPAddress = "10.238.4.15";
			}
			else
				_sysSetting.IPAddress = NssIT.Kiosk.AppDecorator.Config.Setting.GetLocalIPAddress();

			CheckRegistryConfig();

			DbLog.GetDbLog()?.LogText(LogChannel, "*", _sysSetting, "K01", "SysInit.Start");

			Task.Delay(3000).Wait();
			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

			_sysSetting.HashSecretKey = StatusCleanupSchedule.Properties.Settings.Default["HashSecretKey"].ToString();
			_sysSetting.TVMKey = StatusCleanupSchedule.Properties.Settings.Default["TVMKey"].ToString();
			_sysSetting.TimeZoneId = StatusCleanupSchedule.Properties.Settings.Default["TimeZoneId"].ToString();
			_sysSetting.AesEncryptKey = StatusCleanupSchedule.Properties.Settings.Default["AesEncryptKey"].ToString();

			//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		}

		private static void CheckRegistryConfig()
		{
			Thread testT = new Thread(new ThreadStart(new Action(() => {
				try
				{
					string rValue = RegistrySetup.GetRegistrySetting().DeviceId;
					//_msg.ShowMessage($@"Current Device Id : {rValue}");
				}
				catch (Exception ex)
				{

					DbLog.GetDbLog()?.LogError(LogChannel, "*", ex, "EX01", "SysInit.CheckRegistryConfig");
					//MessageBox.Show($@"Error when Check Registry Config.; {ex.ToString()}");
				}
			})));
			testT.IsBackground = true;
			testT.Start();
		}
	}
}
