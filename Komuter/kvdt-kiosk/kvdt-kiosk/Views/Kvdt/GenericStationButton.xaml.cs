using System.Windows;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Kvdt
{
    /// <summary>
    /// Interaction logic for GenericStationButton.xaml
    /// </summary>
    public partial class GenericStationButton : UserControl
    {
        public GenericStationButton()
        {
            InitializeComponent();
        }

        private void BtnGenericStation_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LblStationName_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lblStationName.Foreground = System.Windows.Media.Brushes.Crimson;
            lblStationName.FontWeight = FontWeights.Bold;
        }

        private void LblStationName_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lblStationName.Foreground = System.Windows.Media.Brushes.Black;
        }

    }
}
