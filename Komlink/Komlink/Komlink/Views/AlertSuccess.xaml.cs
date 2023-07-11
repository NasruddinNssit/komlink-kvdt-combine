using Komlink.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Komlink.Views
{
    /// <summary>
    /// Interaction logic for AlertSuccess.xaml
    /// </summary>
    public partial class AlertSuccess : UserControl
    {



        private static Lazy<AlertSuccess> alertSucces = new Lazy<AlertSuccess>(() => new AlertSuccess());

        public event EventHandler OnNoPrintClicked;
        public event EventHandler OnYesPrintClicked;

        public static AlertSuccess GetAlertSuccess()
        {
            return alertSucces.Value;
        }


        public AlertSuccess()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;
            OnNoPrintClicked?.Invoke(this, EventArgs.Empty);
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;
            OnYesPrintClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
