using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.CreditDebit;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.TnG;
using NssIT.Kiosk.AppDecorator.Log;
using NssIT.Kiosk.Device.PAX.IM30.AccessSDK;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Base;
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

namespace IM30TestAccessSDKInterface01
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

        private string _hisCreditDebitInvNo = null;
        private string _hisCreditDebitPosTransNo = null;
        private decimal _hisCreditDebitChargeAmount = 0;
        private string _hisCreditDebitCardToken = null;

        //private OnTransactionFinishedDelg _onTransactionFinishedDelgHandler = null;
        //private NssIT.ACG.AppDecorator.DomainLibs.IM30.Base.OnCardDetectedDelg _onCardDetectedDelgHandler = null;

        private IM30AccessSDK _im30AccessSDK = null;

        public uscIM30CardSale()
        {
            InitializeComponent();

            //_showMessageDelgHandler = new ShowMessageDelg(ShowMessageDelgWorking);
            
            //_onTransactionFinishedDelgHandler = new OnTransactionFinishedDelg(OnTransactionFinishedDelgWorking);
            //_onCardDetectedDelgHandler = new NssIT.ACG.AppDecorator.DomainLibs.IM30.Base.OnCardDetectedDelg(OnCardDetectedDelgWorking);
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

        private void _im30AccessSDK_OnTransactionResponse(object sender, CardTransResponseEventArgs e)
        {
            if  (e.EventCode == TransEventCodeEn.CardInfoResponse)
            {
                CardInfoReceived(e);
            }
            else if (e.TransType == TransactionTypeEn.CreditCard_2ndComm)
            {
                CrediDebitChargingReceivedResult(e);
            }
            //else if (e.TransType == TransactionTypeEn.Settlement)
            //{
            //    SettlementReceivedResult(e);
            //}
            else if (e.EventCode == TransEventCodeEn.Timeout) 
            {
                if ((e.ResponseInfo is ErrorResponse errResp) && (errResp.DataError != null))
                    _msg.ShowMessage(errResp.DataError.ToString());
                else
                    _msg.ShowMessage("#####----- Timeout -----#####");
            }
            else 
            {
                _msg.ShowMessage($@"!!!!! !!!!! Unhandled Transaction Type ({e.TransType})) - !!!!! !!!!! -- Card Sales");
            }

            return;
            /////==============================================================================================
            void CardInfoReceived(CardTransResponseEventArgs eArg)
            {
                if (eArg.ResponseInfo is CreditDebitCardInfoResp cdCardInfo)
                {
                    _msg.ShowMessage("TT04--" + JsonConvert.SerializeObject(eArg.ResponseInfo, Formatting.Indented));

                    if ((cdCardInfo.IsDataFound) && (cdCardInfo.DataError is null))
                    {
                        _msg.ShowMessage("===== ===== Credit Card Info Found - Success - ===== =====");
                    }
                    else
                    {
                        _msg.ShowMessage("===== ===== Credit Card Info Found - Fail - ===== =====");
                    }
                }
                else if (eArg.ResponseInfo is TnGCardInfoResp tngCardInfo)
                {
                    _msg.ShowMessage("TT03--" + JsonConvert.SerializeObject(eArg.ResponseInfo, Formatting.Indented));

                    if ((tngCardInfo.IsDataFound) && (tngCardInfo.DataError is null))
                    {
                        _msg.ShowMessage("===== ===== TnG Card Info Found - Success - ===== =====");
                    }
                    else
                    {
                        _msg.ShowMessage("===== ===== TnG Card Info Found - Fail - ===== =====");
                    }
                }
                else if (eArg.ResponseInfo is ErrorResponse errResp)
                {
                    _msg.ShowMessage("===== ===== Card Info - Fail - ===== =====");
                    _msg.ShowMessage(errResp.DataError?.ToString());
                }
                else
                {
                    //CYA-DEBUG .. log error here
                }
            }

            void CrediDebitChargingReceivedResult(CardTransResponseEventArgs eArg)
            {
                if (eArg.ResponseInfo is CreditDebitChargeCardResp cdCardInfo)
                {
                    _msg.ShowMessage("TT02--" + JsonConvert.SerializeObject(eArg.ResponseInfo, Formatting.Indented));

                    if (cdCardInfo.ResponseResult == ResponseCodeEn.Success)
                    {
                        _hisCreditDebitInvNo = cdCardInfo.InvoiceNo;
                        _hisCreditDebitChargeAmount = cdCardInfo.Amount;
                        _hisCreditDebitPosTransNo = cdCardInfo.POSTransactionID;
                        _hisCreditDebitCardToken = cdCardInfo.CardToken;
                        _msg.ShowMessage("===== ===== Credit Card Final Result Found - Success - ===== =====");
                    }
                    else
                    {
                        _msg.ShowMessage($@"===== ===== Credit Card Info Found - Fail - {cdCardInfo.ResponseText} -===== =====");
                    }
                }
                else if (eArg.ResponseInfo is ErrorResponse errResp)
                {
                    _msg.ShowMessage("===== ===== Card Info - Fail - ===== =====");
                    _msg.ShowMessage(errResp.DataError?.ToString());
                }
                else
                {
                    //CYA-DEBUG .. log error here
                }
            }

            //void SettlementReceivedResult(CardTransResponseEventArgs eArg)
            //{
            //    if (eArg.ResponseInfo is SettlementResp sttCardInfo)
            //    {
            //        _msg.ShowMessage("TT06--" + JsonConvert.SerializeObject(eArg.ResponseInfo, Formatting.Indented));

            //        if (sttCardInfo.SettlementResult == SettlementStatusEn.Success)
            //        {
            //            _msg.ShowMessage("===== ===== Settlement - Success - ===== =====");
            //        }
            //        else if (sttCardInfo.SettlementResult == SettlementStatusEn.PartiallyDone)
            //        {
            //            _msg.ShowMessage("===== ===== Settlement Partially Done - ===== =====");
            //        }
            //        else if (sttCardInfo.SettlementResult == SettlementStatusEn.Fail)
            //        {
            //            _msg.ShowMessage("===== ===== Fail Settlement - ===== =====");
            //        }
            //        else /* if (sttCardInfo.SettlementResult == SettlementStatusEn.Empty) */
            //        {
            //            _msg.ShowMessage("===== ===== Empty Settlement - ===== =====");
            //        }
            //    }
            //    else if (eArg.ResponseInfo is ErrorResponse errResp)
            //    {
            //        _msg.ShowMessage("===== ===== Settlement - Fail - ===== =====");
            //        _msg.ShowMessage(errResp.DataError?.ToString());
            //    }
            //}
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
        //    {
        //        _msg.ShowMessage($@"----- ----- Final Sale Transaction Result Received ----- -----");

        //        string processId = null;

        //        this.Dispatcher.Invoke(new Action(() =>
        //        {
        //            processId = TxtDocNo.Text.Trim();
        //        }));

        //        RunThreadMan tMan = new RunThreadMan(new Action(() =>
        //        {
        //            try
        //            {
        //                if (transResult.IsSuccess)
        //                {
        //                    _msg.ShowMessage($@"Card Sale Success; Process Id : {processId}");

        //                    if (TransactionCodeDef.IsEqualTrans(transResult.ResultData.TransactionCode, TransactionCodeDef.ChargeAmount)
        //                        && ResponseCodeDef.IsEqualResponse(transResult.ResultData.ResponseCode, ResponseCodeDef.Approved)
        //                    )
        //                    {
        //                        _msg.ShowMessage($@"Credit/Debit Card Sale Success; Process Id : {processId}");

        //                        bool isSuccessCreditDebitHistoryCache = false;

        //                        var invFd = (from fd in transResult.ResultData.FieldElementCollection
        //                                     where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.InvoiceNo)
        //                                     select fd).FirstOrDefault();

        //                        var trnAmtFd = (from fd in transResult.ResultData.FieldElementCollection
        //                                        where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.TransAmount)
        //                                        select fd).FirstOrDefault();

        //                        var posTrnIdFd = (from fd in transResult.ResultData.FieldElementCollection
        //                                          where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.PosTransId)
        //                                          select fd).FirstOrDefault();

        //                        if ((invFd != null) && (trnAmtFd != null) && (posTrnIdFd != null))
        //                        {
        //                            if (long.TryParse(trnAmtFd.Data, out long trnAmountCent)
        //                                && (string.IsNullOrWhiteSpace(invFd.Data) == false)
        //                                && (string.IsNullOrWhiteSpace(posTrnIdFd.Data) == false)
        //                            )
        //                            {
        //                                _hisCreditDebitInvNo = invFd.Data.Trim();
        //                                _hisCreditDebitChargeAmount = ((decimal)trnAmountCent / 100M);
        //                                _hisCreditDebitPosTransNo = posTrnIdFd.Data.Trim();
        //                                isSuccessCreditDebitHistoryCache = true;

        //                                _msg.ShowMessage($@"----- ----- -CreditDebitHistoryCache Updated~ ----- -----");
        //                            }
        //                        }

        //                        if (isSuccessCreditDebitHistoryCache == false)
        //                            _msg.ShowMessage($@"----- ----- -CreditDebitHistoryCache Fail Updated~ ----- -----");
        //                    }
        //                    else if (TransactionCodeDef.IsEqualTrans(transResult.ResultData.TransactionCode, TransactionCodeDef.ChargeAmount))
        //                    {
        //                        _msg.ShowMessage($@"Credit/Debit Card Sale NOT Success; Response Code: {transResult.ResultData.ResponseCode}; Process Id : {processId}");
        //                    }

        //                    if (CardEntityDataTools.CheckCardType(transResult.ResultData, out bool isSuccessData) == CardTypeEn.CreditCard)
        //                    {
        //                        if (isSuccessData)
        //                        {
        //                            CreditDebitChargeCardResp cardResp = new CreditDebitChargeCardResp(transResult.ResultData);
        //                            _msg.ShowMessage("TT07--" + JsonConvert.SerializeObject(cardResp, Formatting.Indented));
        //                        }
        //                    }
        //                }
        //                else if (transResult.IsTimeout)
        //                    _msg.ShowMessage($@"Transaction timeout.");

        //                else if (transResult.IsManualStopped)
        //                    _msg.ShowMessage($@"Transaction has been stopped.");

        //                else
        //                {
        //                    if (transResult.Error != null)
        //                        _msg.ShowMessage($@"Error when Credit Card Sale; Process Id : {processId}{"\r\n"}" + transResult.Error.ToString());

        //                    else
        //                        _msg.ShowMessage($@"Error when Credit Card Sale; Unknown error; Process Id : {processId}\r\n");
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                //CYA-DEBUG .. Log Error Here
        //            }
        //        }), processId, lifeTimeSec: 10, _logChannel
        //        , threadPriority: System.Threading.ThreadPriority.AboveNormal);
        //    }
        //}

        //private void OnCardDetectedDelgWorking(IM30DataModel cardInfo)
        //{
        //    _msg.ShowMessage($@"===== ===== Card Info found ===== ===== ===== ===== ====={"\r\n"}");
        //}

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

            CreateNewSaleTest01();

            return;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void CreateNewSaleTest01()
            {
                try
                {
                    if (decimal.TryParse(TxtPrice.Text, out decimal priceRMAmount))
                    {
                        if (_im30AccessSDK.StartCardTransaction(komLinkFirstSPNo, komLinkSecondSPNo, 
                            maxCardDetectedWaitingTimeSec, maxSaleDecisionWaitingTimeSec,
                            out string deviceSerialNo, out Exception error) == true)
                        {
                            _msg.ShowMessage($@"Card Sale Started Successful. ----- Device Serial No.: {deviceSerialNo} {"\r\n"}");
                        }
                        else
                        {
                            if (error != null)
                                _msg.ShowMessage(error.ToString());
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
                    if (_im30AccessSDK.CreditDebitChargeCard(TxtDocNo.Text.Trim(), priceRMAmount, out Exception error))
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
                    string dummyPosTransId = Guid.NewGuid().ToString();

                    /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                    if (_im30AccessSDK.TnGACGCheckin(transDateTime, penaltyRMAmount
                            , out TnGACGCheckinResp responseResult
                            , out Exception error)
                    )
                    {
                        string msg = "\r\n";

                        msg += "-Check-in TnG parameters has been sent successful~\r\n";

                        if (responseResult != null)
                        {
                            if (responseResult.ResponseResult == ResponseCodeEn.Success)
                            {
                                _msg.ShowMessage($@"----- Response Result Type: {responseResult.GetType().Name} -----
" + JsonConvert.SerializeObject(responseResult, Formatting.Indented));

                                msg += "----- ----- -Final TnG Check-in result received successfully~ ----- -----\r\n";
                            }
                            else
                            {
                                msg += "----- ----- -Fail TnG Check-in with with related result~ ----- -----\r\n";
                            }
                        }
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
                        _msg.ShowMessage("-Unknown Error; Fail to Send 2nd Card Command Parameter~TnG Check-in Card");
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
                    string dummyPosTransId = Guid.NewGuid().ToString();

                    /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                    if (_im30AccessSDK.TnGACGCheckout(transDateTime, priceRMAmount, penaltyRMAmount
                            , out TnGACGCheckoutResp responseResult
                            , out Exception error)
                    )
                    {
                        string msg = "\r\n";

                        msg += "-Checkout TnG parameters has been sent successful~\r\n";

                        if (responseResult != null)
                        {
                            if (responseResult.ResponseResult == ResponseCodeEn.Success)
                            {
                                _msg.ShowMessage($@"----- Response Result Type: {responseResult.GetType().Name} -----
" + JsonConvert.SerializeObject(responseResult, Formatting.Indented));
                                msg += "----- ----- -Final TnG Checkout result received successfully~ ----- -----\r\n";
                            }
                            else
                            {
                                msg += "----- ----- -Fail TnG Checkout with with related result~ ----- -----\r\n";
                            }
                        }
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
                        _msg.ShowMessage("-Unknown Error; Fail to Send 2nd Card Command Parameter~TnG Checkout Card");
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

        private void StopSale_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _msg.ShowMessage("-Request to stop .....~\r\n");

                if (_im30AccessSDK.StopCardTransaction(out Exception error))
                {
                    _msg.ShowMessage("-Stop Card Sale Transaction (2nd Command) done successful~\r\n");
                }
                else if (error != null)
                {
                    _msg.ShowMessage(error.ToString());
                }
                else
                {
                    _msg.ShowMessage("-Unknown Error; Fail to Send 2nd Card Command Parameter~Stop Card Sale Trans.");
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
