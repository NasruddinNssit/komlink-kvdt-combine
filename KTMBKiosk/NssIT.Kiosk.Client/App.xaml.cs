using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Config.ConfigConstant;
using NssIT.Kiosk.AppDecorator.Global;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Client.Base.LocalDevices;
using NssIT.Kiosk.Client.Base.Time;
using NssIT.Kiosk.Client.NetClient;
using NssIT.Kiosk.Client.Reports;
using NssIT.Kiosk.Client.ViewPage.Menu;
using NssIT.Kiosk.Device.PAX.IM30.AccessSDK;
using NssIT.Kiosk.Device.PAX.IM30.IM30PayApp;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NssIT.Kiosk.Client
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static event EventHandler OnPresentCustomer;
		public static event EventHandler OnAbsentCustomer;

		private const int MaxAdvanceTicketDays = 365;

		private static string _logChannel = "AppSys";

		private static LibShowMessageWindow.MessageWindow _messageWindow = null;
		private static IMainScreenControl _mainScreenControl = null;
		private static StatusMonitorClientDispatcher _statusMonitorClientDispatcher = null;

		public const int CustomerInfoTimeoutExtensionSec = 60 * 10;

		/// <summary>
		/// Version Refer to an application Version, Date, and release count of the day.
		/// Like "V1.R20200805.1" mean application Version is V1, the release Year is 2020, 5th (05) of August (08), and 1st (.1) release count of the day.
		/// Note : With "DEV-" for undeployable version. This version is not for any release purpose. Only for development process.
		/// </summary>
		public static string SystemVersion = "V1.R230525.1";

		private static NssIT.Kiosk.AppDecorator.Config.Setting _sysSetting = null;

		public static AppHelper AppHelp { get; private set; } = null;

		public static bool IsLocalServerReady { get; set; } = false;

		public static bool IsClientReady { get; private set; } = false;

		public static bool IsAutoTimeoutExtension { get; set; } = false;

		public static SysLocalParam SysParam { get; private set; }

		public static DbLog Log { get; private set; }

		public static UserSession LatestUserSession { get; set; }

		public static NetClientService NetClientSvc { get; private set; }

		private static AppSalesSvcEventsHandler _appSalesSvcEventsHandler = null;

		public static ReportPDFFileManager ReportPDFFileMan { get; private set; }

		public static SynchronizationContext ExecuteContext { get; private set; }

		public static int AppMaxSeatPerTrip { get; private set; } = 20;

		public static ScreenImageManager PaymentScrImage { get; private set; }

		public static TravelMode AvailableTravelMode { get; private set; } = TravelMode.DepartOnly;

		public static ResetTimeoutManager TimeoutManager { get; private set; } = null;

		public static DateTime MaxTicketAdvanceDate { get; private set; } = DateTime.Now.AddDays(MaxAdvanceTicketDays);

		public static PayWaveSettlementScheduler CardSettlementScheduler { get; private set; } = null;

		private static NssIT.Kiosk.Device.PosiFlex.MotionSensor1.OrgAPI.PosiFlexMotionSensorDLLWrapper CustomerSensor { get; set; } = null;

		public static BookingTimeoutCounter BookingTimeoutMan { get; set; } = null;

		public static ITowerLight TowerLight { get; set; } = null;

		public static TicketTransactionStage CurrentTransStage { get; set; } = TicketTransactionStage.ETS;

		public static WebAPISiteCode WebAPICode { get; set; } = WebAPISiteCode.Unknown;

		public static MarkLogList MarkLog { get; set; }

		private SysLog _sysLog = null;

		private static IM30AccessSDK _im30AccessSDK = null;

        public static bool IsCustomerSensorExist
		{
			get
			{
				return (CustomerSensor != null);
			}
		}

		public static void ResetMaxTicketAdvanceDate()
		{
			DateTime dateTime = DateTime.Now.AddDays(MaxAdvanceTicketDays);

			MaxTicketAdvanceDate = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, 999);
		}

		public static string NextCardSettlementTimeString
		{
            get
            {
				if (CardSettlementScheduler != null)
				{
					return CardSettlementScheduler.NextSettlementTimeString;
				}
				else
					return "*";
            }
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

            SysGlobalLock.Init();

            //System.Diagnostics.Debugger.Launch();

            _sysLog = new SysLog();

			this.Exit += App_Exit;
			this.DispatcherUnhandledException += App_DispatcherUnhandledException;

			ExecuteContext = SynchronizationContext.Current;

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

				_sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

				//_sysSetting.HashSecretKey = NssIT.Kiosk.Client.Properties.Settings.Default.HashSecretKey;
				//_sysSetting.TVMKey = NssIT.Kiosk.Client.Properties.Settings.Default.TVMKey;
				//_sysSetting.TimeZoneId = NssIT.Kiosk.Client.Properties.Settings.Default.TimeZoneId;
				//_sysSetting.AesEncryptKey = NssIT.Kiosk.Client.Properties.Settings.Default.AesEncryptKey;

				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
				String thisprocessname = Process.GetCurrentProcess().ProcessName;

				if (Process.GetProcesses().Count(p => p.ProcessName == thisprocessname) > 1)
				{
					MessageBox.Show("Twice access to Kiosk Client Application is prohibited", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
					System.Windows.Application.Current.Shutdown();
					return;
				}

				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

				Log = DbLog.GetDbLog();

				MarkLog = MarkLogList.GetLogList().ActivateCardMarkingLog();

				Log.LogText(_logChannel, "-", $@"Start - Kiosk Client Version : {SystemVersion} - App XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
						"A01", "App.OnStartup");

				SysParam = new SysLocalParam();
				SysParam.ReadParameters();

				// Note : AcroRd32 will block system from Opening IP Port correctly.
				//PDFTools.KillAdobe("AcroRd32");

				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
				//System Parameter Validation
				//-----------------------------

				bool passSysParameterCheck = true;

				//Server & Client Port Checking
				if ((SysParam.PrmLocalServerPort < 0) || (SysParam.PrmLocalServerPort > 65535))
				{
					passSysParameterCheck = false;
					MessageBox.Show("Invalid LocalServerPort parameter", "System Parameter Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
				else if ((SysParam.PrmClientPort < 0) || (SysParam.PrmClientPort > 65535))
				{
					passSysParameterCheck = false;
					MessageBox.Show("Invalid ClientPort parameter", "System Parameter Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
				//// Payment Method Checking
				//else if ((SysParam.IsPayMethodValid == false))
				//{
				//	passSysParameterCheck = false;
				//	MessageBox.Show("Invalid PayMethod parameter. Only C is allowed at the moment.", "System Parameter Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				//}
				// Adobe Reader File Path Checking
				//else if ((SysParam.PrmAcroRd32FilePath is null))
				//{
				//	passSysParameterCheck = false;
				//	MessageBox.Show("Adobe Reader File (AcroRd32.exe) Path is missing. Please make sure Adobe Reader has installed. And set the right default printer.", "System Parameter Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				//}

				else if ((SysParam.PrmPayWaveCOM is null))
				{
					passSysParameterCheck = false;
					MessageBox.Show("COM Port for credit card machine is missing. Please make sure COM Port has installed properly. And assign the COM Port into parameter file.", "System Parameter Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}

				else if ((SysParam.PrmCardSettlementTime is null))
				{
					passSysParameterCheck = false;
					MessageBox.Show("CardSettlementTime parameter is missing. Please enter a time in HH:mm (24Hours format).", "System Parameter Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}

				else if ((SysParam.PrmCheckPrinterPaperLow.HasValue == false))
				{
					passSysParameterCheck = false;
					MessageBox.Show("CheckPrinterPaperLow parameter is missing. Please set a 'Yes' or 'No' to the parameter.", "System Parameter Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}

				////AcroRd32.exe file checking
				//if (passSysParameterCheck == true)
				//{
				//	try
				//	{
				//		FileInfo fInf = new FileInfo(SysParam.PrmAcroRd32FilePath);
				//		if (fInf.Exists == false)
				//		{
				//			passSysParameterCheck = false;
				//			MessageBox.Show($@"Fail to allocate AcroRd32.exe refer to AcroRd32 parameter {SysParam.PrmAcroRd32FilePath}; Please verify the path of the file.  And set the right default printer.", "System Parameter Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				//		}
				//	}
				//	catch (Exception ex)
				//	{
				//		passSysParameterCheck = false;
				//		MessageBox.Show($@"Error when clarify AcroRd32 parameter {SysParam.PrmAcroRd32FilePath}; {ex.Message}; Please verify the path of the file.", "System Parameter Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				//	}
				//}

				if (passSysParameterCheck == false)
				{
					System.Windows.Application.Current.Shutdown();
					return;
				}

				//PDFTools.AdobeReaderFullFilePath = SysParam.PrmAcroRd32FilePath;

				SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

				if (SysParam.PrmIsDebugMode == true)
				{ 
					_messageWindow = new LibShowMessageWindow.MessageWindow();
					System.Windows.Forms.Application.DoEvents();
				}

				_sysSetting.NoCardSettlement = SysParam.PrmNoCardSettlement;
				_sysSetting.DisablePrinterTracking = SysParam.PrmDisablePrinterTracking;
				_sysSetting.ApplicationVersion = SystemVersion;

				Log.LogText(_logChannel, "SystemSetting", _sysSetting, "SETTING", "App.OnStartup");

				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
				_sysSetting.HashSecretKey = NssIT.Kiosk.Client.Properties.Settings.Default.HashSecretKey;
				_sysSetting.TVMKey = NssIT.Kiosk.Client.Properties.Settings.Default.TVMKey;
				_sysSetting.TimeZoneId = NssIT.Kiosk.Client.Properties.Settings.Default.TimeZoneId;
				_sysSetting.AesEncryptKey = NssIT.Kiosk.Client.Properties.Settings.Default.AesEncryptKey;
				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
				try
				{
					if (KioskServiceSwitching.StartService())
					{
						Log.LogText(_logChannel, "-", $@"Service -Nssit.Kiosk.Server- should be started", "A10B", "App.OnStartup");

						Task.Delay(1000 * 12).Wait();
					}
					else
					{
						Log.LogText(_logChannel, "-", $@"-Nssit.Kiosk.Server- may already started", "A10C", "App.OnStartup");
					}
				}
				catch (Exception ex)
				{
					Log.LogText(_logChannel, "-", $@"Error starting -NssIT.Kiosk.Server service-; {ex.Message}", "EX01B", "App.OnStartup",
						AppDecorator.Log.MessageType.Error);
				}
				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
				RDLCLibraryStarter.InitLibrary();
				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
				//_CustSensor
				if (SysParam.PrmCustomerSensorCOM != null)
                {
					CustomerSensor = new Device.PosiFlex.MotionSensor1.OrgAPI.PosiFlexMotionSensorDLLWrapper(SysParam.PrmCustomerSensorCOM, 1);
                    CustomerSensor.OnPresentCustomer += CustomerSensor_OnPresentCustomer;
                    CustomerSensor.OnAbsentCustomer += CustomerSensor_OnAbsentCustomer;
				}
				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
				//TowerLight
				if (SysParam.PrmLightIndicatorCOM != null)
				{
					//ShowDebugMsg($@"Start - Create PosiFlexStatusIndicator COM : {SysParam.PrmLightIndicatorCOM}");
					TowerLight = new PosiFlexStatusIndicator(SysParam.PrmLightIndicatorCOM, new AppDecorator.Log.ShowMessageLogDelg(App.ShowDebugMsg));
					//Debug-Testing..TowerLight = new DummyLightIndicator();
					//ShowDebugMsg($@"End -Create PosiFlexStatusIndicator");
				}
				else
				{
					//ShowDebugMsg($@"Create DummyLightIndicator.");
					TowerLight = new DummyLightIndicator();
				}
				//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

				PaymentScrImage = new ScreenImageManager("PaymentDone");

				AppHelp = new AppHelper();

				NetClientSvc = new NetClientService();

				_appSalesSvcEventsHandler = new AppSalesSvcEventsHandler(NetClientSvc);

				Log.LogText(_logChannel, "SystemParam", SysParam, "SYS", "App.OnStartup");

				ReportPDFFileMan = new ReportPDFFileManager();

				TimeoutManager = ResetTimeoutManager.GetLocalTimeoutManager();

				//WndTestingMonitor testMon = null;

				CardSettlementScheduler = new PayWaveSettlementScheduler(SysParam.PrmPayWaveCOM, SysParam.PrmCardSettlementTime);

				BookingTimeoutMan = new BookingTimeoutCounter();

				_statusMonitorClientDispatcher = StatusMonitorClientDispatcher.GetDispatcher();

				App.IsClientReady = true;

                _im30AccessSDK = new IM30AccessSDK(SysParam.PrmPayWaveCOM, null);

                MainWindow main = new MainWindow();
				_mainScreenControl = (IMainScreenControl)main;
				_mainScreenControl.InitiateMaintenance(CardSettlementScheduler);
				
				main.WindowState = WindowState.Maximized;

				if (SysParam.PrmIsDebugMode == false)
				{
					main.WindowStyle = WindowStyle.None;
					//main.Topmost = true;
				}

				if (SysParam.PrmIsDebugMode)
				{
					//DEBUG-Testing .. testMon = new WndTestingMonitor(main);
					//DEBUG-Testing .. testMon.Show();
					//DEBUG-Testing .. ShowDebugMsg("Start Debug");
				}

				main.Show();
				System.Windows.Forms.Application.DoEvents();

				((IMainScreenControl)main).ShowWelcome();
				System.Windows.Forms.Application.DoEvents();

				Log.LogText(_logChannel, "SystemParam", SysParam, "PARAMETER", "App.OnStartup");
				//Log.LogText(_logChannel, "SystemSetting", _sysSetting, "SETTING", "App.OnStartup");

			}
			catch (Exception ex)
			{
				if (Log != null)
				{
					Log.LogError(_logChannel, "-", ex, "EX01", "App.OnStartup");
				}

				MessageBox.Show($@"Error. Application is quit; {ex.Message}", "System Parameter Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				System.Windows.Application.Current.Shutdown();
			}
		}

		// callback used to validate the certificate in an SSL conversation
		private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
		{
			//bool result = cert.Subject.Contains("YourServerName");
			//return result;
			return true;
		}

		private static void CustomerSensor_OnPresentCustomer(object sender, EventArgs e)
        {
            try
            {
				OnPresentCustomer?.Invoke(null, new EventArgs());
			}
			catch (Exception ex)
            {
				Log.LogError(_logChannel, "-", ex, "EX01", "App.CustomerSensor_OnPresentCustomer");
            }
        }

		private static void CustomerSensor_OnAbsentCustomer(object sender, EventArgs e)
		{
			try
			{
				OnAbsentCustomer?.Invoke(null, new EventArgs());
			}
			catch (Exception ex)
			{
				Log.LogError(_logChannel, "-", ex, "EX01", "App.CustomerSensor_OnAbsentCustomer");
			}
		}

		public static void StartCustomerSensor()
        {
	        try
            {
				if (CustomerSensor != null)
					CustomerSensor.StartMotionDetector();
			}
			catch (Exception ex)
            {
				Log.LogError(_logChannel, "-", ex, "EX01", "App.StartCustomerSensor");
			}
		}

		public static void EndCustomerSensor()
		{
			try
			{
				if (CustomerSensor != null)
					CustomerSensor.StopMotionDetector();
			}
			catch (Exception ex)
			{
				Log.LogError(_logChannel, "-", ex, "EX01", "App.EndCustomerSensor");
			}
		}

		private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			_sysLog.WriteLog(e.Exception.ToString());
			ShieldErrorScreen.ShowMessage($@"System encountered difficulty and will be shutdowned;{"\r\n"}{DateTime.Now.ToString("HH:mm:ss")}{"\r\n"}{e.Exception.Message}");
		}

		//private static SemaphoreSlim _appLock = new SemaphoreSlim(1);
		//public static void ShutdownX()
		//{
		//	try
		//	{
		//		_statusMonitorClientDispatcher?.Dispose();
		//		Task.Delay(500).Wait();
		//	}
		//	catch { }

		//	try
		//	{
		//		_appLock.WaitAsync().Wait();

		//		if (_hasShutDown)
		//			return;

		//		_hasShutDown = true;

		//		App.TowerLight?.SwitchOff();
		//		App.TowerLight?.Dispose();

		//		NetClientSvc?.Dispose();
		//		Task.Delay(2500).Wait();
		//	}
		//	catch { }
		//	finally
		//	{
		//		if (_appLock.CurrentCount == 0)
		//			_appLock.Release();
		//	}
		//}

		private static bool _hasShutDown = false;
		private static object _shutdownLock = new object();
		public static void ShutdownX()
		{
			Thread sThreadWorker = new Thread(new ThreadStart(new Action(() =>
			{
				if (_hasShutDown)
					return;

				string shutdownLog = "";

				lock(_shutdownLock)
                {
					_hasShutDown = true;

					///// Shutdown Paywave -------------------------------------------------------
					///// PayECRAccess.GetExistingPayECRAccess()?.Dispose();
					try
					{
                        _im30AccessSDK?.StopCardTransaction(out _);
                    }
					catch (Exception ex)
					{ }
					
                    shutdownLog += "Shutdown Paywave; ";

					///// Shutdown Status Monitor -------------------------------------------------------
					try
					{
						_statusMonitorClientDispatcher?.Dispose();
						shutdownLog += "Shutdown Status Monitor; ";
					}
					catch { }

					///// Shutdown TowerLight & NetClientSvc -------------------------------------------------------
					try
					{
						App.TowerLight?.SwitchOff();
						Thread.Sleep(500);
						App.TowerLight?.Dispose();
						shutdownLog += "Shutdown TowerLight Monitor; ";
					}
					catch { }

					///// Shutdown App. Helper -------------------------------------------------------
					AppHelp?.Dispose();
					shutdownLog += "Shutdown AppHelp; ";

					Log.LogText(_logChannel, "*", $@"Shutdown Kiosk Client; {shutdownLog}", "E100", "App.ShutdownX");
					
					try
					{
						MarkLog.QuitMarkingLog();
					}
					catch { }
					
					Thread.Sleep(5000);
				}
				
			})));
			sThreadWorker.IsBackground = true;
			sThreadWorker.Priority = ThreadPriority.Highest;
			sThreadWorker.Start();
			sThreadWorker.Join(30 * 1000);
		}

		private void App_Exit(object sender, ExitEventArgs e)
		{
			ShutdownX();
			ExitNetClientService();
		}

		protected override void OnExit(System.Windows.ExitEventArgs e)
		{
			ShutdownX();
			ExitNetClientService();
		}

		private static bool _isExitNetClientSvc = false;
		private static object _exitNetClientSvcLock = new object();
		private static void ExitNetClientService()
        {
			Thread tWorker = new Thread(new ThreadStart(new Action(() => 
			{
				if (_isExitNetClientSvc == false)
				{
					lock (_exitNetClientSvcLock)
					{
						_isExitNetClientSvc = true;
						///// Shutdown NetClientSvc -----------------------
						try
						{
							NetClientSvc?.Dispose();
						}
						catch { }
					}

					Log.LogText(_logChannel, "*", $@"Exit NetClientService", "E100", "App.ExitNetClientService");
					Thread.Sleep(1000);
				}
			})));
			tWorker.IsBackground = true;
			tWorker.Priority = ThreadPriority.Highest;
			tWorker.Start();
			tWorker.Join();
		}

		public static string ExecutionFolderPath
		{
			get
			{
				string executionFilePath = Assembly.GetExecutingAssembly().Location;

				FileInfo fInf = new FileInfo(executionFilePath);
				string executionFolderPath = fInf.DirectoryName;

				return executionFolderPath;
			}
		}

		public static void ShowDebugMsg(string msg)
		{
			if (SysParam.PrmIsDebugMode)
			{
				DebugMsg?.ShowMessage(msg);
			}
		}

		private static LibShowMessageWindow.MessageWindow DebugMsg
		{
			get
			{
				return _messageWindow;
			}
		}

		public static IMainScreenControl MainScreenControl
		{
			get
			{
				return _mainScreenControl;
			}
		}

	}
}
