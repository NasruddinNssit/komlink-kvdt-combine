using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfBoostTouchNGoAPITest;


namespace WpfBoostTouchNGoAPITest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
			SysLocalParam SysParam = new SysLocalParam();
			SysParam.ReadParameters();

			NssIT.Kiosk.AppDecorator.Config.Setting _sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

			RegistrySetup.GetRegistrySetting().ReadAllRegistryValues(out string regErr);

			if (string.IsNullOrWhiteSpace(regErr) == false)
			{
				DbLog.GetDbLog()?.LogText("Application", "OnStartup", $@"Registry Error -- {regErr}", "X21", "App.OnStartup");

				throw new Exception(regErr);
			}

			_sysSetting.ApplicationVersion = "WpfBoostTouchNGoAPITest_x01";

			_sysSetting.WebApiURL = RegistrySetup.GetRegistrySetting().WebApiUrl;
			//_sysSetting.EWalletWebApiBaseURL = SysParam.PrmEWalletWebApiBaseURL;
			_sysSetting.IsDebugMode = SysParam.PrmIsDebugMode;
			_sysSetting.LocalServicePort = SysParam.PrmLocalServerPort;
			_sysSetting.PayMethod = SysParam.PrmPayMethod;
			_sysSetting.KioskId = SysParam.PrmKioskId;

			if (_sysSetting.IsDebugMode)
			{
				_sysSetting.IPAddress = "10.1.1.111";
				//_sysSetting.IPAddress = "10.238.4.15";
			}
			else
				_sysSetting.IPAddress = NssIT.Kiosk.AppDecorator.Config.Setting.GetLocalIPAddress();

			CheckRegistryConfig();

			//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

			_sysSetting.HashSecretKey = WpfBoostTouchNGoAPITest.Properties.Settings.Default["HashSecretKey"].ToString();
			_sysSetting.TVMKey = WpfBoostTouchNGoAPITest.Properties.Settings.Default["TVMKey"].ToString();
			_sysSetting.TimeZoneId = WpfBoostTouchNGoAPITest.Properties.Settings.Default["TimeZoneId"].ToString();
			_sysSetting.AesEncryptKey = WpfBoostTouchNGoAPITest.Properties.Settings.Default["AesEncryptKey"].ToString();

			//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

			// Testing  - NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway
			//MainWindow main = new MainWindow();

			// Testing  - NssIT.Kiosk.Network.PaymentGatewayApp
			MainWindow2 main = new MainWindow2();

			main.Show();
			System.Windows.Forms.Application.DoEvents();
		}

		private void CheckRegistryConfig()
		{
			Thread testT = new Thread(new ThreadStart(new Action(() => {
				try
				{
					RegistrySetup tr = new RegistrySetup();

					string rValue = RegistrySetup.GetRegistrySetting().DeviceId;

					//_msg.ShowMessage($@"Current Device Id : {rValue}");
				}
				catch (Exception ex)
				{
					//_log.LogError(logChannel, "KioskService01", ex, "EX01", "KioskService01.TestWriteRegistry");
					MessageBox.Show($@"Error when Check Registry Config.; {ex.ToString()}");
				}
			})));
			testT.IsBackground = true;
			testT.Start();
		}
	}
}
