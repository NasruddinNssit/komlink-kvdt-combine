using kvdt_kiosk.Services;
using kvdt_kiosk.Views.Kvdt.PurchaseTicket;
using System.Windows;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Kvdt
{
    /// <summary>
    /// Interaction logic for ChooseActionScreen.xaml
    /// </summary>
    public partial class ChooseActionScreen : UserControl
    {
        public ChooseActionScreen()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PurchaseTicketScreen purchaseTicketScreen = new PurchaseTicketScreen();

            MyDispatcher.Invoke(() =>
            {
                this.Content = purchaseTicketScreen;
            });
        }
    }
}
