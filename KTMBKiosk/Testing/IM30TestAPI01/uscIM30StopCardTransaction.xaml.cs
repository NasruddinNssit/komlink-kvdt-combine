using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult;
using NssIT.Kiosk.AppDecorator.Log;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Base;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.StopTransaction;
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

namespace IM30TestAPI01
{
    /// <summary>
    /// Interaction logic for uscIM30StopCardTransaction.xaml
    /// </summary>
    public partial class uscIM30StopCardTransaction : UserControl
    {
        private LibShowMessageWindow.MessageWindow _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;
        private ShowMessageLogDelg _showMessageDelgHandler = null;
        private string _comPort = null;
        public uscIM30StopCardTransaction()
        {
            InitializeComponent();
            _showMessageDelgHandler = new ShowMessageLogDelg(ShowMessageDelgWorking);
        }

        private void ShowMessageDelgWorking(string message)
        {
            _msg.ShowMessage(message);
        }

        public void SetCOMPort(string comPort)
        {
            _comPort = comPort;
        }

        private void StopCardTransIM30_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (string.IsNullOrWhiteSpace(_comPort))
            {
                _msg.ShowMessage("Please select and apply a COM port.");
                return;
            }

            Guid processId = Guid.NewGuid();
            IM30StopTransaction newIM30Trans = null;

            CreateStopStransactionTest01();

            return;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            ///
            void CreateStopStransactionTest01()
            {
                try
                {

                    newIM30Trans = new IM30StopTransaction(_comPort, "TestX02_" + DateTime.Now.ToString("HHmmss_fffffff"), new OnTransactionFinishedDelg(OnTransactionFinishedDelgWorking)
                        , noActionMaxWaitSec: 10, _showMessageDelgHandler);
                    newIM30Trans.StartTransaction(out Exception ex2);
                    if (ex2 != null)
                        throw new Exception("Error when run CreateNewPingTest01; (A)", ex2);

                }
                catch (Exception ex)
                {
                    _msg.ShowMessage(ex.ToString());
                }
            }

            void OnTransactionFinishedDelgWorking(IIM30TransResult transResult)
            {
                if (newIM30Trans == null)
                    return;

                if (newIM30Trans.FinalResult != null)
                {
                    if (newIM30Trans.FinalResult.IsSuccess)
                        _msg.ShowMessage($@"Stop Stransaction of Card Reader Success; Process Id : {processId}");

                    else
                    {
                        if (newIM30Trans.FinalResult.Error != null)
                            _msg.ShowMessage($@"Error when Stop Stransaction of Card Reader; Process Id : {processId}{"\r\n"}" + newIM30Trans.FinalResult.Error.ToString());

                        else
                            _msg.ShowMessage($@"Error when Stop Stransaction of Card Reader; Unknown error; Process Id : {processId}\r\n");
                    }
                }
            }
        }

        private void DisableDebugTest_Click(object sender, RoutedEventArgs e)
        {
            IM30RequestResponseDataWorks.SetDebugTesting(false);
        }

        private void EnableDebugTest_Click(object sender, RoutedEventArgs e)
        {
            IM30RequestResponseDataWorks.SetDebugTesting(true);
        }
    }
}
