using kvdt_kiosk.Models;
using kvdt_kiosk.Services;
using kvdt_kiosk.Views;
using kvdt_kiosk.Views.Idle;
using kvdt_kiosk.Views.Welcome;
using LazyCache;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace kvdt_kiosk
{
    public partial class MainWindow
    {
        private readonly APIServices _aPiServices = new APIServices(new APIURLServices(), new APISignatureServices());
        public IList<AFCRouteModel> AfcRouteModels { get; set; }
        public IList<AFCStationDetails> AfcStationDetails { get; set; }
        public IList<Packages> AfcPackages { get; set; }
        private bool isIdle = true;
        private DateTime lastActivity = DateTime.UtcNow;

        private readonly IAppCache _afcRoutecache = new CachingService();
        private readonly IAppCache _afcPackagescache = new CachingService();
        private readonly IAppCache _afcStationscache = new CachingService();

        public MainWindow()
        {
            InitializeComponent();

            var welcome = new WelcomeScreen();
            Content = welcome;
            StartActivityCheck();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var kioskId = await Task.Run(() => System.IO.File.ReadAllText(@"C:\NssITKiosk\LocalServer\Parameter.txt"));

            var kioskIdSplit = kioskId.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            var kioskIdValue = kioskIdSplit[0].Split('=')[1];

            var result = await _aPiServices.GetAFCServiceByCounter(kioskIdValue);

            UserSession.AFCService = result.Id;
            UserSession.FromStationId = result.CounterStationId;
            UserSession.FromStationName = result.CounterStationName;
            UserSession.KioskId = kioskIdValue;
            UserSession.MCounters_Id = kioskIdValue;


            kvdt_kiosk.Models.Komlink.UserSession.PurchaseStationId = result.CounterStationId;
            kvdt_kiosk.Models.Komlink.UserSession.MCounters_Id = kioskIdValue;
            kvdt_kiosk.Models.Komlink.UserSession.KioskId = kioskIdValue;
            await Task.Run(() => LoadDataFromApiAndDoTheCachingAsync());
        }

        public async Task LoadDataFromApiAndDoTheCachingAsync()
        {
            var resultRouteTask = _aPiServices.GetAFCStations(UserSession.AFCService);
            var resultPackageTask = _aPiServices.GetAFCPackage();
            var resultStationTask = _aPiServices.GetAFCStations(UserSession.AFCService);

            await Task.WhenAll(resultRouteTask, resultPackageTask, resultStationTask);

            AfcRouteModels = resultRouteTask.Result.AfcRouteModels;
            AfcPackages = resultPackageTask.Result.Data;
            AfcStationDetails = resultStationTask.Result.AfcStationModels;

            _afcRoutecache.Add("AFCRoutesCache", AfcRouteModels, DateTimeOffset.Now.AddHours(1));
            _afcPackagescache.Add("AFCPackagesCache", AfcPackages, DateTimeOffset.Now.AddHours(1));
            _afcStationscache.Add("AFCStationsCache", AfcStationDetails, DateTimeOffset.Now.AddHours(1));
        }

        private void StartActivityCheck()
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            timer.Tick += CheckIdleTime;
            timer.Start();
        }

        private void CheckIdleTime(object sender, EventArgs e)
        {
            if (SystemConfig.IsResetIdleTimer)
            {
                lastActivity = DateTime.UtcNow;
                SystemConfig.IsResetIdleTimer = false;
            }

            if (isIdle && DateTime.UtcNow.Subtract(lastActivity).TotalSeconds > 120)
            {
                var closingScreen = new ClosingScreen();

                var window = new Window
                {
                    WindowStyle = WindowStyle.None,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,

                    Background = System.Windows.Media.Brushes.Transparent,
                    AllowsTransparency = true,

                    Width = 800,
                    Height = 600,

                    Content = closingScreen
                };

                window.ShowDialog();

                if (window.IsActive || closingScreen.IsActivity)
                {
                    lastActivity = DateTime.UtcNow;
                }

                isIdle = false;
            }
            else
            {
                isIdle = true;
            }
        }
    }

}
