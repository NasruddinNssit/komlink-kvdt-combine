using kvdt_kiosk.Models;
using kvdt_kiosk.Services;
using kvdt_kiosk.Views.SeatingScreen.New.Kvdt;
using kvdt_kiosk.Views.Windows;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Kvdt.PurchaseTicket
{
    /// <summary>
    /// Interaction logic for JourneyTypeScreen.xaml
    /// </summary>
    public partial class JourneyTypeButton : UserControl
    {
        APIServices aPIServices = new APIServices(new APIURLServices(), new APISignatureServices());
        public JourneyTypeButton()
        {
            InitializeComponent();

            GetAFCPackage();
        }

        private void GetAFCPackage()
        {

        }

        private void BtnJourney_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ShowSeatScreen();
        }

        private void ShowSeatScreen()
        {
            MyDispatcher.Invoke(() =>
            {
                UserSession.JourneyType = TxtJourney.Text;

                ReturnJourneyWindow returnJourneyWindow = new ReturnJourneyWindow();

                // SeatScreen seatScreen = new SeatScreen();
                pgKomuterPax pgKomuterPax = new pgKomuterPax();
                returnJourneyWindow.Content = pgKomuterPax;
                returnJourneyWindow.WindowStyle = System.Windows.WindowStyle.None;
                returnJourneyWindow.Width = 710;
                returnJourneyWindow.Height = 1000;
                returnJourneyWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                returnJourneyWindow.WindowState = System.Windows.WindowState.Normal;
                returnJourneyWindow.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#074481"));
                returnJourneyWindow.Owner = System.Windows.Application.Current.MainWindow;
                returnJourneyWindow.Owner.Effect = new System.Windows.Media.Effects.BlurEffect();
                returnJourneyWindow.Owner.Opacity = 0.4;
                returnJourneyWindow.ShowDialog();
            });
        }
    }
}
