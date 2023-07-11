using Komlink.Models;
using Komlink.Views.Komlink.Payment.PayWave;
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
    /// Interaction logic for PaymentOption.xaml
    /// </summary>
    public partial class PaymentOption : UserControl
    {

        public event EventHandler ButtonPayWaveClicked;
        public PaymentOption()
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
                    PaymentOptionText.Text = "Pilihan Pembayaran";
                }

            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in PaymentOption.xaml.cs");

            }
        }

        private void BtnCardWave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ButtonPayWaveClicked?.Invoke(this, e);
        }
    }
}
