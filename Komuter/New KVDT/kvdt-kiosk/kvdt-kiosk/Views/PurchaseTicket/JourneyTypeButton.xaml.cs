using kvdt_kiosk.Models;
using Serilog;
using System;
using System.Windows;

namespace kvdt_kiosk.Views.PurchaseTicket
{
    /// <summary>
    /// Interaction logic for JourneyTypeButton.xaml
    /// </summary>
    public partial class JourneyTypeButton
    {
        public event EventHandler<bool> OpenPgKomuterPax;

        public JourneyTypeButton()
        {
            InitializeComponent();
            LoadLanguage();
        }

        private void LoadLanguage()
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (App.Language != "ms") return;
                if (lblJourney.Text == "Single Journey")
                {
                    lblJourney.Text = "SEHALA";
                }

                if (lblJourney.Text == "Return Journey")
                {
                    lblJourney.Text = "DUA HALA";
                }
            });
        }
        private async void BtnJourney_Click(object sender, RoutedEventArgs e)
        {
            UserSession.ChildSeat = 0;
            UserSession.SeniorSeat = 0;

            UserSession.IsCheckOut = false;

            UserSession.JourneyType = lblJourney.Text;
            UserSession.JourneyTypeId = lblJourneyId.Text;
            UserSession.JourneyDuration = lblJourneyDate.Text;

            var resetStyle = FindResource("BtnReset") as Style;
            var selectedStyle = FindResource("BtnSelectedJourney") as Style;

            PassengerInfo.JourneyButtonText = lblJourneyId.Text;

            // BtnJourney.Style = selectedStyle;

            //var window = new Window
            //{
            //    Content = new PgKomuterPax(),

            //    WindowStyle = WindowStyle.None,
            //    Width = 710,
            //    Height = 1000,
            //    WindowStartupLocation = WindowStartupLocation.CenterScreen,
            //    WindowState = WindowState.Normal,
            //    Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#074481")),
            //    Owner = Application.Current.MainWindow,
            //    ResizeMode = ResizeMode.NoResize
            //};

            //window.Owner.Effect = new System.Windows.Media.Effects.BlurEffect();
            //window.Owner.Opacity = 0.4;

            try
            {
                //  this.OpenPgKomuterPax?.Invoke(this, true);
                //  window.ShowDialog();

                Log.Logger.Information(UserSession.SessionId + " User selected " + lblJourney.Text);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in JourneyTypeButton.xaml.cs");
            }
        }

    }
}
