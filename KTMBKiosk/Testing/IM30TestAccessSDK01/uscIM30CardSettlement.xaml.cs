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
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult;
using NssIT.Kiosk.AppDecorator.Log;
using NssIT.Kiosk.Device.PAX.IM30.AccessSDK;
using NssIT.Kiosk.Device.PAX.IM30.AccessSDK.Base.AxCommandSet;

namespace IM30TestAccessSDK01
{
    /// <summary>
    /// Interaction logic for uscCardSettlement.xaml
    /// </summary>
    public partial class uscCardSettlement : UserControl
    {
        private const string _logChannel = "TestUI";

        private LibShowMessageWindow.MessageWindow _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;
        private ShowMessageLogDelg _showMessageDelgHandler = null;

        private string _comPort = null;
        private IM30PortManagerAx _im30PortMan = null;

        private OnTransactionFinishedDelg _onTransactionFinishedDelgHandler = null;

        public uscCardSettlement()
        {
            InitializeComponent();

            _showMessageDelgHandler = new ShowMessageLogDelg(ShowMessageDelgWorking);

            _onTransactionFinishedDelgHandler = new OnTransactionFinishedDelg(OnTransactionFinishedDelgWorking);
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
                    IM30PortMan.SetOnClientTransactionFinishedHandler(_onTransactionFinishedDelgHandler);
                    //IM30PortMan.SetOnClientCardDetectedHandler(new NssIT.ACG.AppDecorator.DomainLibs.IM30.Base.OnCardDetectedDelg(OnCardDetectedDelgWorking));
                }

                return _im30PortMan;
            }
        }

        private void ShowMessageDelgWorking(string message)
        {
            _msg.ShowMessage(message);
        }

        private void OnTransactionFinishedDelgWorking(IIM30TransResult transResult)
        {
            if (transResult is null)
                _msg.ShowMessage($@"----- Invalid Transaction Result -----");

            else /* (transResult != null) */
            {
                
                if (CardEntityDataTools.CheckIsSettlementData(transResult.ResultData, out bool isSuccessData) == true)
                {
                    SettlementResp settleResp = null;
                    try
                    {
                        settleResp = new SettlementResp(transResult.ResultData);
                    }
                    catch (Exception ex)
                    {
                        _msg.ShowMessage(ex.ToString());
                    }
                    _msg.ShowMessage($@"----- ----- Final Settlement Settlement Result Received ----- ----- Result: {settleResp?.SettlementResult}");

                    if (settleResp != null)
                        _msg.ShowMessage(JsonConvert.SerializeObject(settleResp, Formatting.Indented));
                }
                else
                {
                    _msg.ShowMessage("-----!!!!! Invalid Settlement Response Data !!!!!------");
                }
            }
        }

        //private void OnCardDetectedDelgWorking(IM30DataModel cardInfo)
        //{
        //    _msg.ShowMessage($@"===== ===== Card Info found ===== ===== ===== ===== ====={"\r\n"}");
        //}

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
                    IM30PortMan.SetOnClientTransactionFinishedHandler(_onTransactionFinishedDelgHandler);

                    IIM30TransResult trnResult = IM30PortMan.RunSoloCommand(
                                                    new CardSettlementAxComm(),
                                                    out bool isPendingOutstandingTransaction,
                                                    out Exception error);

                    if (trnResult?.IsSuccess == true)
                        _msg.ShowMessage("-Card Reader Settlement started Successful (GB01)~");

                    else if (trnResult?.IsSuccess == false)
                        _msg.ShowMessage("-Fail to execute Card Settlement command (GB01)~");

                    else if (error != null)
                        _msg.ShowMessage("-Fail to Card Settlement~; " + error.ToString());

                    else if (isPendingOutstandingTransaction)
                        _msg.ShowMessage("-Found outstanding/previous Card Reader Transaction (GB01)~Card Settlement#");

                    else
                        _msg.ShowMessage("-Fail to start Card Reader (Single) command with unknown error (GB01)~Card Settlement#");
                }
                catch (Exception ex)
                {
                    _msg.ShowMessage(ex.ToString());
                }
            }
        }
    }
}
