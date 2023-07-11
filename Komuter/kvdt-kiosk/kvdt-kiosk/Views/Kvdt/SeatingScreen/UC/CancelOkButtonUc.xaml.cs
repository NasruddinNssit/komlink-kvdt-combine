using kvdt_kiosk.Services;
using kvdt_kiosk.Views.Kvdt.ReturnJourney;
using kvdt_kiosk.Views.Windows;
using System.Windows;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Kvdt.SeatingScreen.UC
{
    /// <summary>
    /// Interaction logic for CancelOkButtonUc.xaml
    /// </summary>
    public partial class CancelOkButtonUc : UserControl
    {
        public CancelOkButtonUc()
        {
            InitializeComponent();
        }

        private void BtnCancle_Click(object sender, RoutedEventArgs e)
        {

            Window returnJourneyWindow = Window.GetWindow(this);

            returnJourneyWindow.Owner.Effect = null;
            returnJourneyWindow.Owner.Opacity = 1;

            returnJourneyWindow.Close();

        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            MyDispatcher.Invoke(new System.Action(() =>
            {
                ReturnJourneyPassengerWindow returnJourneyPassengerWindow = new ReturnJourneyPassengerWindow();
                PassengerReturnJourney passengerReturnJourneyScreen = new PassengerReturnJourney();

                returnJourneyPassengerWindow.Owner = Window.GetWindow(this);
                returnJourneyPassengerWindow.Content = passengerReturnJourneyScreen;
                returnJourneyPassengerWindow.WindowStyle = WindowStyle.None;
                returnJourneyPassengerWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                returnJourneyPassengerWindow.ShowDialog();
            }));
        }

    }
}
