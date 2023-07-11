using kvdt_kiosk.Models;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.PurchaseTicket
{
    /// <summary>
    /// Interaction logic for DestinationScreen.xaml
    /// </summary>
    public partial class DestinationScreen : UserControl
    {
        //create ui update event
        public DestinationScreen()
        {
            InitializeComponent();

            lblFromStation.Text = UserSession.FromStationName;
            LoadLanguage();
            UpdateToStation();
        }

        private void LoadLanguage()
        {
            Dispatcher.Invoke(() =>
            {
                if (App.Language != "ms") return;
                lblDestination.Text = "DESTINASI";
                // lblFromStation.Text = "DARI STESEN";
                lblToStation.Text = "KE STESEN";
            });
        }

        public async void UpdateToStation()
        {
            while (PassengerInfo.DynamicInfo == null)
            {
                await Task.Delay(500);
            }
            lblToStation.Text = PassengerInfo.DynamicInfo;

            PassengerInfo.DynamicInfo = null;
        }

    }
}

