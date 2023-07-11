using Komlink.Models;
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
using System.Windows.Threading;

namespace Komlink.Views.Idle
{
    /// <summary>
    /// Interaction logic for TimeOutScreen.xaml
    /// </summary>
    public partial class TimeOutScreen : UserControl
    {

        DispatcherTimer timer = new DispatcherTimer();

        public bool IsActivity { get; internal set; }
        public TimeOutScreen()
        {
            InitializeComponent();

            if(timer.IsEnabled == false)
            {
                timer.Start();
            }

            CountingToClose();
            LoadLanguange();
        }


        private void LoadLanguange()
        {
            if (App.Language == "ms")
            {
                TxtTimeoutMessage.Text = "Aplikasi ini akan ditutup dalam masa";
                ContinueText.Text = "TERUSKAN TRANSAKSI";
                ExitText.Text = "KELUAR";
            }
        }


        public void CountingToClose()
        {
            var count = 30;

            if (App.Language == "ms")
            {
                TxtTimeoutMessage.Text = "Aplikasi ini akan ditutup dalam masa ";
            }
            else
            {
                TxtTimeoutMessage.Text = "This application will be close within ";

            }
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

                if(count == 0)
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
            Application.Current.Shutdown();
        }

    }
}
