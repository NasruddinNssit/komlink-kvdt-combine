using kvdt_kiosk.Views.Komlink;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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


        private void BtnStart3_Click(object sender, RoutedEventArgs e)
        {
           
            BtnStart4.IsEnabled = false;

            var language = new LanguageScreen();
            Content = language;
        }

        private void BtnStart4_Click(object sender, RoutedEventArgs e)
        {
           
            BtnStart3.IsEnabled = false;
            var komlinkLanguage = new kvdt_kiosk.Views.Komlink.LanguageScreen();
            Content = komlinkLanguage;

            //ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.FileName = @"C:\NssITKiosk\Komlink\Komlink.exe";

            //Process.Start(startInfo);
        }

    }
}
