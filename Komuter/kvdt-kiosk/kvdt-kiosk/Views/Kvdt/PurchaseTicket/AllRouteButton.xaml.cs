using System.Windows;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Kvdt.PurchaseTicket
{
    /// <summary>
    /// Interaction logic for AllRouteButton.xaml
    /// </summary>
    public partial class AllRouteButton : UserControl
    {
        public AllRouteButton()
        {
            InitializeComponent();
        }

        private void BtnAll_Click(object sender, RoutedEventArgs e)
        {
            BtnAll.Style = (Style)FindResource("BtnSelected");
        }
    }
}
