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
    /// Interaction logic for AlertSuccess.xaml
    /// </summary>
    public partial class AlertSuccess : UserControl
    {



        private static Lazy<AlertSuccess> alertSucces = new Lazy<AlertSuccess>(() => new AlertSuccess());

        public event EventHandler OnNoPrintClicked;
        public event EventHandler OnYesPrintClicked;

        public static AlertSuccess GetAlertSuccess()
        {
            return alertSucces.Value;
        }


        public AlertSuccess()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;
            OnNoPrintClicked?.Invoke(this, EventArgs.Empty);
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;
            OnYesPrintClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
