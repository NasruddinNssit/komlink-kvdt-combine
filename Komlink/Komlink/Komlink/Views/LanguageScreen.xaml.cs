
using Komlink.Models;
using Microsoft.Reporting.WinForms;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using QRCoder;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;



namespace Komlink.Views
{
    /// <summary>
    /// Interaction logic for LanguageScreen.xaml
    /// </summary>
    public partial class LanguageScreen : System.Windows.Controls.UserControl
    {
        private static List<Stream> m_streams;
        private static int m_currentPageIndex = 0;
        public LanguageScreen()
        {
            InitializeComponent();

            //TestPrint();
            UserSession.SessionId = System.DateTime.Now.ToString("ddMMyyyyHHmmss");
            //PrintReport("C:\\Users\\moham\\source\\repos\\NSSIT\\KTMB_KTMBKiosk\\Komlink\\Komlink\\Komlink\\Reports\\TopUpKomlinkReceipt.rdlc");

            //PrintReportQrCode("C:\\Users\\moham\\source\\repos\\NSSIT\\KTMB_KTMBKiosk\\Komlink\\Komlink\\Komlink\\Reports\\Ticket.rdlc");
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

