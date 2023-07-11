using Komlink.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Komlink.Views
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
