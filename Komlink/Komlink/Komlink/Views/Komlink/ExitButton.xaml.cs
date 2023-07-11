using Komlink.Models;
using Serilog;
using System;
using System.Collections.Generic;
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

namespace Komlink.Views.Komlink
{
    /// <summary>
    /// Interaction logic for ExitButton.xaml
    /// </summary>
    public partial class ExitButton : UserControl
    {
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

            //UserSession.EndSession();
            UserSession.CurrentUserSession = null;

           Application.Current.Shutdown();
        }

    }
}
