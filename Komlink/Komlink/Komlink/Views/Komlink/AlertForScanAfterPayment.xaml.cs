using Komlink.Models;
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

namespace Komlink.Views.Komlink
{
    /// <summary>
    /// Interaction logic for AlertForScanAfterPayment.xaml
    /// </summary>
    public partial class AlertForScanAfterPayment : UserControl
    {
        public AlertForScanAfterPayment()
        {
            InitializeComponent();
            LoadLanguage();
        }


        private void LoadLanguage()
        {
            try
            {
                if (App.Language == "ms")
                {
                    ScanBtnText.Content = "Cetak";
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

        }
    }
}
