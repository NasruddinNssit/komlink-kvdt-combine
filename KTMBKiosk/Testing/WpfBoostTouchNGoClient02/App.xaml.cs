using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WpfBoostTouchNGoClient02.NetClient;

namespace WpfBoostTouchNGoClient02
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static string _logChannel = "AppSys";
        private static NssIT.Kiosk.AppDecorator.Config.Setting _sysSetting = null;
        private static LibShowMessageWindow.MessageWindow _messageWindow = null;
        private static AppBTnGSvcEventsHandler _appBTnGSvcEventsHandler = null;

        public static DbLog Log { get; private set; }
        public static SynchronizationContext ExecuteContext { get; private set; }
        public static NetClientService NetClientSvc { get; private set; }
        public static MainWindow AppWindow { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.Exit += App_Exit;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            ExecuteContext = SynchronizationContext.Current;

            try
            {
                _sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

                _sysSetting.ApplicationVersion = "WpfBoostTouchNGoClient_x01";
                if (_sysSetting.IsDebugMode)
                {
                    _sysSetting.IPAddress = "10.1.1.111";
                    //_sysSetting.IPAddress = "10.238.4.15";
                }
                else
                    _sysSetting.IPAddress = NssIT.Kiosk.AppDecorator.Config.Setting.GetLocalIPAddress();

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

                Log.LogText(_logChannel, "-", $@"Start - WpfBoostTouchNGoClient - App XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
                        "A01", "App.OnStartup");

                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                //System Parameter Validation
                //-----------------------------

                bool passSysParameterCheck = true;

                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                _sysSetting.HashSecretKey = WpfBoostTouchNGoClient02.Properties.Settings.Default.HashSecretKey;
                _sysSetting.TVMKey = WpfBoostTouchNGoClient02.Properties.Settings.Default.TVMKey;
                _sysSetting.TimeZoneId = WpfBoostTouchNGoClient02.Properties.Settings.Default.TimeZoneId;
                _sysSetting.AesEncryptKey = WpfBoostTouchNGoClient02.Properties.Settings.Default.AesEncryptKey;
                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                NetClientSvc = new NetClientService();

                _appBTnGSvcEventsHandler = new AppBTnGSvcEventsHandler(NetClientSvc);

                System.Windows.Forms.Application.DoEvents();

                AppWindow = new MainWindow();
                AppWindow.Show();
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

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string ss = e.Exception.Message;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            int ss = e.ApplicationExitCode;

            try
            {
                NetClientSvc?.Dispose();

                if (NetClientSvc != null)
                    Task.Delay(3000).Wait();
            }
            catch { }
            finally
            {
                NetClientSvc = null;
            }
        }

        protected override void OnExit(System.Windows.ExitEventArgs e)
        {
            try
            {
                NetClientSvc?.Dispose();

                if (NetClientSvc != null)
                    Task.Delay(3000).Wait();
            }
            catch { }
            finally
            {
                NetClientSvc = null;
            }
        }
    }
}
