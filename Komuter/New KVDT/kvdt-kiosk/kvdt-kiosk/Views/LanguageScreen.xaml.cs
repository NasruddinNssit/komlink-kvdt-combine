using kvdt_kiosk.Models;
using Serilog;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace kvdt_kiosk.Views
{
    /// <summary>
    /// Interaction logic for LanguageScreen.xaml
    /// </summary>
    public partial class LanguageScreen
    {
        private DispatcherTimer timer;

        public LanguageScreen()
        {
            InitializeComponent();

            UserSession.SessionId = System.DateTime.Now.ToString("ddMMyyyyHHmmss");
        }

        private async void BtnMalay_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                lblBMLang.Text = "Please wait...";
                this.IsEnabled = false;
                await Task.Delay(250);
                this.IsEnabled = true;

                var window = Window.GetWindow(this);
                window.Content = new ChooseActionScreen();

                App.Language = "ms";
                Log.Logger.Information(UserSession.SessionId + " Malay Language Selected");

            }
            catch (System.Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in LanguageScreen.xaml.cs");
            }

        }

        private async void BtnEnglish_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lblENLang.Text = "Please wait...";
                this.IsEnabled = false;
                await Task.Delay(250);
                this.IsEnabled = true;

                var window = Window.GetWindow(this);
                window.Content = new ChooseActionScreen();

                App.Language = "en";
                Log.Logger.Information(UserSession.SessionId + " English Language Selected");

            }
            catch (System.Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in LanguageScreen.xaml.cs");
            }
        }

        private void InfoMessage()
        {
            // TxtInfo.Text = msg;

            //TxtInfo.BeginAnimation(System.Windows.Controls.TextBlock.OpacityProperty, blink);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InfoMessage();

            if (SystemConfig.IsIm30Available)
            {
                lblPaymentDevice.Text = "Payment Device Ready";
            }
            else
            {
                lblPaymentDevice.Text = "Payment Device Not Found";
                lblPaymentDevice.Foreground = System.Windows.Media.Brushes.Yellow;
            }
        }

    }
}
