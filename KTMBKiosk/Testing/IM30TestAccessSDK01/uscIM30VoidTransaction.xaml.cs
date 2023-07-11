using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.CreditDebit;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult;
using NssIT.Kiosk.AppDecorator.Log;
using NssIT.Kiosk.Device.PAX.IM30.AccessSDK;
using NssIT.Kiosk.Device.PAX.IM30.AccessSDK.Base.AxCommandSet;
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

namespace IM30TestAccessSDK01
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
        private IM30PortManagerAx _im30PortMan = null;

        public uscIM30VoidTransaction()
        {
            InitializeComponent();
            _showMessageDelgHandler = new ShowMessageLogDelg(ShowMessageDelgWorking);
        }

        public void SetCOMPort(string comPort)
        {
            _comPort = comPort;
        }

        private IM30PortManagerAx IM30PortMan
        {
            get
            {
                if (_im30PortMan is null)
                {
                    if (string.IsNullOrWhiteSpace(_comPort))
                        throw new Exception("COM Port not set yet.");

                    _im30PortMan = IM30PortManagerAx.GetAxPortManager(_comPort);

                    IM30PortMan.SetOnDebugShowMessageHandler(new ShowMessageLogDelg(ShowMessageDelgWorking));
                    //IM30PortMan.SetOnClientTransactionFinishedHandler(new OnTransactionFinishedDelg(OnTransactionFinishedDelgWorking));
                    //IM30PortMan.SetOnClientCardDetectedHandler(new NssIT.ACG.AppDecorator.DomainLibs.IM30.Base.OnCardDetectedDelg(OnCardDetectedDelgWorking));
                }

                return _im30PortMan;
            }
        }

        private void ShowMessageDelgWorking(string message)
        {
            _msg.ShowMessage(message);
        }

        //private void OnTransactionFinishedDelgWorking(IIM30TransResult transResult)
        //{
        //    if (transResult is null)
        //        _msg.ShowMessage($@"----- Invalid Transaction Result -----");

        //    else /* (transResult != null) */
        //        _msg.ShowMessage($@"----- ----- Final Void Transaction Result Received ----- -----");
        //}

        //private void OnCardDetectedDelgWorking(IM30DataModel cardInfo)
        //{
        //    _msg.ShowMessage($@"===== ===== Card Info found ===== ===== ===== ===== ====={"\r\n"}");
        //}

        public void SyncUpdateHistoryInvoice(string invNo, decimal chargeAmount, string posTransNo, string cardToken)
        {
            TxtInvNo.Text = invNo;
            TxtVoidAmount.Text = $@"{chargeAmount:#,###.00}";
            TxtPosTransNo.Text = posTransNo;
            TxtCardToken.Text = (cardToken ?? "").Trim();
        }

        private bool _controlLoaded = false;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (_controlLoaded)
                return;
            _controlLoaded = true;
        }

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
                    IIM30TransResult trnResult = IM30PortMan.RunSoloCommand(
                                                    new VoidTransactionAxComm(invoiceNoX, voidAmountX, cardToken),
                                                    out bool isPendingOutstandingTransaction,
                                                    out Exception error);
                    if (trnResult?.IsSuccess == true)
                    {
                        _msg.ShowMessage("-Card Reader Void Transaction has done Successful (GB01)~");
                        //CreditDebitVoidTransactionResp

                        CreditDebitVoidTransactionResp cardResp = new CreditDebitVoidTransactionResp(trnResult.ResultData);
                        _msg.ShowMessage(JsonConvert.SerializeObject(cardResp, Formatting.Indented));
                    }
                    else if (trnResult?.IsSuccess == false)
                        _msg.ShowMessage("-Fail Card Reader Void Transaction (GB01)~");

                    else if (error != null)
                        _msg.ShowMessage(error.ToString());

                    else if (isPendingOutstandingTransaction)
                        _msg.ShowMessage("-Found outstanding/previous Card Reader Transaction (GB01)~Void Transaction#");

                    else
                        _msg.ShowMessage("-Fail to execute Card Reader (Single) command with unknown error (GB01)~Void Transaction#");
                }
                catch (Exception ex)
                {
                    _msg.ShowMessage(ex.ToString());
                }
            }
        }
    }
}
