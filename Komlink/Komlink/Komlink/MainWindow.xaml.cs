
using Komlink.Models;
using Komlink.Services;
using Komlink.Views;
using Komlink.Views.Idle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Komlink
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isIdle = true;
        private DateTime lastActivity = DateTime.Now;

        private APIServices _apiService = null;
        public MainWindow()
        {
            InitializeComponent();


            LanguageScreen languageScreen = new LanguageScreen();
            this.Content = languageScreen;

            _apiService = new APIServices(new APIURLServices(), new APISignatureServices());
            //Komlink.Views.Komlink.Calendar calendar = new Komlink.Views.Komlink.Calendar();
            //this.Content = calendar;

            StartActivityCheck();
        }

        private void StartActivityCheck()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += new EventHandler(CheckIdleTime);
            timer.Start();

        }

        private void CheckIdleTime(object sender, EventArgs e)
        {
            if(SystemConfig.IsResetIdleTimer)
            {
                lastActivity = DateTime.Now;
                SystemConfig.IsResetIdleTimer = false;

            }

            if (isIdle && DateTime.Now.Subtract(lastActivity).TotalSeconds > 120)
            {
                var timeOutScreen = new TimeOutScreen();

                var window = new Window
                {
                    WindowStyle = WindowStyle.None,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Background = System.Windows.Media.Brushes.Transparent,
                    AllowsTransparency = true,
                    Width = 800,
                    Height = 600,

                    Content = timeOutScreen
                };

                window.ShowDialog();

                if(window.IsActive)
                {
                    lastActivity = DateTime.Now;
                }

                if (timeOutScreen.IsActivity)
                {
                    lastActivity = DateTime.Now;
                }

                isIdle = false;
            }
            else
            {
                isIdle = true;  
            }

           
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var kioskId = await Task.Run(() => System.IO.File.ReadAllText(@"C:\NssITKiosk\LocalServer\Parameter.txt"));
            var kioskIdSplit = kioskId.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var kioskIdValue = kioskIdSplit[0].Split('=')[1];

            var result = await _apiService.GetAFCServiceByCounter(kioskIdValue);

            UserSession.PurchaseStationId = result.CounterStationId;
            UserSession.MCounters_Id = kioskIdValue;
            UserSession.KioskId = kioskIdValue;
        }
    }
}
