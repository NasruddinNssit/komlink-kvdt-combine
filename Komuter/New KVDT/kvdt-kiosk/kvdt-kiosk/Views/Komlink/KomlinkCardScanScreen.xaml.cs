using kvdt_kiosk.Models.Komlink;
using kvdt_kiosk.Views.Komlink;
using Serilog;
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
    /// Interaction logic for KomlinkCardScreen.xaml
    /// </summary>
    public partial class KomlinkCardScanScreen : UserControl
    {

        ExitButton exitButton;
        BigAlert bigAlert;

        private AlertUnSuccessScan unSuccessScan;
        public KomlinkCardScanScreen()
        {
            InitializeComponent();
            exitButton = new ExitButton();
            ExitButtonGrid.Children.Add(exitButton);
          
            bigAlert = BigAlert.GetBigAlertPage();

            unSuccessScan =  AlertUnSuccessScan.GetUnSuccessScan();
            bigAlert.onFailureScanCard += BigAlert_onFailureScanCard;

            unSuccessScan.ButtonScanAgainClicked += UnSuccessScan_ButtonScanAgainClicked;
        }

        private void UnSuccessScan_ButtonScanAgainClicked(object sender, ScanEventArgs e)
        {
            FrmSubFrame.Content = null;
            FrmSubFrame.NavigationService.RemoveBackEntry();
            FrmSubFrame.NavigationService.Content = null;

            System.Windows.Forms.Application.DoEvents();

            FrmSubFrame.NavigationService.Navigate(bigAlert);
            BdSubFrame.Visibility = Visibility.Visible;
            System.Windows.Forms.Application.DoEvents();
        }

        private void BigAlert_onFailureScanCard(object sender, EventArgs e)
        {
            FrmSubFrame.Content = null;
            FrmSubFrame.NavigationService.RemoveBackEntry();
            FrmSubFrame.NavigationService.Content = null;

            System.Windows.Forms.Application.DoEvents();


            FrmSubFrame.NavigationService.Navigate(unSuccessScan);
            BdSubFrame.Visibility = Visibility.Visible;
            System.Windows.Forms.Application.DoEvents();
        }

    


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            FrmSubFrame.Content = null;
            FrmSubFrame.NavigationService.RemoveBackEntry();
            FrmSubFrame.NavigationService.Content = null;
            System.Windows.Forms.Application.DoEvents();


            FrmSubFrame.NavigationService.Navigate(bigAlert);
            BdSubFrame.Visibility = Visibility.Visible;
        }
    }
}
