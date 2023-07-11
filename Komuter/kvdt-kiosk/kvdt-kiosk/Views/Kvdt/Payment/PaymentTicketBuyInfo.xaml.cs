using kvdt_kiosk.Views.Windows;
using System.Windows;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Kvdt.Payment
{
    /// <summary>
    /// Interaction logic for PaymentTicketBuyInfo.xaml
    /// </summary>
    public partial class PaymentTicketBuyInfo : UserControl
    {
        public PaymentTicketBuyInfo()
        {
            InitializeComponent();
        }

        private void BtnCard_Click(object sender, RoutedEventArgs e)
        {
            PaymentWindow paymentWindow = new PaymentWindow();
            CardPayWave cardPayWave = new CardPayWave();
            paymentWindow.Content = cardPayWave;

            paymentWindow.ShowDialog();
        }
    }
}
