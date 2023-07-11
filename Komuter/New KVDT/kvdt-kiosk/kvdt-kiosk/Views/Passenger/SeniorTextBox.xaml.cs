using kvdt_kiosk.Models;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace kvdt_kiosk.Views.Passenger
{

    public partial class SeniorTextBox : UserControl
    {
        private DispatcherTimer dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };

        public SeniorTextBox()
        {
            InitializeComponent();
        }

        public void UpdatePassenger()
        {
            dispatcherTimer.Start();

            dispatcherTimer.Tick += async (sender, args) =>
            {
                if (TxtPassengerName.Text == PassengerInfo.CurrentScanNumberForSenior.ToString())
                {
                    TxtPassengerName.Text = await Task.Run(() => PassengerInfo.PassengerName);
                    TxtPassengerName.Visibility = System.Windows.Visibility.Visible;
                }
            };
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdatePassenger();
        }
    }
}
