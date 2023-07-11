using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransParams;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult;
using NssIT.Kiosk.Tools.ThreadMonitor;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Base;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
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

        private IM30CardSale _im30SaleTrans = null;

        public uscIM30CardSale()
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

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            /////CYA-DEBUG .. Below code is not working because UserControl_Unloaded is not called when close window.
            try
            {
                DisposeSale();
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
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

        public bool QueryHisCreditDebitInvoice(out string invNo, out decimal chargeAmount, out string posTransNo, out string cardToken)
        {
            invNo = null;
            chargeAmount = -1;
            posTransNo = null;
            cardToken = null;

            if ((string.IsNullOrWhiteSpace(_hisCreditDebitInvNo)==false) && (_hisCreditDebitChargeAmount > 0))
            {
                invNo = _hisCreditDebitInvNo;
                chargeAmount = _hisCreditDebitChargeAmount;
                posTransNo = _hisCreditDebitPosTransNo;
                cardToken = _hisCreditDebitCardToken;
                return true;
            }

            return false;
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
                    if (_im30SaleTrans?.IsCurrentWorkingEnded == false)
                    {
                        _msg.ShowMessage("Dispose existing Sale Transaction .. Please wait 10 seconds");
                        _im30SaleTrans.Dispose();

                        Thread.Sleep(10 * 1000);
                    }

                    if (decimal.TryParse(TxtPrice.Text, out decimal priceRMAmount))
                    {
                        _im30SaleTrans = new IM30CardSale(_comPort, new OnTransactionFinishedDelg(OnTransactionFinishedDelgWorking),
                            null,
                            TxtDocNo.Text.Trim(), gateDirection, komLinkFirstSPNo, komLinkSecondSPNo,
                            maxCardDetectedWaitingTimeSec, maxSaleDecisionWaitingTimeSec,
                            _showMessageDelgHandler);

                        _im30SaleTrans.StartTransaction(out Exception ex2);
                        if (ex2 != null)
                            throw new Exception("Error when run CreateNewSaleTest01; (A)", ex2);

                        //IM30DataModel im30Data = newIM30Trans.GetNewIM30Data();
                        //byte[] dataResult = IM30RequestResponseDataWorks.RenderData(im30Data);
                        //_msg.ShowMessage("\r\n" + JsonConvert.SerializeObject(im30Data, Formatting.Indented));
                    }
                    else
                        throw new Exception("Unable to parse Price. Please enter valid numeric price amount in Ringgit.");
                }
                catch (Exception ex)
                {
                    _msg.ShowMessage(ex.ToString());
                }
            }

            void OnTransactionFinishedDelgWorking(IIM30TransResult transResult)
            {
                if (_im30SaleTrans == null)
                    return;

                RunThreadMan tMan = new RunThreadMan(new Action(() =>
                {
                    try
                    {
                        if (_im30SaleTrans.FinalResult != null)
                        {
                            if (_im30SaleTrans.FinalResult.IsSuccess)
                            {
                                _msg.ShowMessage($@"Card Sale Success; Process Id : {processId}");

                                if (TransactionCodeDef.IsEqualTrans(_im30SaleTrans.FinalResult.ResultData.TransactionCode, TransactionCodeDef.ChargeAmount) 
                                    && ResponseCodeDef.IsEqualResponse(_im30SaleTrans.FinalResult.ResultData.ResponseCode, ResponseCodeDef.Approved)
                                )
                                {
                                    _msg.ShowMessage($@"Credit/Debit Card Sale Success; Process Id : {processId}");

                                    bool isSuccessCreditDebitHistoryCache = false;

                                    var invFd = (from fd in _im30SaleTrans.FinalResult.ResultData.FieldElementCollection
                                                where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.InvoiceNo)
                                                select fd).FirstOrDefault();

                                    var trnAmtFd = (from fd in _im30SaleTrans.FinalResult.ResultData.FieldElementCollection
                                                 where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.TransAmount)
                                                 select fd).FirstOrDefault();

                                    var posTrnIdFd = (from fd in _im30SaleTrans.FinalResult.ResultData.FieldElementCollection
                                                    where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.PosTransId)
                                                    select fd).FirstOrDefault();

                                    var crdTok = (from fd in transResult.ResultData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.CardToken)
                                                  select fd).FirstOrDefault();

                                    if ((invFd != null) && (trnAmtFd != null) && (posTrnIdFd != null))
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
                                else if (TransactionCodeDef.IsEqualTrans(_im30SaleTrans.FinalResult.ResultData.TransactionCode, TransactionCodeDef.ChargeAmount))
                                {
                                    _msg.ShowMessage($@"Credit/Debit Card Sale NOT Success; Response Code: {_im30SaleTrans.FinalResult.ResultData.ResponseCode}; Process Id : {processId}");
                                }
                            }
                            else if (_im30SaleTrans.FinalResult.IsTimeout)
                                _msg.ShowMessage($@"Transaction timeout.");

                            else if (_im30SaleTrans.FinalResult.IsManualStopped)
                                _msg.ShowMessage($@"Transaction has been stopped and terminated.");

                            else
                            {
                                if (_im30SaleTrans.FinalResult.Error != null)
                                    _msg.ShowMessage($@"Error when Credit Card Sale; Process Id : {processId}{"\r\n"}" + _im30SaleTrans.FinalResult.Error.ToString());

                                else
                                    _msg.ShowMessage($@"Error when Credit Card Sale; Unknown error; Process Id : {processId}\r\n");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //CYA-DEBUG .. Log Error Here
                    }
                }), processId.ToString(), 10, _logChannel
                , threadPriority: System.Threading.ThreadPriority.AboveNormal);
            }
        }

        private void ChargeCreditCard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                e.Handled = true;

                if (_im30SaleTrans is null)
                {
                    throw new Exception("Please create new card transaction");
                }
                else if (string.IsNullOrWhiteSpace(TxtDocNo.Text.Trim()))
                {
                    throw new Exception("Please valid Document Number");
                }
                else if (decimal.TryParse(TxtPrice.Text, out decimal priceRMAmount))
                {
                    if (_im30SaleTrans.Send2ndTransCommand(new CreditDebitChargeParam(TxtDocNo.Text.Trim(), priceRMAmount)))
                    {
                        _msg.ShowMessage("Charge Credit Card parameters has been sent successful");
                    }
                    else
                    {
                        _msg.ShowMessage("Sending Deny; Existing command parameter has been sent already");
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
                if (_im30SaleTrans is null)
                {
                    throw new Exception("Please create new card transaction");
                }
                else if (string.IsNullOrWhiteSpace(TxtDocNo.Text.Trim()))
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

                    if (_im30SaleTrans.Send2ndTransCommand(new TnGEntryCheckinParam(TxtDocNo.Text.Trim(), penaltyRMAmount, transDateTime)))
                    {
                        _msg.ShowMessage("TnG Check-in parameters has been sent successful");
                    }
                    else
                    {
                        _msg.ShowMessage("Sending denied; Existing command parameter has been sent already");
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
                if (_im30SaleTrans is null)
                {
                    throw new Exception("Please create new card transaction");
                }
                else if (string.IsNullOrWhiteSpace(TxtDocNo.Text.Trim()))
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

                    if (_im30SaleTrans.Send2ndTransCommand(new TnGExitCheckoutParam(TxtDocNo.Text.Trim(), priceRMAmount, penaltyRMAmount, transDateTime)))
                    {
                        _msg.ShowMessage("TnG Checkout parameters has been sent successful");
                    }
                    else
                    {
                        _msg.ShowMessage("Sending denied; Existing command parameter has been sent already");
                    }
                }
                else
                    throw new Exception("Unable to read Penalty Amount and Fare Amount. Please enter valid numeric Penalty Amount and Fare Amount in Ringgit.");
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
                if (_im30SaleTrans != null)
                {
                    if (_im30SaleTrans.IsCurrentWorkingEnded == false)
                    {
                        _msg.ShowMessage("Request Stop Sale Transaction");
                        _im30SaleTrans.RequestStopTransaction(out _);
                    }
                    else
                    {
                        _msg.ShowMessage("Existing Sale Transaction has already disposed");
                    }
                }
                else
                {
                    _msg.ShowMessage("Existing Sale Transaction has not found");
                }
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void DisposeSale_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DisposeSale();
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void DisposeSale()
        {
            if (_im30SaleTrans != null)
            {
                if (_im30SaleTrans.IsCurrentWorkingEnded == false)
                {
                    _msg.ShowMessage("Dispose existing Sale Transaction");
                    _im30SaleTrans.Dispose();
                    _im30SaleTrans = null;
                }
                else
                {
                    _msg.ShowMessage("Existing Sale Transaction has already disposed");
                }
            }
            else
            {
                _msg.ShowMessage("Existing Sale Transaction has not found");
            }
        }

        
    }
}