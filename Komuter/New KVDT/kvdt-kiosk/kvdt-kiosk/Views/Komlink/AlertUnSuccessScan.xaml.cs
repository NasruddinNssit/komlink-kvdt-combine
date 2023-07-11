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
    /// Interaction logic for AlertUnSuccessScan.xaml
    /// </summary>
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
