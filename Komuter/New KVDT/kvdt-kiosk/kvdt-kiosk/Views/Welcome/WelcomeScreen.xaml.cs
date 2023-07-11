using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Welcome
{
    /// <summary>
    /// Interaction logic for WelcomeScreen.xaml
    /// </summary>
    public partial class WelcomeScreen : UserControl
    {
        public WelcomeScreen()
        {
            InitializeComponent();
        }


        private async void BtnStart3_Click(object sender, RoutedEventArgs e)
        {
            TxtShowNormal3.Text = "IN PROGRESS...";
            await Task.Delay(1000);

            BtnStart4.IsEnabled = false;

            var language = new LanguageScreen();
            Content = language;
        }

        private async void BtnStart4_Click(object sender, RoutedEventArgs e)
        {
            TxtShowNormal4.Text = "IN PROGRESS...";
            await Task.Delay(1000);

            var komlinkLanguage = new kvdt_kiosk.Views.Komlink.LanguageScreen();
            Content = komlinkLanguage;

            //ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.FileName = @"C:\NssITKiosk\Komlink\Komlink.exe";

            //Process.Start(startInfo);
        }

    }
}
