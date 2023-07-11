using System;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Payment
{
    /// <summary>
    /// Interaction logic for PaymentInfoScreen.xaml
    /// </summary>
    public partial class PaymentInfoScreen : UserControl
    {
        public event EventHandler startPayWavePaymentbtn;
        //public event EventHandler startEWalletPaymentbtn;

        public event EventHandler StartTngEwalletPaymentBtn;
        public event EventHandler StartBoostEwalletPaymentBtn;
        public PaymentInfoScreen()
        {
            InitializeComponent();

            PaymentTicketBuyInfo paymentTicketBuyInfo = new PaymentTicketBuyInfo();

            paymentTicketBuyInfo.StartPayWavePayment += startPayWavePayment;
            paymentTicketBuyInfo.StartTngEwalletPayment += StartTngEwalletPayment;
            paymentTicketBuyInfo.StartBoostEwalletPayment += StartBoostEwalletPayment;
            GridPaymentInfo.Children.Add(paymentTicketBuyInfo);
        }
        private void StartTngEwalletPayment(object sender, EventArgs e)
        {
            StartTngEwalletPaymentBtn.Invoke(null, null);
        }
       
        private void StartBoostEwalletPayment(object sende, EventArgs e)
        {
            StartBoostEwalletPaymentBtn.Invoke(null, null);
        }

        private void startPayWavePayment(object sender, EventArgs e)
        {
            startPayWavePaymentbtn.Invoke(null, null);
        }

    }
}
