using kvdt_kiosk.Models.Komlink;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace kvdt_kiosk.Views.Komlink
{
    /// <summary>
    /// Interaction logic for LanguageScreen.xaml
    /// </summary>
    public partial class LanguageScreen : UserControl
    {
        private static List<Stream> m_streams;
        private static int m_currentPageIndex = 0;
        public LanguageScreen()
        {
            InitializeComponent();

            UserSession.SessionId = System.DateTime.Now.ToString("ddMMyyyyHHmmss");
            
        }

        private void BtnEnglish_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dispatcher.InvokeAsync(() =>
                {
                    Window.GetWindow(this).Content = new KomlinkCardScanScreen();

                    Log.Logger.Information(UserSession.SessionId + " English Language Selected");
                });
            }
            catch (System.Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in LanguageScreen.xaml.cs");
            }
        }

        private void BtnMalay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dispatcher.InvokeAsync(() =>
                {
                    App.Language = "ms";
                    Window.GetWindow(this).Content = new KomlinkCardScanScreen();
                });
                Log.Logger.Information(UserSession.SessionId + " Malay Language Selected");

            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in LanguageScreen.xaml.cs");

            }
        }
    }
}
