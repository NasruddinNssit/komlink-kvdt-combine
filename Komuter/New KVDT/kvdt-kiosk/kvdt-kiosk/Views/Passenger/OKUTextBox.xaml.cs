using kvdt_kiosk.Models;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace kvdt_kiosk.Views.Passenger
{
    /// <summary>
    /// Interaction logic for OKUTextBox.xaml
    /// </summary>
    public partial class OKUTextBox : UserControl
    {
        private DispatcherTimer dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        public OKUTextBox()
        {
            InitializeComponent();
        }

        public void UpdatePassenger()
        {
            dispatcherTimer.Start();

            dispatcherTimer.Tick += async (sender, args) =>
            {
                if (TxtPassengerName.Text == PassengerInfo.CurrentScanNumberForChild.ToString())
                {
                    TxtPassengerName.Text = await Task.Run(() => PassengerInfo.PassengerName);
                    TxtPassengerName.Visibility = System.Windows.Visibility.Visible;
                }
            };
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePassenger();
        }
    }
}
