using kvdt_kiosk.Models;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Kvdt.ReturnJourney
{
    /// <summary>
    /// Interaction logic for SeniorPassengerInfoTextbox.xaml
    /// </summary>
    public partial class SeniorPassengerInfoTextbox : UserControl
    {
        public SeniorPassengerInfoTextbox()
        {
            InitializeComponent();
            CheckDataThread();
        }

        private void CheckData()
        {

            if (PassengerInfo.PassengerName != null && PassengerInfo.PassengerName != "")
            {
                TxtPassengerName.Text = PassengerInfo.PassengerName;
            }

        }

        private void CheckDataThread()
        {
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += (sender, e) =>
            {
                CheckData();
            };
            dispatcherTimer.Interval = new System.TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }
    }
}
