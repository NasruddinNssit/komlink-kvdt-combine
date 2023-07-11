using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.UI;
//using NssIT.Kiosk.Device.B2B.B2BApp;
using NssIT.Kiosk.Common.AppService.Network.TCP;
using NssIT.Kiosk.Server.Service.Adaptor;
using NssIT.Kiosk.AppDecorator.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using NssIT.Kiosk.Common.AppService.Network;
//using NssIT.Kiosk.Device.B2B.Server.Service.Adaptor;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Configuration;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace WpfKtmbKioskServerTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        string logChannel = "KioskService";
        string logDBConnStr = $@"Data Source=C:\dev\source code\Kiosk\Code\NssIT.Kiosk.Server\LogDB\NssITKioskLog01_Test.db;Version=3";

        private NssIT.Kiosk.AppDecorator.Config.Setting _sysSetting = null;
        private NssIT.Kiosk.Log.DB.DbLog _log = null;
        private INetMediaInterface _netInterface = null;
        private CashPaymentServerSvcAdaptor _cashPaymentSvc = null;
        private SalesServerSvcAdaptor _salesSvr = null;
		private BTnGServerSvcAdaptor _bTnGSvr = null;
		
        //private B2BServerServiceAdaptor _b2bSvr = null;

        /// <summary>
        /// Version Refer to an application Version, Date, and release count of the day.
        /// Like "V1.R20200805.1" mean application Version is V1, the release Year is 2020, 5th (05) of August (08), and 1st (.1) release count of the day.
        /// Note : With "DEV-" for undeployable version. This version is not for any release purpose. Only for development process.
        /// </summary>
        private string SystemVersion = "R210405.1";
        private IUIPayment _cashPaymentApp = null;
        private NetInfoRepository _netInfoRepository = null;

        public SysLocalParam SysParam { get; private set; }

        [MTAThread]
        protected override void OnStartup(StartupEventArgs e)
        {
			base.OnStartup(e);

			//CYA-DEBUG .. System.Diagnostics.Debugger.Launch();

			try
			{
				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
				// Resolve Cert Problem 
				//Trust all certificates
				System.Net.ServicePointManager.ServerCertificateValidationCallback =
					((sender, certificate, chain, sslPolicyErrors) => true);

				// trust sender
				System.Net.ServicePointManager.ServerCertificateValidationCallback
								= ((sender, cert, chain, errors) => cert.Subject.Contains("YourServerName"));

				// validate cert by calling a function
				ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

				_log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();
				_log.LogText(logChannel, "KioskService01", "Start - KioskService01 XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", "A01", "App.OnStartup");

				RegistrySetup.GetRegistrySetting().ReadAllRegistryValues(out string regErr);

				if (string.IsNullOrWhiteSpace(regErr) == false)
				{
					_log.LogText(logChannel, "OnStartup", $@"Registry Error -- {regErr}", "X21", "App.OnStartup");

					throw new Exception(regErr);
				}

				SysParam = new SysLocalParam();
				SysParam.ReadParameters();

				_sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

				_sysSetting.ApplicationVersion = SystemVersion;

				//_sysSetting.WebServiceURL = SysParam.PrmWebServiceURL;
				_sysSetting.WebApiURL = SysParam.PrmWebApiURL;
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

				_log.LogText(logChannel, "SystemParam", SysParam, "PARAMETER", "KioskService01.OnStart");
				_log.LogText(logChannel, "SystemSetting", _sysSetting, "SETTING", "KioskService01.OnStart");

				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

				_sysSetting.HashSecretKey = WpfKtmbKioskServerTest.Properties.Settings.Default["HashSecretKey"].ToString();
				_sysSetting.TVMKey = WpfKtmbKioskServerTest.Properties.Settings.Default["TVMKey"].ToString();
				_sysSetting.TimeZoneId = WpfKtmbKioskServerTest.Properties.Settings.Default["TimeZoneId"].ToString();
				_sysSetting.AesEncryptKey = WpfKtmbKioskServerTest.Properties.Settings.Default["AesEncryptKey"].ToString();

				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

				CheckRegistryConfig();

				//-----------------------------------------------------------------------------------------------------------------------------
				_netInfoRepository = new NetInfoRepository();
				_netInterface = new LocalTcpService(_sysSetting.LocalServicePort);
				//_cashPaymentApp = new B2BPaymentApplication();

				// Standard Server Service Adaptors ---------- ---------- ---------- ---------- ---------- ---------- ---------- 
				// Module : UIKioskSales
				_salesSvr = new SalesServerSvcAdaptor(_netInterface, _netInfoRepository);
				// Module : UIPayment
				//_cashPaymentSvc = new CashPaymentServerSvcAdaptor(_netInterface, _cashPaymentApp, _netInfoRepository);
				//---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- 
				// Custom Device Server Service Adaptors
				// Module : UIB2B
				//_b2bSvr = new B2BServerServiceAdaptor(_netInterface, _netInfoRepository);
				//---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- 
				//BTnGServerSvcAdaptor
				// Module : UIKioskSales
				_bTnGSvr = new BTnGServerSvcAdaptor(_netInterface, _netInfoRepository);
				// Module : UIPayment
				//_cashPaymentSvc = new CashPaymentServerSvcAdaptor(_netInterface, _cashPaymentApp, _netInfoRepository);
				//---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- 

				_log.LogText(logChannel, "-", $@"Local Kiosk Service Loaded; Kiosk Loacl Server Version : {SystemVersion}", "A10", "KioskService01.OnStart");

				(new MainWindow()).Show();
			}
			catch (Exception ex)
			{
				_log.LogError(logChannel, "KioskService01", ex, "EX01", "KioskService01.OnStart");
				throw ex;
			}
			finally
			{
				_log.LogText(logChannel, "KioskService01", "End - KioskService01", "A100", "KioskService01.OnStart");
			}
			
        }

        protected override void OnExit(ExitEventArgs e)
        {
			if (_netInterface != null)
			{
				try
				{
					_netInterface.ShutdownService();
				}
				catch { }
				try
				{
					_netInterface.Dispose();
				}
				catch { }

				_netInterface = null;
			}

			if (_netInfoRepository != null)
			{
				try
				{
					_netInfoRepository.Dispose();
				}
				catch { }

				_netInfoRepository = null;
			}

			if (_cashPaymentSvc != null)
			{
				try
				{
					_cashPaymentSvc.Dispose();
				}
				catch { }

				_cashPaymentSvc = null;
			}

			if (_cashPaymentApp != null)
			{
				try
				{
					_cashPaymentApp.ShutDown();
				}
				catch { }
				try
				{
					_cashPaymentApp.Dispose();
				}
				catch { }

				_cashPaymentApp = null;
			}

			try
			{
				//_b2bSvr.Dispose();
			}
			catch { }

			try
			{
				NssIT.Kiosk.AppDecorator.Config.Setting.Shutdown();
			}
			catch { }

			Task.Delay(500).Wait();
		}

		// callback used to validate the certificate in an SSL conversation
		private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
		{
			//bool result = cert.Subject.Contains("YourServerName");
			//return result;
			return true;
		}

		private void CheckRegistryConfig()
		{
			Exception err = null;
			Thread testT = new Thread(new ThreadStart(new Action(() => {
				try
				{
					string rValue = RegistrySetup.GetRegistrySetting().DeviceId;
					//_msg.ShowMessage($@"Current Device Id : {rValue}");
				}
				catch (Exception ex)
				{
					err = ex;
					_log.LogError(logChannel, "KioskService01", ex, "EX01", "KioskService01.CheckRegistryConfig");
				}
			})));
			testT.IsBackground = true;
			testT.Start();
			testT.Join();

			if (err != null)
				throw err;
		}
	}
}