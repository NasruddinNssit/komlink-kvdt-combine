using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult;
using NssIT.Kiosk.Tools.ThreadMonitor;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.VoidTransaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using NssIT.Kiosk.AppDecorator.Log;

namespace IM30TestAPI01
{
    /// <summary>
    /// Interaction logic for uscIM30VoidTransaction.xaml
    /// </summary>
    public partial class uscIM30VoidTransaction : UserControl
    {
        private const string _logChannel = "TestUI";

        private LibShowMessageWindow.MessageWindow _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;
        private ShowMessageLogDelg _showMessageDelgHandler = null;
        private string _comPort = null;

        public uscIM30VoidTransaction()
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

        public void SyncUpdateHistoryInvoice(string invNo, decimal chargeAmount, string posTransNo, string cardToken)
        {
            TxtInvNo.Text = invNo;
            TxtVoidAmount.Text = $@"{chargeAmount:#,###.00}";
            TxtPosTransNo.Text = posTransNo;
            TxtCardToken.Text = cardToken;
        }

        private bool _controlLoaded = false;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (_controlLoaded)
                return;
            _controlLoaded = true;
        }

        private IM30VoidTransaction _im30VoidTrans = null;
        private void VoidTrans_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (string.IsNullOrWhiteSpace(_comPort))
            {
                _msg.ShowMessage("Please select and apply a COM port.");
                return;
            }
            else if (string.IsNullOrWhiteSpace(TxtInvNo.Text))
            {
                _msg.ShowMessage("Please enter Invoice No.");
                return;
            }
            else if (string.IsNullOrWhiteSpace(TxtCardToken.Text))
            {
                _msg.ShowMessage("Invalid Card Token");
                return;
            }
            else if (decimal.TryParse(TxtVoidAmount.Text, out decimal voidRMAmount))
            {
                if (voidRMAmount <= 0)
                {
                    _msg.ShowMessage("Invalid Void Amount");
                }
                else 
                {
                    VoidTransactionTest01(TxtInvNo.Text.Trim(), voidRMAmount, TxtCardToken.Text.Trim());
                }
            }
            else
            {
                _msg.ShowMessage("Invalid Price Amount when Void Card Transaction. Value must be a decimal amount");
            }

            return;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            ///
            void VoidTransactionTest01(string invoiceNoX, decimal voidAmountX, string cardToken)
            {
                try
                {
                    if (_im30VoidTrans?.IsCurrentWorkingEnded == false)
                    {
                        _msg.ShowMessage("Dispose existing Void Card Transaction .. Please wait 10 seconds");
                        _im30VoidTrans.Dispose();

                        Thread.Sleep(5 * 1000);
                    }

                    _im30VoidTrans = new IM30VoidTransaction(_comPort, invoiceNoX, cardToken, voidAmountX,
                            new OnTransactionFinishedDelg(OnTransactionFinishedDelgWorking),
                            noActionMaxWaitSec: 60, _showMessageDelgHandler);

                    if (_im30VoidTrans.StartTransaction(out Exception ex2) == true)
                    {
                        _msg.ShowMessage("-Void Card Transaction has started~");
                    }
                    else if (ex2 != null)
                        throw new Exception($@"Error when run VoidTransactionTest01 (A); Invoice No.: {invoiceNoX}", ex2);

                    else
                        throw new Exception($@"Unable to Void Card Transaction (A)", ex2);
                }
                catch (Exception ex)
                {
                    _msg.ShowMessage(ex.ToString());
                }
            }

            void OnTransactionFinishedDelgWorking(IIM30TransResult transResult)
            {
                if (_im30VoidTrans == null)
                    return;

                string processId = "*";

                this.Dispatcher.Invoke(new Action(() => 
                {
                    processId = "VoidCardTransInv_" + TxtInvNo.Text.Trim();
                }));

                RunThreadMan tMan = new RunThreadMan(new Action(() =>
                {
                    try
                    {
                        if (_im30VoidTrans.FinalResult != null)
                        {
                            if (_im30VoidTrans.FinalResult.IsSuccess)
                                _msg.ShowMessage($@"Void Card Transaction Success; Process Id : {processId}");

                            else if (_im30VoidTrans.FinalResult.IsTimeout)
                                _msg.ShowMessage($@"Transaction timeout.");

                            else if (_im30VoidTrans.FinalResult.IsManualStopped)
                                _msg.ShowMessage($@"Transaction has been stopped and terminated.");

                            else
                            {
                                if (_im30VoidTrans.FinalResult.Error != null)
                                    _msg.ShowMessage($@"Error when Void Card Transaction; Process Id : {processId}{"\r\n"}" + _im30VoidTrans.FinalResult.Error.ToString());

                                else
                                    _msg.ShowMessage($@"Error when Void Card Transaction; Unknown error; Process Id : {processId}\r\n");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //CYA-DEBUG .. Log Error Here
                    }
                }), processId, 5, _logChannel
                , threadPriority: System.Threading.ThreadPriority.AboveNormal);
            }
        }


    }
}
