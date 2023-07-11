using kvdt_kiosk.Models.Komlink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace kvdt_kiosk.Views.Komlink
{
    /// <summary>
    /// Interaction logic for ConfirmButton.xaml
    /// </summary>
    public partial class ConfirmButton : UserControl
    {

        public event EventHandler ButtonConfirmClicked;
        public ConfirmButton()
        {
            InitializeComponent();
        }

        private void Confirm_Button_Click(object sender, RoutedEventArgs e)
        {
            ButtonConfirmClicked?.Invoke(this, EventArgs.Empty);
            SystemConfig.IsResetIdleTimer = true;
        }

        private void CheckDateIsValid()
        {

        }
    }
}
