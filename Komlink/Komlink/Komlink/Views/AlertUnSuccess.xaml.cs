using Komlink.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Komlink.Views
{
    /// <summary>
    /// Interaction logic for AlertUnSuccess.xaml
    /// </summary>
    public partial class AlertUnSuccess : UserControl
    {

        private static Lazy<AlertUnSuccess> alert = new Lazy<AlertUnSuccess>(() => new AlertUnSuccess());
        public event EventHandler<ScanEventArgs> ButtonScanAgainClicked;
        public event EventHandler ButtonNotScanAgainClicked;
        public static AlertUnSuccess GetUnSuccesAlertPage()
        {
            return alert.Value;
        }
        public AlertUnSuccess()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            ScanEventArgs scanEventArgs = new ScanEventArgs(isRetryScan: true, retryCount: 1);
            ButtonScanAgainClicked.Invoke(this, scanEventArgs);
            SystemConfig.IsResetIdleTimer = true;
        }

        public void HideClickAgainButton()
        {
            ClickAgainButton.Visibility = Visibility.Collapsed;
            OrText.Visibility = Visibility.Collapsed;
            SystemConfig.IsResetIdleTimer = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ButtonNotScanAgainClicked.Invoke(null, null);
            SystemConfig.IsResetIdleTimer = true;
        }
    }
}
