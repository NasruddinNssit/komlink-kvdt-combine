using kvdt_kiosk.Models;
using kvdt_kiosk.Views.Welcome;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace kvdt_kiosk.Views.Idle
{
    /// <summary>
    /// Interaction logic for ClosingScreen.xaml
    /// </summary>
    public partial class ClosingScreen : UserControl
    {
        DispatcherTimer timer = new DispatcherTimer();
        public ClosingScreen()
        {
            InitializeComponent();

            if (timer.IsEnabled == false)
            {
                timer.Start();
            }
            CountingToClose();
        }

        public bool IsActivity { get; internal set; }

        public void CountingToClose()
        {
            var count = 30;
            TxtTimeoutMessage.Text = "This application will be closed within ";
            TxtCountDown.Text = count.ToString();


            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                count--;
                TxtCountDown.Text = count.ToString();

                if (count <= 5)
                {
                    TxtCountDown.Foreground = System.Windows.Media.Brushes.Red;
                }

                if (count == 0)
                {
                    timer.Stop();
                    Application.Current.Shutdown();
                }
            };

            timer.Start();
        }

        private void BdContinue_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;

            timer.Stop();

            Window.GetWindow(this).Close();
        }

        private void BdExit_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //get parent window
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Content = new WelcomeScreen();
            }
        }
    }

}
