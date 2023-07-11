using Komlink.Models;
using Serilog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Komlink.Views
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
