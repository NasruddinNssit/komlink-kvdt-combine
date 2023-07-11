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
    /// Interaction logic for AlertForScanAfterPayment.xaml
    /// </summary>
    public partial class AlertForScanAfterPayment : UserControl
    {

        public event EventHandler<ScanEventArgs> ButtonScanClicked;

        private static Lazy<AlertForScanAfterPayment> alert = new Lazy<AlertForScanAfterPayment>(() => new AlertForScanAfterPayment());


        public static AlertForScanAfterPayment GetAlertPage()
        {
            return alert.Value;
        }
        public AlertForScanAfterPayment()
        {
            InitializeComponent();
            LoadLanguage();
        }


        private void LoadLanguage()
        {
            SystemConfig.IsResetIdleTimer = true;

            try
            {
                if (App.Language == "ms")
                {
                    ScanBtnText.Content = "Imbas";
                    AlertTitle.Text = "Sila imbas Kad Komlink anda pada pembaca untuk meneruskan transaksi";
                }

            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in AlerForScanAfterPayment.xaml.cs");

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;

            ScanEventArgs scanEventArgs = new ScanEventArgs(isRetryScan: false, retryCount: 0);
            ButtonScanClicked?.Invoke(null, scanEventArgs);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
