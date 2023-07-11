using kvdt_kiosk.Models.Komlink;
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
