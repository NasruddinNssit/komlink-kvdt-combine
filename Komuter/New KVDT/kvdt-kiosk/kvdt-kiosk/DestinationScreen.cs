namespace kvdt_kiosk.Views.PurchaseTicket
{
    /// <summary>
    /// Interaction logic for DestinationScreen.xaml
    /// </summary>
    public partial class DestinationScreen : UserControl
    {

        public DestinationScreen()
        {
            InitializeComponent();

            lblFromStation.Text = UserSession.FromStationName;
            LoadLanguage();
        }

        private void LoadLanguage()
        {
            Dispatcher.Invoke(() =>
            {
                if (App.Language == "ms")
                {
                    lblDestination.Text = "DESTINASI";
                    // lblFromStation.Text = "DARI STESEN";
                    lblToStation.Text = "KE STESEN";
                }
            });
        }

        public async void UpdateToStation(string stationName)
        {
            await Task.Delay(500);
            lblToStation.Text = stationName;
        }

    }
}
