using kvdt_kiosk.Models;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace kvdt_kiosk.Views.PurchaseTicket
{
    /// <summary>
    /// Interaction logic for StationRouteButton.xaml
    /// </summary>
    public partial class StationRouteButton
    {
        public StationRouteButton()
        {
            InitializeComponent();

            IsUserCheckOutAsync();
        }

        private async void BtnRoute_Click(object sender, RoutedEventArgs e)
        {
            BtnRoute.IsEnabled = false;
            await Task.Delay(50);
            BtnRoute.Style = (Style)FindResource("BtnSelected");
            BtnRoute.IsEnabled = true;
        }

        private async void IsUserCheckOutAsync()
        {
            while (!UserSession.IsCheckOut)
            {
                await Task.Delay(500);
            }
            BtnRoute.IsEnabled = false;
            BtnRoute.Cursor = Cursors.No;
        }

    }
}
