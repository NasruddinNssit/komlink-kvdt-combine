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
    /// Interaction logic for KeyPad.xaml
    /// </summary>
    public partial class KeyPad : UserControl
    {

        public event EventHandler KeyPadClicked;

        public event EventHandler KeyPadDeletedClicked;

        public event EventHandler KeyPadEnterClicked;

        public event EventHandler keyPadCancelClicked;
        public KeyPad()
        {
            InitializeComponent();

        }

        private void KeyPad_Clicked(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;


            KeyPadClicked?.Invoke(button, e);
        }

        public void UpdateEnterButton()
        {
            EnterBorder.Background = Brushes.Green;
            EnterButton.IsEnabled = true;
            SystemConfig.IsResetIdleTimer = true;
        }

        public void DisableEnterButton()
        {
            EnterBorder.Background = Brushes.Gray;
            EnterButton.IsEnabled = false;
        }

        private void KeyPadDelete_Clicked(object sender, RoutedEventArgs e)
        {
            KeyPadDeletedClicked?.Invoke(sender, e);
            SystemConfig.IsResetIdleTimer = true;
        }

        private void KeyPadEnter_Clicked(object sender, RoutedEventArgs e)
        {
            KeyPadEnterClicked?.Invoke(sender, e);
            SystemConfig.IsResetIdleTimer = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            keyPadCancelClicked?.Invoke(sender, e);
            SystemConfig.IsResetIdleTimer = true;
        }
    }
}
