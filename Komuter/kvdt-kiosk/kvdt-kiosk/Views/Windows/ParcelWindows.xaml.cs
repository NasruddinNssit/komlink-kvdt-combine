using kvdt_kiosk.Views.Kvdt.Parcel;
using System.Windows;

namespace kvdt_kiosk.Views.Windows
{
    /// <summary>
    /// Interaction logic for ParcelWindows.xaml
    /// </summary>
    public partial class ParcelWindows : Window
    {
        public ParcelWindows()
        {
            InitializeComponent();
            GetParcelButton();
        }

        private void GetParcelButton()
        {
            ParcelScreen parcelScreen = new ParcelScreen();
            GridParcel.Children.Add(parcelScreen);
        }
    }
}
