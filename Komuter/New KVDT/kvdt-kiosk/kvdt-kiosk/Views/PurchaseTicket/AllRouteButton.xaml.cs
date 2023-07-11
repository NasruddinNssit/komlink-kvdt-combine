using kvdt_kiosk.Models;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace kvdt_kiosk.Views.PurchaseTicket
{
    /// <summary>
    /// Interaction logic for AllRouteButton.xaml
    /// </summary>
    public partial class AllRouteButton : UserControl
    {
        public AllRouteButton()
        {
            InitializeComponent();
            LoadLanguage();

            IsUserCheckOutAsync();
        }

        private async void BtnAll_Click(object sender, RoutedEventArgs e)
        {
            BtnAll.IsEnabled = false;
            await Task.Delay(500);
            BtnAll.IsEnabled = true;
        }

        private void LoadLanguage()
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (App.Language == "ms")
                {
                    lblAll.Text = "Semua Laluan";
                }
            });
        }

        private async void IsUserCheckOutAsync()
        {
            while (!UserSession.IsCheckOut)
            {
                await Task.Delay(500);
            }
            BtnAll.IsEnabled = false;
            BtnAll.Cursor = Cursors.No;
        }
    }
}
