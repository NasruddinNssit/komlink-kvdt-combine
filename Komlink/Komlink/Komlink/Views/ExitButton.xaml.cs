using Komlink.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
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

namespace Komlink.Views
{
    /// <summary>
    /// Interaction logic for ExitButton.xaml
    /// </summary>
    public partial class ExitButton : UserControl
    {

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public ExitButton()
        {
            InitializeComponent();

            LoadLanguage();
        }


        private void LoadLanguage()
        {
            try
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (App.Language == "ms")
                    {
                        ExitButtonText.Content = "Keluar";
                    }
                });
            }catch (Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in ExitButton.xaml.cs");

            }
        }

        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {

            UserSession.ClearSession();
           
            StopTransaction();
            Application.Current.Shutdown();
            //ShowWindow(App)

        }

        private async void StopTransaction()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = "http://127.0.0.1:1234/Para=18";
                    var response = await client.GetStringAsync(url);
                }
            }
            catch (Exception ex)
            {

            }
        }

    }
}
