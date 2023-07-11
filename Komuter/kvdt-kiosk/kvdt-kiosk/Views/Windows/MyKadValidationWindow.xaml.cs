using kvdt_kiosk.Models;
using kvdt_kiosk.Services;
using System.Windows;

namespace kvdt_kiosk.Views.Windows
{
    /// <summary>
    /// Interaction logic for MyKadValidationWindow.xaml
    /// </summary>
    public partial class MyKadValidationWindow : Window
    {
        public MyKadValidationWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MyDispatcher.Invoke(() =>
            {
                PassengerInfo.PassengerName = "";
            });
        }
    }
}
