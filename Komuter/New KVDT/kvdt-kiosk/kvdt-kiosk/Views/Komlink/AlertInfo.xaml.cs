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
