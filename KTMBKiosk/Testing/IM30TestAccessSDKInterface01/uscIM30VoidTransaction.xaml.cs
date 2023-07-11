using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.CreditDebit;
using NssIT.Kiosk.AppDecorator.Log;
using NssIT.Kiosk.Device.PAX.IM30.AccessSDK;
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

namespace IM30TestAccessSDKInterface01
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
        private IM30AccessSDK _im30AccessSDK = null;

        public uscIM30VoidTransaction()
        {
            InitializeComponent();
            //_showMessageDelgHandler = new ShowMessageDelg(ShowMessageDelgWorking);
        }

        public void SetCOMPort(string comPort)
        {
            if ((string.IsNullOrWhiteSpace(comPort) == false) && (_im30AccessSDK is null))
            {
                _comPort = comPort;

                _im30AccessSDK = new IM30AccessSDK(_comPort, _showMessageDelgHandler);
            }
        }

        private void ShowMessageDelgWorking(string message)
        {
            _msg.ShowMessage(message);
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
            void VoidTransactionTest01(string invoiceNoX, decimal voidAmountX, string cardTokenX)
            {
                try
                {
                    bool isVoidDone = _im30AccessSDK.VoidCreditCardTransaction(invoiceNoX, voidAmountX, cardTokenX,
                        out CreditDebitVoidTransactionResp responseResult, out Exception error);

                    if (isVoidDone == true)
                    {
                        _msg.ShowMessage("-Void Sale Transaction Successful (GB01)~");
                    }
                    else if (error != null)
                        _msg.ShowMessage("-Fail to Void Sale Transaction~; " + error.ToString());

                    else
                        _msg.ShowMessage("-Fail to execute Void Sale Transaction command (GB01)~");


                    if (responseResult is CreditDebitVoidTransactionResp sttCardInfo)
                    {
                        _msg.ShowMessage(JsonConvert.SerializeObject(responseResult, Formatting.Indented));

                        if (sttCardInfo.ResponseResult == ResponseCodeEn.Success)
                        {
                            _msg.ShowMessage("===== ===== Void Sale Transaction successfull - ===== =====");
                        }
                        else if (error != null)
                        {
                            _msg.ShowMessage("-Fail to Void Sale Transaction~; " + error.ToString());
                        }
                        else if (sttCardInfo.DataError != null)
                        {
                            _msg.ShowMessage(sttCardInfo.DataError.ToString());
                        }
                        else
                        {
                            _msg.ShowMessage("-Fail to Void Sale Transaction~");
                        }
                    }
                    else if (error != null)
                    {
                        _msg.ShowMessage("-Fail to Void Sale Transaction~; " + error.ToString());
                    }
                    else
                    {
                        _msg.ShowMessage("-Fail to Void Sale Transaction; No response data found~");
                    }
                }
                catch (Exception ex)
                {
                    _msg.ShowMessage(ex.ToString());
                }
            }
        }
    }
}
