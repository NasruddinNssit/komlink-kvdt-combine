using kvdt_kiosk.Models.Komlink;
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

namespace kvdt_kiosk.Views.Komlink
{
    /// <summary>
    /// Interaction logic for BackButton.xaml
    /// </summary>
    public partial class BackButton : UserControl
    {
        public event EventHandler BackButtonClicked;

        public BackButton()
        {
            InitializeComponent();

            LoadLanguage();
        }

        private void LoadLanguage()
        {
            try
            {
                if (App.Language == "ms")
                {
                    BackBtnText.Content = "Kembali";
                }

            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in BackButton.xaml.cs");

            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Button button = (Button)sender;

            BackButtonClicked?.Invoke(button, e);

            SystemConfig.IsResetIdleTimer = true;
        }

    }
}
