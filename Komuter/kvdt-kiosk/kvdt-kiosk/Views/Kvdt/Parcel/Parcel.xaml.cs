using System.Windows;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Kvdt.Parcel
{
    /// <summary>
    /// Interaction logic for Parcel.xaml
    /// </summary>
    public partial class Parcel : UserControl
    {

        int parcelQuantiy;
        public Parcel()
        {
            InitializeComponent();

            parcelQuantiy = int.Parse(ParcelQuantityText.Text);
        }


        private void Btn_Plus_Clicked(object sender, RoutedEventArgs e)
        {
            parcelQuantiy++;
            SetParcelQuantity();
        }
        private void Btn_Minus_Clicked(object sender, RoutedEventArgs e)
        {
            if (parcelQuantiy > 0)
            {
                parcelQuantiy--;
                SetParcelQuantity();
            }

        }


        private void SetParcelQuantity()
        {
            ParcelQuantityText.Text = parcelQuantiy.ToString();
        }

    }
}
