using kvdt_kiosk.Views.Windows;
using System.Windows;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Kvdt.Parcel
{
    /// <summary>
    /// Interaction logic for ParcelActionButton.xaml
    /// </summary>
    public partial class ParcelActionButton : UserControl
    {
        public ParcelActionButton()
        {
            InitializeComponent();
        }

        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            Window.GetWindow(this).Close();

            Window returnJourneyPassengerWindow = Application.Current.Windows[Application.Current.Windows.Count - 1];
            returnJourneyPassengerWindow.Close();

            //Window returnJourneyWindow = Application.Current.Windows[Application.Current.Windows.Count - 1];
            //returnJourneyWindow.Close();

            Window paymentWindow = new PaymentWindow();
            //paymentWindow.Owner = Window.GetWindow(this);

            //paymentWindow.Owner.Effect = new System.Windows.Media.Effects.BlurEffect();
            //paymentWindow.Owner.Opacity = 0.5;

            //CardPayWave cardPayWave = new CardPayWave();
            //paymentWindow.Content = cardPayWave;

            //paymentWindow.ShowDialog();

        }
    }
}
