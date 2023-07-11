using Komlink.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Komlink.Views
{
    /// <summary>
    /// Interaction logic for AlertUnSuccessScan.xaml
    /// </summary>
    /// 

    public partial class AlertUnSuccessScan : UserControl
    {

        public event EventHandler<ScanEventArgs> ButtonScanAgainClicked;

        private static Lazy<AlertUnSuccessScan> alert = new Lazy<AlertUnSuccessScan>(() => new AlertUnSuccessScan());

        public static AlertUnSuccessScan GetUnSuccessScan()
        {
            return alert.Value;
        }

        public AlertUnSuccessScan()
        {
            InitializeComponent();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {

            ScanEventArgs scanEventArgs = new ScanEventArgs(isRetryScan: true, retryCount: 1);
            ButtonScanAgainClicked.Invoke(this, scanEventArgs);
            SystemConfig.IsResetIdleTimer = true;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            BdClickAgain.Style = FindResource("ProgressBorder") as Style;
            BtnClickAgain.Style = FindResource("ButtonProgress") as Style;

            if (App.Language == "ms")
            {
                BtnClickAgain.Content = "Sedang Dijalankan..";
            }


            BdClickAgain.Style = FindResource("BorderStyle") as Style;
            BtnClickAgain.Style = FindResource("ButtonStyle") as Style;

            if (App.Language == "ms")
            {
                BtnClickAgain.Content = "Imbas Lagi";
            }

            SystemConfig.IsResetIdleTimer = true;
        }
    }
}
