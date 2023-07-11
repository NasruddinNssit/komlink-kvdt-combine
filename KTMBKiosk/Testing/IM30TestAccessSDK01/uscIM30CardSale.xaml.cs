using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.CreditDebit;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransParams;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult;
using NssIT.Kiosk.AppDecorator.Log;
using NssIT.Kiosk.Device.PAX.IM30.AccessSDK;
using NssIT.Kiosk.Device.PAX.IM30.AccessSDK.Base.AxCommandSet;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Base;
using NssIT.Kiosk.Tools.ThreadMonitor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for uscIM30CardSale.xaml
    /// </summary>
    public partial class uscIM30CardSale : UserControl
    {
        private const string _logChannel = "TestUI";

        private LibShowMessageWindow.MessageWindow _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;
        private ShowMessageLogDelg _showMessageDelgHandler = null;
        private string _comPort = null;

        private IM30PortManagerAx _im30PortMan = null;
        private string _hisCreditDebitInvNo = null;
        private string _hisCreditDebitPosTransNo = null;
        private decimal _hisCreditDebitChargeAmount = 0;
        private string _hisCreditDebitCardToken = null;

        private OnTransactionFinishedDelg _onTransactionFinishedDelgHandler = null;
        private NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base.OnCardDetectedDelg _onCardDetectedDelgHandler = null;
        public uscIM30CardSale()
        {
            InitializeComponent();
            _showMessageDelgHandler = new ShowMessageLogDelg(ShowMessageDelgWorking);

            _onTransactionFinishedDelgHandler = new OnTransactionFinishedDelg(OnTransactionFinishedDelgWorking);
            _onCardDetectedDelgHandler = new NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base.OnCardDetectedDelg(OnCardDetectedDelgWorking);
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
                    IM30PortMan.SetOnClientCardDetectedHandler(_onCardDetectedDelgHandler);
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
                _msg.ShowMessage($@"----- ----- Final Sale Transaction Result Received ----- -----");

                string processId = null;

                this.Dispatcher.Invoke(new Action(() => 
                {
                    processId = TxtDocNo.Text.Trim();
                }));

                RunThreadMan tMan = new RunThreadMan(new Action(() =>
                {
                    try
                    {
                        if (transResult.IsSuccess)
                        {
                            _msg.ShowMessage($@"Card Sale Success; Process Id : {processId}");

                            if (TransactionCodeDef.IsEqualTrans(transResult.ResultData.TransactionCode, TransactionCodeDef.ChargeAmount)
                                && ResponseCodeDef.IsEqualResponse(transResult.ResultData.ResponseCode, ResponseCodeDef.Approved)
                            )
                            {
                                _msg.ShowMessage($@"Credit/Debit Card Sale Success; Process Id : {processId}");

                                bool isSuccessCreditDebitHistoryCache = false;

                                var invFd = (from fd in transResult.ResultData.FieldElementCollection
                                                where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.InvoiceNo)
                                                select fd).FirstOrDefault();

                                var trnAmtFd = (from fd in transResult.ResultData.FieldElementCollection
                                                where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.TransAmount)
                                                select fd).FirstOrDefault();

                                var posTrnIdFd = (from fd in transResult.ResultData.FieldElementCollection
                                                    where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.PosTransId)
                                                    select fd).FirstOrDefault();

                                var crdTok = (from fd in transResult.ResultData.FieldElementCollection
                                                where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.CardToken)
                                                select fd).FirstOrDefault();

                                if ((invFd != null) && (trnAmtFd != null) && (posTrnIdFd != null) && (crdTok != null))
                                {
                                    if (long.TryParse(trnAmtFd.Data, out long trnAmountCent)
                                        && (string.IsNullOrWhiteSpace(invFd.Data) == false)
                                        && (string.IsNullOrWhiteSpace(posTrnIdFd.Data) == false)
                                    )
                                    {
                                        _hisCreditDebitInvNo = invFd.Data.Trim();
                                        _hisCreditDebitChargeAmount = ((decimal)trnAmountCent / 100M);
                                        _hisCreditDebitPosTransNo = posTrnIdFd.Data.Trim();
                                        _hisCreditDebitCardToken = crdTok.Data.Trim();
                                        isSuccessCreditDebitHistoryCache = true;

                                        _msg.ShowMessage($@"----- ----- -CreditDebitHistoryCache Updated~ ----- -----");
                                    }
                                }

                                if (isSuccessCreditDebitHistoryCache == false)
                                    _msg.ShowMessage($@"----- ----- -CreditDebitHistoryCache Fail Updated~ ----- -----");
                            }
                            else if (TransactionCodeDef.IsEqualTrans(transResult.ResultData.TransactionCode, TransactionCodeDef.ChargeAmount))
                            {
                                _msg.ShowMessage($@"Credit/Debit Card Sale NOT Success; Response Code: {transResult.ResultData.ResponseCode}; Process Id : {processId}");
                            }

                            if (CardEntityDataTools.CheckCardType(transResult.ResultData, out bool isSuccessData) == CardTypeEn.CreditCard)
                            {
                                if (isSuccessData)
                                {
                                    CreditDebitChargeCardResp cardResp = new CreditDebitChargeCardResp(transResult.ResultData);
                                    _msg.ShowMessage(JsonConvert.SerializeObject(cardResp, Formatting.Indented));
                                }
                            }
                        }
                        else if (transResult.IsTimeout)
                            _msg.ShowMessage($@"Transaction timeout.");

                        else if (transResult.IsManualStopped)
                            _msg.ShowMessage($@"Transaction has been stopped.");

                        else
                        {
                            if (transResult.Error != null)
                                _msg.ShowMessage($@"Error when Credit Card Sale; Process Id : {processId}{"\r\n"}" + transResult.Error.ToString());

                            else
                                _msg.ShowMessage($@"Error when Credit Card Sale; Unknown error; Process Id : {processId}\r\n");
                        }
                    }
                    catch (Exception ex)
                    {
                        //CYA-DEBUG .. Log Error Here
                    }

                    

                }), processId, lifeTimeSec: 10, _logChannel
                , threadPriority: System.Threading.ThreadPriority.AboveNormal);
            }
        }

        private void OnCardDetectedDelgWorking(IM30DataModel cardInfo)
        {
            _msg.ShowMessage($@"===== ===== Card Info found ===== ===== ===== ===== ====={"\r\n"}");
        }

        public bool QueryHisCreditDebitInvoice(out string invNo, out decimal chargeAmount, out string posTransNo, out string cardToken)
        {
            invNo = null;
            chargeAmount = -1;
            posTransNo = null;
            cardToken = null;

            if ((string.IsNullOrWhiteSpace(_hisCreditDebitInvNo) == false) && (_hisCreditDebitChargeAmount > 0))
            {
                invNo = _hisCreditDebitInvNo;
                chargeAmount = _hisCreditDebitChargeAmount;
                posTransNo = _hisCreditDebitPosTransNo;
                cardToken = _hisCreditDebitCardToken;
                return true;
            }

            return false;
        }

        private bool _controlLoaded = false;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (_controlLoaded)
                return;

            _controlLoaded = true;
            TxtDocNo.Text = "DOC_" + DateTime.Now.ToString("yyMMdd-HHmmss");

            CboGateDirection.Items.Add("Counter");
            CboGateDirection.Items.Add("Entry");
            CboGateDirection.Items.Add("Exit");
            CboGateDirection.SelectedItem = "Entry";

            CboKomLinkFirstSPNo.Items.Add("1");
            CboKomLinkFirstSPNo.Items.Add("2");
            CboKomLinkFirstSPNo.Items.Add("3");
            CboKomLinkFirstSPNo.Items.Add("4");
            CboKomLinkFirstSPNo.Items.Add("5");
            CboKomLinkFirstSPNo.Items.Add("6");
            CboKomLinkFirstSPNo.Items.Add("7");
            CboKomLinkFirstSPNo.Items.Add("8");
            CboKomLinkFirstSPNo.SelectedItem = "2";

            CboKomLinkSecondSPNo.Items.Add("1");
            CboKomLinkSecondSPNo.Items.Add("2");
            CboKomLinkSecondSPNo.Items.Add("3");
            CboKomLinkSecondSPNo.Items.Add("4");
            CboKomLinkSecondSPNo.Items.Add("5");
            CboKomLinkSecondSPNo.Items.Add("6");
            CboKomLinkSecondSPNo.Items.Add("7");
            CboKomLinkSecondSPNo.Items.Add("8");
            CboKomLinkSecondSPNo.SelectedItem = "3";

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            TxtTimer.Text = $@"{DateTime.Now:HH:mm:ss}";
        }

        private void NewDocNo_Click(object sender, RoutedEventArgs e)
        {
            TxtDocNo.Text = "DOC_" + DateTime.Now.ToString("yyMMdd-HHmmss");
        }

        private void ResetDebugCounter_Click(object sender, RoutedEventArgs e)
        {
            IM30RequestResponseDataWorks.DebugReset();
        }

        private void CreateNewSale_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            int maxCardDetectedWaitingTimeSec = 5;
            int maxSaleDecisionWaitingTimeSec = 5;

            GateDirectionEn gateDirection = GateDirectionEn.Counter;
            int komLinkFirstSPNo = 0;
            int komLinkSecondSPNo = 0;

            try
            {
                if (CboGateDirection.SelectedItem is string gateDirectionStr)
                {
                    gateDirection = (GateDirectionEn)Enum.Parse(typeof(GateDirectionEn), gateDirectionStr);
                }
                else
                    throw new Exception("Please select valid Gate Direction");

                if (CboKomLinkFirstSPNo.SelectedItem is string spNoStr1)
                {
                    komLinkFirstSPNo = int.Parse(spNoStr1);
                }
                else
                    throw new Exception("Please select valid First KomLink Season Pass No");

                if (CboKomLinkSecondSPNo.SelectedItem is string spNoStr2)
                {
                    komLinkSecondSPNo = int.Parse(spNoStr2);
                }
                else
                    throw new Exception("Please select valid Second KomLink Season Pass No");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
                return;
            }

            if (string.IsNullOrWhiteSpace(_comPort))
            {
                _msg.ShowMessage("Please select and apply a COM port.");
                return;
            }
            else if (string.IsNullOrWhiteSpace(TxtDocNo.Text))
            {
                _msg.ShowMessage("Please enter document no.");
                return;
            }
            else if (int.TryParse(TxtMaxCardDetectedWaitingTimeSec.Text, out maxCardDetectedWaitingTimeSec) == false)
            {
                _msg.ShowMessage("Invalid Max. Card Detected Waiting Time");
                return;
            }
            else if (maxCardDetectedWaitingTimeSec < 1)
            {
                _msg.ShowMessage("Max. Card Detected Waiting Time must more than 5");
                return;
            }
            else if (int.TryParse(TxtmaxSaleDecisionWaitingTimeSec.Text, out maxSaleDecisionWaitingTimeSec) == false)
            {
                _msg.ShowMessage("Invalid Max. Sale Decision Waiting Time");
                return;
            }
            else if (maxSaleDecisionWaitingTimeSec < 1)
            {
                _msg.ShowMessage("Max. Sale Decision Waiting Time must more than 5");
                return;
            }

            Guid processId = Guid.NewGuid();

            CreateNewSaleTest01();

            return;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            ///
            void CreateNewSaleTest01()
            {
                try
                {
                    if (decimal.TryParse(TxtPrice.Text, out decimal priceRMAmount))
                    {
                        IM30PortMan.SetOnClientTransactionFinishedHandler(_onTransactionFinishedDelgHandler);
                        IM30PortMan.SetOnClientCardDetectedHandler(_onCardDetectedDelgHandler);

                        bool isStartSuccessful = IM30PortMan.StartCardSaleTrans( 
                                                    new StartCardTransAxComm(gateDirection, komLinkFirstSPNo, komLinkSecondSPNo
                                                        , maxCardDetectedWaitingTimeSec, maxSaleDecisionWaitingTimeSec)
                                                    , out bool isPendingOutstandingTrans
                                                    , out Exception error);

                        if (isStartSuccessful == true)
                        {
                            _msg.ShowMessage($@"Card Sale Started Successful.{"\r\n"}");
                        }
                        else
                        {
                            if (error != null)
                                _msg.ShowMessage(error.ToString());

                            else if (isPendingOutstandingTrans)
                                _msg.ShowMessage("-Outstanding Card Reader Transaction has not finished~");
                            else
                                _msg.ShowMessage("Fail to start card sale transaction with unknown error.");
                        }
                    }
                    else
                        throw new Exception("Unable to parse Price. Please enter valid numeric price amount in Ringgit.");
                }
                catch (Exception ex)
                {
                    _msg.ShowMessage(ex.ToString());
                }
            }
        }

        

        private void ChargeCreditCard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                e.Handled = true;

                if (string.IsNullOrWhiteSpace(TxtDocNo.Text.Trim()))
                {
                    throw new Exception("Please valid Document Number");
                }
                else if (decimal.TryParse(TxtPrice.Text, out decimal priceRMAmount))
                {
                    if (IM30PortMan.Send2ndCommandParameter(new CreditDebitChargeParam(TxtDocNo.Text.Trim(), priceRMAmount)
                            , out bool? isFinalResultCollectFromEvent
                            , out IIM30TransResult transResult
                            , out Exception error)
                    )
                    {
                        _msg.ShowMessage("-Charge Credit Card parameters has been sent successful~");
                    }
                    else if (error != null)
                    {
                        _msg.ShowMessage(error.ToString());
                    }
                    else
                    {
                        _msg.ShowMessage("-Unknown Error; Fail to Send 2nd Card Command Parameter~Credit/Debit Card");
                    }
                }
                else
                    throw new Exception("Unable to parse Price. Please enter valid numeric price amount in Ringgit.");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void TnGCheckin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                e.Handled = true;
               if (string.IsNullOrWhiteSpace(TxtDocNo.Text.Trim()))
                {
                    throw new Exception("Please valid Document Number");
                }
                else if (decimal.TryParse(TxtPenalty.Text, out _) == false)
                {
                    throw new Exception("Please enter valid penalty amount");
                }
                else if (decimal.TryParse(TxtPenalty.Text, out decimal penaltyRMAmountX) && (penaltyRMAmountX < 0))
                {
                    throw new Exception("Penalty amount should more then 0");
                }
                else if (decimal.TryParse(TxtPenalty.Text, out decimal penaltyRMAmount))
                {
                    DateTime transDateTime = DateTime.Now;

                    if (IM30PortMan.Send2ndCommandParameter(new TnGEntryCheckinParam(TxtDocNo.Text.Trim(), penaltyRMAmount, transDateTime)
                            , out bool? isFinalResultCollectFromEvent
                            , out IIM30TransResult transResult
                            , out Exception error)
                    )
                    {
                        string msg = "\r\n";

                        msg += "-Check-in TnG parameters has been sent successful~\r\n";

                        if (transResult != null)
                            msg += "----- ----- -Final TnG Check-in result received successfully~ ----- -----\r\n";
                        else
                            msg += "----- ----- -Final TnG Check-in has NOT received~ ----- -----\r\n";

                        _msg.ShowMessage(msg);
                    }
                    else if (error != null)
                    {
                        _msg.ShowMessage(error.ToString());
                    }
                    else
                    {
                        _msg.ShowMessage("-Unknown Error; Fail to Send 2nd Card Command Parameter~Credit/Debit Card");
                    }

                }
                else
                    throw new Exception("Unable to read Penalty Amount. Please enter valid numeric Penalty Amount in Ringgit.");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void TnGCheckout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                e.Handled = true;
                if (string.IsNullOrWhiteSpace(TxtDocNo.Text.Trim()))
                {
                    throw new Exception("Please valid Document Number");
                }
                else if (decimal.TryParse(TxtPenalty.Text, out _) == false)
                {
                    throw new Exception("Please enter valid penalty amount");
                }
                else if (decimal.TryParse(TxtPrice.Text, out _) == false)
                {
                    throw new Exception("Please enter valid fare amount");
                }
                else if (decimal.TryParse(TxtPenalty.Text, out decimal penaltyRMAmountX) && (penaltyRMAmountX < 0))
                {
                    throw new Exception("Penalty amount should more then 0");
                }
                else if (decimal.TryParse(TxtPrice.Text, out decimal priceRMAmountX) && (priceRMAmountX < 0))
                {
                    throw new Exception("Fare amount should more then 0");
                }
                else if (decimal.TryParse(TxtPenalty.Text, out decimal penaltyRMAmount) && decimal.TryParse(TxtPrice.Text, out decimal priceRMAmount))
                {
                    DateTime transDateTime = DateTime.Now;

                    if (IM30PortMan.Send2ndCommandParameter(new TnGExitCheckoutParam(TxtDocNo.Text.Trim(), priceRMAmount, penaltyRMAmount, transDateTime)
                            , out bool? isFinalResultCollectFromEvent
                            , out IIM30TransResult transResult
                            , out Exception error)
                    )
                    {
                        string msg = "\r\n";

                        msg += "-Checkout TnG parameters has been sent successful~\r\n";

                        if (transResult != null)
                            msg += "----- ----- -Final TnG Checkout result received successfully~ ----- -----\r\n";
                        else
                            msg += "----- ----- -Final TnG Checkout has NOT received~ ----- -----\r\n";

                        _msg.ShowMessage(msg);
                    }
                    else if (error != null)
                    {
                        _msg.ShowMessage(error.ToString());
                    }
                    else
                    {
                        _msg.ShowMessage("-Unknown Error; Fail to Send 2nd Card Command Parameter~TnG Checkout");
                    }

                }
                else
                    throw new Exception("Unable to read Penalty Amount or Fare Amount. Please enter valid numeric Penalty Amount and Fare Amount in Ringgit.");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void StopSale_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               bool isSuccess =  IM30PortMan.Send2ndCommandParameter(new StopCardTransParam()
                                , out bool? isFinalResultCollectFromEvent
                                , out IIM30TransResult transResult
                                , out Exception error);

                if (isSuccess)
                {
                    if (transResult?.IsSuccess == true)
                        _msg.ShowMessage("----- ----- 2nd Stop Command has been sent ----- -----");

                    else if (transResult?.IsSuccess == false)
                        _msg.ShowMessage("----- ----- Fail to send 2nd Stop Command ----- -----");

                    else 
                        _msg.ShowMessage("----- ----- Unknown error when sending 2nd Stop Command ----- -----");
                }
                else
                {
                    if (error != null)
                        _msg.ShowMessage(error.ToString());
                    else
                        _msg.ShowMessage("Fail to send 2nd Stop Command with unknown error.");
                }
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void DisposeSale_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    DisposeSale();
            //}
            //catch (Exception ex)
            //{
            //    _msg.ShowMessage(ex.ToString());
            //}
        }

        private void DisposeSale()
        {
            //if (_im30SaleTrans != null)
            //{
            //    if (_im30SaleTrans.IsCurrentWorkingEnded == false)
            //    {
            //        _msg.ShowMessage("Dispose existing Sale Transaction");
            //        _im30SaleTrans.Dispose();
            //        _im30SaleTrans = null;
            //    }
            //    else
            //    {
            //        _msg.ShowMessage("Existing Sale Transaction has already disposed");
            //    }
            //}
            //else
            //{
            //    _msg.ShowMessage("Existing Sale Transaction has not found");
            //}
        }

        
    }
}
