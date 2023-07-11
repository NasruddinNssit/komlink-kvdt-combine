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
using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.Settlement;
using NssIT.Kiosk.AppDecorator.Log;
using NssIT.Kiosk.Device.PAX.IM30.AccessSDK;

namespace IM30TestAccessSDKInterface01
{
    /// <summary>
    /// Interaction logic for uscIM30CardSettlement.xaml
    /// </summary>
    public partial class uscIM30CardSettlement : UserControl
    {
        private const string _logChannel = "TestUI";

        private LibShowMessageWindow.MessageWindow _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;
        private ShowMessageLogDelg _showMessageDelgHandler = null;

        private string _comPort = null;
        private IM30AccessSDK _im30AccessSDK = null;

        public uscIM30CardSettlement()
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
                _im30AccessSDK.OnTransactionResponse += _im30AccessSDK_OnTransactionResponse;
            }
        }

        private void ShowMessageDelgWorking(string message)
        {
            _msg.ShowMessage(message);
        }

        private void _im30AccessSDK_OnTransactionResponse(object sender, CardTransResponseEventArgs e)
        {
            if (e.TransType == TransactionTypeEn.Settlement)
            {
                SettlementReceivedResult(e);
            }
            else if (e.EventCode == TransEventCodeEn.Timeout)
            {
                if ((e.ResponseInfo is ErrorResponse errResp) && (errResp.DataError != null))
                    _msg.ShowMessage(errResp.DataError.ToString());
                else
                    _msg.ShowMessage("#####----- Timeout -----#####");
            }
            else
            {
                _msg.ShowMessage($@"!!!!! !!!!! Unhandled Transaction Type ({e.TransType})) - !!!!! !!!!! - Settlement");
            }

            return;
            /////==============================================================================================
            void SettlementReceivedResult(CardTransResponseEventArgs eArg)
            {
                if (eArg.ResponseInfo is SettlementResp sttCardInfo)
                {
                    _msg.ShowMessage("TT06--" + JsonConvert.SerializeObject(eArg.ResponseInfo, Formatting.Indented));

                    if (sttCardInfo.SettlementResult == SettlementStatusEn.Success)
                    {
                        _msg.ShowMessage("===== ===== Settlement - Success - ===== =====");
                    }
                    else if (sttCardInfo.SettlementResult == SettlementStatusEn.PartiallyDone)
                    {
                        _msg.ShowMessage("===== ===== Settlement Partially Done - ===== =====");
                    }
                    else if (sttCardInfo.SettlementResult == SettlementStatusEn.Fail)
                    {
                        _msg.ShowMessage("===== ===== Fail Settlement - ===== =====");
                    }
                    else /* if (sttCardInfo.SettlementResult == SettlementStatusEn.Empty) */
                    {
                        _msg.ShowMessage("===== ===== Empty Settlement - ===== =====");
                    }
                }
                else if (eArg.ResponseInfo is ErrorResponse errResp)
                {
                    _msg.ShowMessage("===== ===== Settlement - Fail - ===== =====");
                    _msg.ShowMessage(errResp.DataError?.ToString());
                }
            }
        }
                
        private void IM30CardSettlement_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (string.IsNullOrWhiteSpace(_comPort))
            {
                _msg.ShowMessage("Please select and apply a COM port.");
                return;
            }

            Guid processId = Guid.NewGuid();

            CardSettlementTest01();

            return;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            ///
            void CardSettlementTest01()
            {
                try
                {
                    bool isSettSent = _im30AccessSDK.SettleCreditDebitSales(out Exception error);

                    if (isSettSent == true)
                        _msg.ShowMessage("-Card Reader Settlement started Successful (GB01)~");

                    else if (error != null)
                        _msg.ShowMessage("-Fail to Card Settlement~; " + error.ToString());

                    else  
                        _msg.ShowMessage("-Fail to execute Card Settlement command (GB01)~");

                }
                catch (Exception ex)
                {
                    _msg.ShowMessage(ex.ToString());
                }
            }
        }
    }
}
