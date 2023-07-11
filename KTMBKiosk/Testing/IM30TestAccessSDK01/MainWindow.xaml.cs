using NssIT.Kiosk.AppDecorator.Global;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IM30TestAccessSDK01
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LibShowMessageWindow.MessageWindow _msg = null;

        public MainWindow()
        {
            SysGlobalLock.Init();
            InitializeComponent();
            _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] portX = SerialPort.GetPortNames();

            string[] ports = (from prt in portX
                              orderby prt
                              select prt).ToArray();

            foreach (string portStr in ports)
                CboComPort.Items.Add(portStr);

            CboComPort.Items.Add("Problem COM Port");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //IM30CardSale.Shutdown();
            //IM30GetDeviceInfo.Shutdown();
        }

        private void ApplyPortSetting_Click(object sender, RoutedEventArgs e)
        {
            if (CboComPort.SelectedItem is string theCOMPort)
            {
                //UscPing01.SetCOMPort(theCOMPort.ToUpper().Trim());
                UscCardSale01.SetCOMPort(theCOMPort.ToUpper().Trim());
                UscGetDeviceInfo01.SetCOMPort(theCOMPort.ToUpper().Trim());
                //UscStopCardTransaction01.SetCOMPort(theCOMPort.ToUpper().Trim());
                UscGetLastTrans01.SetCOMPort(theCOMPort.ToUpper().Trim());
                //UscReboot01.SetCOMPort(theCOMPort.ToUpper().Trim());
                UscCardSettlement01.SetCOMPort(theCOMPort.ToUpper().Trim());
                UscVoidTransaction01.SetCOMPort(theCOMPort.ToUpper().Trim());

                _msg.ShowMessage("Selected COM port applied to all Tab page.");
            }
            else
            {
                _msg.ShowMessage("Please select a COM port");
            }
        }

        private void SyncCreditDebitSaleToVoidTrans_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UscCardSale01.QueryHisCreditDebitInvoice(out string invNo, out decimal chargeAmount, out string posTransNo, out string cardToken))
                {
                    UscVoidTransaction01.SyncUpdateHistoryInvoice(invNo, chargeAmount, posTransNo, cardToken);
                    _msg.ShowMessage("-Sync. CreditDebit Sale To Void Trans. DONE~");
                }
                else
                {
                    _msg.ShowMessage("-Unable to sync. CreditDebit Sale To Void Trans.~");
                }
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }
    }
}
