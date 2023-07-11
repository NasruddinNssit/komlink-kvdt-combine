using Komlink.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Komlink.Views
{
    /// <summary>
    /// Interaction logic for AlertInfo.xaml
    /// </summary>
    public partial class AlertInfo : UserControl
    {

        private static Lazy<AlertInfo> _alertInfo = new Lazy<AlertInfo>(() => new AlertInfo());

        public event EventHandler closeAlert;

        public static AlertInfo GetAlertInfo() { return _alertInfo.Value; }
        public AlertInfo()
        {
            InitializeComponent();
            SystemConfig.IsResetIdleTimer = true;
        }


        public void SetTextAlert(string text)
        {
            alertWarningText.Text = text;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            closeAlert?.Invoke(null, null);
            SystemConfig.IsResetIdleTimer = true;
        }
    }
}
