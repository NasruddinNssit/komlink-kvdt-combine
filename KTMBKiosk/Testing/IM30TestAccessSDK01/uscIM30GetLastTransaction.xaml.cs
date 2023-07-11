using System;
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
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.CreditDebit;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult;
using NssIT.Kiosk.AppDecorator.Log;
using NssIT.Kiosk.Device.PAX.IM30.AccessSDK;
using NssIT.Kiosk.Device.PAX.IM30.AccessSDK.Base.AxCommandSet;

namespace IM30TestAccessSDK01
{
    /// <summary>
    /// Interaction logic for uscIM30GetLastTransaction.xaml
    /// </summary>
    public partial class uscIM30GetLastTransaction : UserControl
    {
        private const string _logChannel = "TestUI";

        private LibShowMessageWindow.MessageWindow _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;
        private ShowMessageLogDelg _showMessageDelgHandler = null;
        private string _comPort = null;
        private IM30PortManagerAx _im30PortMan = null;

        public uscIM30GetLastTransaction()
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
        //        _msg.ShowMessage($@"----- ----- Final Get Last Transaction Result Received ----- -----");
        //}

        //private void OnCardDetectedDelgWorking(IM30DataModel cardInfo)
        //{
        //    _msg.ShowMessage($@"===== ===== Card Info found ===== ===== ===== ===== ====={"\r\n"}");
        //}

        private void IM30GetLastTrans_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (string.IsNullOrWhiteSpace(_comPort))
            {
                _msg.ShowMessage("Please select and apply a COM port.");
                return;
            }

            GetLastTransTest01();

            return;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            ///
            void GetLastTransTest01()
            {
                try
                {
                    IIM30TransResult trnResult = IM30PortMan.RunSoloCommand(
                                                    new GetLastTransAxComm(),
                                                    out bool isPendingOutstandingTransaction,
                                                    out Exception error);

                    if (trnResult?.IsSuccess == true)
                    {
                        _msg.ShowMessage("-Card Reader Get Last Transaction has done Successful (GB01)~");

                        if (CardEntityDataTools.CheckCardType(trnResult.ResultData, out bool isSuccessData) == CardTypeEn.CreditCard)
                        {
                            if (isSuccessData)
                            {
                                CreditDebitChargeCardResp cardResp = new CreditDebitChargeCardResp(trnResult.ResultData);
                                _msg.ShowMessage(JsonConvert.SerializeObject(cardResp, Formatting.Indented));
                            }
                        }
                    }

                    else if (trnResult?.IsSuccess == false)
                        _msg.ShowMessage("-Fail Card Reader Get Last Transaction (GB01)~");

                    else if (error != null)
                        _msg.ShowMessage(error.ToString());

                    else if (isPendingOutstandingTransaction)
                        _msg.ShowMessage("-Found outstanding/previous Card Reader Transaction (GB01)~Get Last Transaction#");

                    else
                        _msg.ShowMessage("-Fail to execute Card Reader (Single) command with unknown error (GB01)~Get Last Transaction#");
                }
                catch (Exception ex)
                {
                    _msg.ShowMessage(ex.ToString());
                }
            }
        }

        private void InitPortAccess_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IM30PortManagerAx t1 = IM30PortMan;
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }
    }
}
