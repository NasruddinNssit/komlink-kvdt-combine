using kvdt_kiosk.Models;
using System;
using System.Windows;
using System.Windows.Threading;

namespace kvdt_kiosk.Views.Passenger
{
    /// <summary>
    /// Interaction logic for PassengerTemplate.xaml
    /// </summary>
    public partial class PassengerTemplate
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        public PassengerTemplate()
        {
            InitializeComponent();
            lblTitle.Text = UserSession.JourneyType.ToUpper();

            TxtDate.Text = DateTime.Now.ToString("ddd, dd-MM");
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                var passengerScreen = new PassengerMyKadScan();

                GridPassenger.Children.Add(passengerScreen);
            }));

            ValidateMyKadScanFinished();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;

            UserSession.ChildSeat = UserSession.TempDataForChildSeat;
            UserSession.SeniorSeat = UserSession.TempDataForSeniorSeat;
            PassengerInfo.CurrentScanNumberForChild = 0;
            PassengerInfo.CurrentScanNumberForSenior = 0;
            PassengerInfo.ICNumber = null;
            PassengerInfo.PassengerName = null;
            PassengerInfo.IsPaxSelected = false;
            PassengerInfo.IcScanned.Clear();

            UserSession.TicketOrderTypes = null;
            UserSession.UserAddons = null;

        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;
        }

        private void Grid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;
        }

        private void ValidateMyKadScanFinished()
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;

            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (UserSession.ChildSeat != 0 && UserSession.SeniorSeat != 0)
            {
                BtnOk.IsEnabled = false;
            }
            else
            {
                BtnOk.IsEnabled = true;
            }
        }
    }
}
