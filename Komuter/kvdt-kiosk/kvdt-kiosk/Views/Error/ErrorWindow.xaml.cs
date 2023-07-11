using System.Windows;

namespace kvdt_kiosk.Views.Error
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public ErrorWindow(string errorMessage)
        {
            InitializeComponent();
        }

        private void ErrorWindow1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            this.Close();
        }
    }
}
