using kvdt_kiosk.Models;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace kvdt_kiosk.Views.PurchaseTicket
{
    /// <summary>
    /// Interaction logic for GenericStationButton.xaml
    /// </summary>
    public partial class GenericStationButton : UserControl
    {

        public GenericStationButton()
        {
            InitializeComponent();
            IsUserCheckOutAsync();
        }

        private async void IsUserCheckOutAsync()
        {
            while (!UserSession.IsCheckOut)
            {
                await Task.Delay(500);
            }
            BtnGenericStation.IsEnabled = false;
            BtnGenericStation.Cursor = Cursors.No;
        }

        private void BtnGenericStation_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

    }
}
