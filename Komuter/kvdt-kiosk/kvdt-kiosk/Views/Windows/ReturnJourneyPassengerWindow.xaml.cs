using System.Windows;

namespace kvdt_kiosk.Views.Windows
{
    /// <summary>
    /// Interaction logic for ReturnJourneyPassengerWindow.xaml
    /// </summary>
    public partial class ReturnJourneyPassengerWindow : Window
    {
        public ReturnJourneyPassengerWindow()
        {
            InitializeComponent();
            CloseReturnJournetWindow();
        }

        private void CloseReturnJournetWindow()
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //MyKadValidationWindow myKadValidationWindow = new MyKadValidationWindow();
            //MyKadValidationScreen myKadValidationScreen = new MyKadValidationScreen();

            //myKadValidationWindow.WindowStyle = WindowStyle.None;

            //myKadValidationWindow.Content = myKadValidationScreen;
            //myKadValidationWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            //myKadValidationWindow.ShowDialog();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            Window mainWindow = Application.Current.MainWindow;
            mainWindow.Effect = null;
            mainWindow.Opacity = 1;
        }
    }
}
