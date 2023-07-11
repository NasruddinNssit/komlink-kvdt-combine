using komlink.Views.komlink;
using Komlink;
using Komlink.Models;
using Komlink.Views.Komlink.Payment.PayWave;

//using kvdt_kiosk.Models;
//using kvdt_kiosk.Views.Printing;
using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.CreditDebit;
using NssIT.Kiosk.Device.PAX.IM30.AccessSDK;
using NssIT.Kiosk.Log.DB.StatusMonitor;
using NssIT.Kiosk.Tools.ThreadMonitor;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

namespace Komlink.Views.Komlink.Payment.PayWave
{
    /// <summary>
    /// Interaction logic for CardPayWave.xaml
    /// </summary>
    public partial class CardPayWave : Page
    {
        private Brush _activeButtonBackgroungColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xF4, 0x82, 0x20));
        private Brush _activeButtonForegroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

        private Brush _deactiveButtonBackgroungColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD));
        private Brush _deactiveButtonForegroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xBB, 0xBB, 0xBB));

        public event EventHandler<EndOfPaymentEventArgs> OnEndPayment;

        public delegate void CompletePayECRCallBack(TrxCallBackEventArgs e);
        public delegate void PayWaveProgress(InProgressEventArgs e);

        private string LogChannel = "CreditCardPaymentUI";
        private const string TransCanceledTag = "Transaction Canceled";

        // private DbLog _log = null;
        private string _currProcessId = "-";
        private decimal _amount = 0M;
        private string _currency = "";

        private string _disableTag = "DISABLED";
        private const int _defaultCountDelay = 72;
        private const int _saleMaxWaitingSec = 330;
        private int _delayCountDown = _defaultCountDelay;
        private const int _maxRetryCount = 3;

        private string _bankRefNo = null;
        private int _tryWaveCount = 1;
        private string _comPort = App.PayWaveCOMPORT;

        private bool _isPageLoaded = false;
        private bool _earlyAborted = false;
        //private bool _allowedRetry = false;
        private bool _cancelTrans = false;
        private bool _payWaveResponseFound = false;
        private bool _exitLastProcess = false;
        private bool _endTransactionFlag = false;

        public ResponseInfo SaleResult { get; private set; } = null;
        public ResponseInfo SettlementResult { get; private set; } = null;
        public bool IsSaleSuccess { get; private set; }
        public TransactionCategoryV2 TransactionCat { get; private set; } = TransactionCategoryV2.Sale;

        private ResourceDictionary _languageResource = null;

        private Thread _timerThreadWorker = null;
        private bool _isTimerEnabled = false;

        //public DbLog Log
        //{
        //    get
        //    {
        //        return _log ?? (_log = DbLog.GetDbLog());
        //    }
        //}

        ///// CYA-DEBUG-----IM30 Implementation.. add IM30AccessSDK here and addd event IM30AccessSDK.OnTransactionResponse


        private IM30AccessSDK _cardPayReader = null;
        private IM30AccessSDK CardPayReader
        {
            get
            {
                return _cardPayReader ?? (_cardPayReader = NewCardPayReader());

                IM30AccessSDK NewCardPayReader()
                {
                    if (_isPageLoaded)
                    {
                        IM30AccessSDK cardPayReader = new IM30AccessSDK(_comPort, null);

                        cardPayReader.OnTransactionResponse += CardPayReader_OnTransactionResponse;
                        cardPayReader.SetStatusMonitor(new NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus.IsCardMachineDataCommNormalDelg(StatusHub.GetStatusHub().zNewStatus_IsCardMachineDataCommNormal));
                        _cardPayReader = cardPayReader;

                        return cardPayReader;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        //private PayECRAccess PayWaveAccs
        //{
        //    get
        //    {
        //        return _payWaveAccs ?? (_payWaveAccs = NewPayECRAccess());

        //        PayECRAccess NewPayECRAccess()
        //        {
        //            PayECRAccess payWvAccs = PayECRAccess.GetPayECRAccess(_comPort, PayECRAccess.SaleMaxWaitingSec); /*, @"C:\eTicketing_Log\ECR_Receipts\", @"C:\eTicketing_Log\ECR_LOG", true, true);*/
        //            payWvAccs.OnCompleteCallback += OnCompletePayECRCallBack;
        //            payWvAccs.OnInProgressCall += OnPayECRInProgressCall;
        //            return payWvAccs;
        //        }
        //    }
        //}

        private static Lazy<CardPayWave> _creditCardPayWavePage = new Lazy<CardPayWave>(() => new CardPayWave());

        public static CardPayWave GetCreditCardPayWavePage()
        {
            return _creditCardPayWavePage.Value;
        }

        public CardPayWave()
        {
            InitializeComponent();
            InitPaymentData(UserSession.CurrencyCode, UserSession.TotalTicketPrice, UserSession.SessionId, App.PayWaveCOMPORT, null);
            LoadLanguage();
            _isTimerEnabled = false;
            _timerThreadWorker = new Thread(TimerThreadWorking);
            _timerThreadWorker.IsBackground = true;
            _timerThreadWorker.Start();
            Page_Loaded();

        }

        private void LoadLanguage()
        {
            Dispatcher.Invoke(() =>
            {
                if (App.Language == "ms")
                {
                    lblBalance.Text = "BAKI ";
                    lblToPay.Text = "JUMLAH HARGA";
                    lblTitle.Text = "PEMBAYARAN KAD DEBIT ATAU KREDIT";
                    TxtName.Text = "BATAL";
                }
            });
        }

        public void InitPaymentData(string currency, decimal amount, string refNo, string comport, ResourceDictionary languageResource)
        {

            _languageResource = languageResource;
            _currency = currency ?? "*";
            _amount = UserSession.TotalTicketPrice;
            _currProcessId = (refNo ?? "").Trim();
            _comPort = (comport ?? "").Trim();

            _firstErrorMsg = null;
            _bankRefNo = null;
            IsSaleSuccess = false;
            TransactionCat = TransactionCategoryV2.Sale;
            _delayCountDown = _defaultCountDelay;
            _tryWaveCount = 1;
            _earlyAborted = false;
            //_allowedRetry = false;
            _cancelTrans = false;
            _payWaveResponseFound = false;
            _exitLastProcess = false;
            _endTransactionFlag = false;
            SaleResult = null;

            _debugAlreadyTestSubmitPayment = false;

            //_completeECRHandle = new CompletePayECRCallBack(OnECRComplete);
            //_ECRProgress = new PayWaveProgress(ECRWorkProg);
        }

        public void ClearEvents()
        {
            if (OnEndPayment != null)
            {
                Delegate[] delgList = OnEndPayment?.GetInvocationList();
                foreach (EventHandler<EndOfPaymentEventArgs> delg in delgList)
                    try
                    {
                        OnEndPayment -= delg;
                    }
                    catch { }
            }
        }


      

        private bool? _isStartCardTransSuccess = null;
        ///// CYA-DEBUG-----IM30 Implementation.. IM30AccessSDK payment below.
        private void Page_Loaded()
        {
            _isPageLoaded = true;
            App.IsAutoTimeoutExtension = true;
            _isStartCardTransSuccess = null;

            // App.TimeoutManager.ResetTimeout();

            if (_languageResource != null)
            {
                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(_languageResource);
            }

            //Disable Cancel Button
            BdCancel.Background = _deactiveButtonBackgroungColor;
            TxtName.Foreground = _deactiveButtonForegroundColor;
            _cancelButtonEnabled = false;
            BdCancel.Tag = _disableTag;
            //--------------------------------------------------------

            TxtMacBusy.Visibility = Visibility.Visible;
            TxtError.Visibility = Visibility.Collapsed;
            TxtError.Text = "";

            TxtPayAmount.Text = $@"{_currency} {_amount:#,###.00}";
            TxtPayBalance.Text = $@"{_currency} {_amount:#,###.00}";

            TxtMacBusy.Text = "Sentuh (masukkan) card anda / Tap (insert) your card";
            TxtInProgress.Text = "";

            TxtTimer.Text = $@"({(_delayCountDown - 10).ToString()})";

            TxtRefNo.Text = $@"Ref.No.: {_currProcessId}    Time: {DateTime.Now: HH:mm:ss}";

            EnableCancelButton();

            if (App.SysParam?.PrmNoPaymentNeed == false)
            {
                Log.Information(LogChannel, _currProcessId, $@"Initiate Card App. (Amount : {_amount:#,###.00})", "A05", "pgCreditCardPayWaveV2.Page_Loaded",
                                     $@"========== ========== Initiate Card App. (Amount : {_amount:#,###.00})========== ========== ");

                //PayWaveAccs.Pay(_currProcessId, $@"{_amount:#,###.00}", AccountType.CreditCard, _currProcessId, null, null);

            }
            _isStartCardTransSuccess = CardPayReader.StartCardTransaction(1, 2, (_defaultCountDelay - 10), 10, out _, out Exception errorX);

            _isTimerEnabled = true;
            System.Windows.Forms.Application.DoEvents();

            this.Focus();
        }


        ///// CYA-DEBUG-----IM30 Implementation.. IM30AccessSDK below
        private void TimerThreadWorking()
        {
            //_disposed = false
            string defaultErrorMsg = "";
            while (_disposed == false)
            {
                bool endFlag = false;
                if ((_isTimerEnabled) && (_isPageLoaded))
                {
                    try
                    {
                        if (_delayCountDown >= 1)
                        {
                            // App.MarkLog.CreditCard.LogMark($@"UITimer#{_delayCountDown}");

                            _delayCountDown -= 1;

                            string runChr = "";
                            int tmCount = _delayCountDown - 10;
                            if (tmCount < 0) tmCount = 0;

                            if (((_delayCountDown % 2) == 0) && (tmCount == 0))
                                runChr = " * ";

                            this.Dispatcher.Invoke(() =>
                            {
                                TxtTimer.Text = $@"({tmCount.ToString()}) {runChr}";
                            });

                            if ((_delayCountDown <= 10) && (_earlyAborted == false) && (_exitLastProcess == false) && (_payWaveResponseFound == false))
                            {
                                if (_cancelButtonEnabled)
                                {
                                    DisableCancelButton();
                                }

                                if (SaleResult == null)
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        TxtTimer.Text = $@"({tmCount.ToString()}) {runChr}";
                                        TxtMacBusy.Text = "Timeout; No customer response.";
                                        TxtInProgress.Text = "Timeout; No customer response.";
                                        defaultErrorMsg = "Timeout; No customer response.";
                                        _delayCountDown = 20;
                                    });
                                }

                                _earlyAborted = true;

                                try
                                {
                                    CardPayReader.StopCardTransaction(out _);
                                }
                                catch { }

                                Log.Information(LogChannel, _currProcessId, "PayWaveAccs.ForceToCancel(); Timeout; No customer response.", "T01", "frmPayWave.TimerThreadWorking");

                                _exitLastProcess = true;
                            }
                            else if ((_delayCountDown == 0) && (_exitLastProcess == false) && (_payWaveResponseFound == false))
                            {
                                if (_cancelButtonEnabled)
                                {
                                    DisableCancelButton();
                                }

                                if (SaleResult == null)
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        TxtMacBusy.Text = "Timeout; No customer response.";
                                        TxtInProgress.Text = "Timeout; No customer response.";
                                        defaultErrorMsg = "Timeout; No customer response.";
                                        _delayCountDown = 20;
                                    });
                                }

                                try
                                {
                                    CardPayReader.StopCardTransaction(out _);
                                }
                                catch { }

                                Log.Information(LogChannel, _currProcessId, "PayWaveAccs.ForceToCancel(); Timeout; No customer response.", "T02", "frmPayWave.TimerThreadWorking");

                                _exitLastProcess = true;
                            }
                            else if ((_delayCountDown == 0) && (_exitLastProcess == false) && (_payWaveResponseFound == true))
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    TxtMacBusy.Text = "Local Timeout; Unable to get response from card server.";
                                    TxtInProgress.Text = "Local Timeout; Unable to get response from card server.";
                                });

                                defaultErrorMsg = "Local Timeout; Unable to get response from card server.";

                                _delayCountDown = 20;
                                try
                                {
                                    CardPayReader.StopCardTransaction(out _);
                                }
                                catch { }

                                Log.Information(LogChannel, _currProcessId, "PayWaveAccs.ForceToCancel(); Local Timeout; Unable to get response from card server", "T03", "frmPayWave.TimerThreadWorking");

                                _exitLastProcess = true;
                            }
                            else if ((_delayCountDown == 0) && (_payWaveResponseFound == true))
                            {
                                endFlag = true;
                            }

                        }
                        //else if (_isStartCardTransSuccess == false)
                        //{
                        //    try
                        //    {
                        //        ...need to show error to screen ..
                        //        CardPayReader.StopCardTransaction(out _);
                        //    }
                        //    catch { }

                        //    //endFlag = true;
                        //    App.MarkLog.CreditCard.LogMark($@"UITimer#FAIL_START_READER");
                        //}
                        else if (_exitLastProcess == true)
                        {
                            endFlag = true;
                            //  App.MarkLog.CreditCard.LogMark($@"UITimer#ENDING");
                        }
                    }
                    catch (Exception ex)
                    {
                        //  App.MarkLog.CreditCard.LogMark($@"UITimer#ERROR");

                        Log.Information(LogChannel, _currProcessId, new Exception("Error when PayWave UI Process. ", ex), "EX01", "frmPayWave.TimerThreadWorking");

                        if (defaultErrorMsg.Length > 0)
                            defaultErrorMsg = $@"{defaultErrorMsg} & System Error when PayWave UI Process. {ex.Message}";
                        else
                            defaultErrorMsg = $@"System Error when PayWave UI Process. {ex.Message}";
                    }
                    finally
                    {
                        if (!endFlag)
                        {
                            //  App.MarkLog.CreditCard.LogMark($@"UITimer#Next");
                        }
                        //else if (_allowedRetry)
                        //{
                        //    _isTimerEnabled = false;
                        //    App.MarkLog.CreditCard.LogMark($@"UITimer#QUIT", true);

                        //    this.Dispatcher.Invoke(() => {
                        //        RetryPayment();
                        //    });
                        //}
                        else /* if (!_allowedRetry) */
                        {
                            _isTimerEnabled = false;
                            //  App.MarkLog.CreditCard.LogMark($@"UITimer#QUIT", true);

                            // App.ShowDebugMsg("End PayTimer_Tick");

                            if (defaultErrorMsg.Length == 0)
                                defaultErrorMsg = "Fail to make paywave payment. Quit 01.";

                            if (SaleResult == null)
                                SaleResult = new ResponseInfo() { ResponseMsg = "SALE", StatusCode = "99", ErrorMsg = (_firstErrorMsg ?? defaultErrorMsg) };

                            if (SettlementResult == null)
                                SettlementResult = new ResponseInfo() { ResponseMsg = "SETTLEMENT", StatusCode = "99", ErrorMsg = "Fail to make paywave payment. Quit 01." };

                            Log.Information(LogChannel, _currProcessId, "Ending Paywave (1/2)", "B01", "frmPayWave.TimerThreadWorking");

                            this.Dispatcher.Invoke(() =>
                            {
                                TxtMacBusy.Visibility = Visibility.Visible;
                                TxtMacBusy.Text = "Ending Transaction .. ";
                                TxtInProgress.Text = "Wait for Ending Transaction.";
                            });

                            try
                            {
                                CardPayReader.StopCardTransaction(out _);
                            }
                            catch { }

                            CardPayReader.Dispose();
                            ;

                            //close window
                            this.Dispatcher.Invoke(() =>
                            {
                                Window.GetWindow(this).Close();
                                PrintingTemplateScreen printingTemplateScreen = new PrintingTemplateScreen();

                                Window mainWindow = Application.Current.MainWindow;
                                mainWindow.Content = printingTemplateScreen;
                            });

                            //Window mainWindow = Application.Current.MainWindow;
                            //PrintingTemplateScreen printingTemplateScreen = new PrintingTemplateScreen();
                            //mainWindow.Content = printingTemplateScreen;


                            Log.Information(LogChannel, _currProcessId, "Ending Paywave (2/2)", "B01", "frmPayWave.TimerThreadWorking");

                            if (TransactionCat == TransactionCategoryV2.Sale)
                            {
                                if (SaleResult?.StatusCode?.Equals(ResponseCodeDef.Approved) == true)
                                {
                                    RaiseOnEndPayment(NssIT.Kiosk.AppDecorator.Common.PaymentResult.Success, SaleResult);
                                }
                                else if (_cancelTrans == true)
                                {
                                    RaiseOnEndPayment(NssIT.Kiosk.AppDecorator.Common.PaymentResult.Cancel, null);
                                }
                                else
                                {
                                    RaiseOnEndPayment(NssIT.Kiosk.AppDecorator.Common.PaymentResult.Fail, SaleResult);
                                }
                            }
                            else
                            {
                                RaiseOnEndPayment(NssIT.Kiosk.AppDecorator.Common.PaymentResult.Fail, null);
                            }
                        }
                    }
                }

                //if (_isTimerEnabled)
                //    Thread.Sleep(200);
                //else
                Thread.Sleep(1000);
            }
        }

        ///// CYA-DEBUG-----IM30 Implementation.. IM30AccessSDK.OnTransactionResponse Below, .. Send 2nd Command when card detected.
        ///// CardPayReader_OnTransactionResponse similar to OnECRComplete of IM20
        private string _firstErrorMsg = null;
        private void CardPayReader_OnTransactionResponse(object sender, NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.CardTransResponseEventArgs e)
        {
            //_allowedRetry = false;

            Log.Information(LogChannel, _currProcessId, ("Final Result", e), "A01", "pgCreditCardPayWaveV2.CardPayReader_OnTransactionResponse",
                        NssIT.Kiosk.AppDecorator.Log.MessageType.Error);

            if (e.EventCode == TransEventCodeEn.CardInfoResponse)
            {
                CardInfoReceived(e);
            }
            else if (e.TransType == TransactionTypeEn.CreditCard_2ndComm)
            {
                _endTransactionFlag = true;
                CrediDebitChargingReceivedResult(e);
            }
            else if (e.EventCode == TransEventCodeEn.ManualStop)
            {
                _endTransactionFlag = true;
                CancelStopReceived();
            }
            else if (e.EventCode == TransEventCodeEn.Timeout)
            {
                _endTransactionFlag = true;
                TimeoutReceived();
            }
            else
            {
                _endTransactionFlag = true;
                UnknowErrorReceived();
            }

            return;
            /////==============================================================================================
            void CardInfoReceived(CardTransResponseEventArgs eArg)
            {
                Log.Information(LogChannel, _currProcessId, "..Card Info", "A01", "pgCreditCardPayWaveV2.CardInfoReceived");

                if (_payWaveResponseFound == false)
                {
                    _payWaveResponseFound = true;

                    _cancelTrans = false;
                    _exitLastProcess = false;
                    _earlyAborted = false;
                    //_allowedRetry = false;
                }

                if (_cancelButtonEnabled)
                {
                    DisableCancelButton();
                }

                if ((eArg.ResponseInfo is CreditDebitCardInfoResp cdCardInfo))
                {
                    if ((cdCardInfo.IsDataFound) && (cdCardInfo.DataError is null))
                    {
                        if (CardPayReader.CreditDebitChargeCard(_currProcessId, _amount, out Exception errX))
                        {
                            if (_endTransactionFlag == false)
                                _delayCountDown = _saleMaxWaitingSec;

                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                if (!IsSaleSuccess)
                                    TxtMacBusy.Text = "Card found ..";

                                string msgX = e.Message ?? "Card found ..";
                                TxtInProgress.Text = msgX;
                            }));
                        }
                        else
                        {
                            try
                            {
                                CardPayReader.StopCardTransaction(out _);
                            }
                            catch { }

                            string errMsg = "";

                            if (errX != null)
                                errMsg = $@"{errX.Message}; Please try again";
                            else
                                errMsg = "Fail payment transaction; Please try again";

                            _firstErrorMsg = _firstErrorMsg ?? errMsg;

                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                TxtMacBusy.Visibility = Visibility.Collapsed;
                                TxtError.Text = errMsg;
                                TxtError.Visibility = Visibility.Visible;
                            }));

                            //_allowedRetry = false;
                            _exitLastProcess = true;
                            _delayCountDown = 5;
                        }
                    }
                    else
                    {
                        try
                        {
                            CardPayReader.StopCardTransaction(out _);
                        }
                        catch { }

                        _firstErrorMsg = _firstErrorMsg ?? "Unable to read card info";

                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            TxtMacBusy.Visibility = Visibility.Collapsed;
                            TxtError.Text = "Unable to read card info; Please try again";
                            TxtError.Visibility = Visibility.Visible;
                        }));

                        //_allowedRetry = false;
                        _exitLastProcess = true;
                        _delayCountDown = 5;
                    }
                }
                else if (eArg.ResponseInfo is ErrorResponse errResp)
                {
                    try
                    {
                        CardPayReader.StopCardTransaction(out _);
                    }
                    catch { }

                    string errMsg = "";

                    if (errResp.DataError != null)
                        errMsg = $@"{errResp.DataError.Message}; Please try again";
                    else
                        errMsg = "Fail payment transaction; Please try again";

                    _firstErrorMsg = _firstErrorMsg ?? errMsg;

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TxtMacBusy.Visibility = Visibility.Collapsed;
                        TxtError.Text = errMsg;
                        TxtError.Visibility = Visibility.Visible;
                    }));

                    //_allowedRetry = false;
                    _exitLastProcess = true;
                    _delayCountDown = 5;
                }
                else
                {
                    try
                    {
                        CardPayReader.StopCardTransaction(out _);
                    }
                    catch { }

                    _firstErrorMsg = _firstErrorMsg ?? "Only Credit/Debit card is allowed";

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TxtMacBusy.Visibility = Visibility.Collapsed;
                        TxtError.Text = "Only Credit/Debit card is allowed";
                        TxtError.Visibility = Visibility.Visible;
                    }));

                    //_allowedRetry = false;
                    _exitLastProcess = true;
                    _delayCountDown = 5;
                }
            }

            void CrediDebitChargingReceivedResult(CardTransResponseEventArgs eArg)
            {
                Log.Information(LogChannel, _currProcessId, "..Final Credir/Debit Card Data", "A01", "pgCreditCardPayWaveV2.CrediDebitChargingReceivedResult");

                if ((eArg.ResponseInfo is CreditDebitChargeCardResp cdCardInfo)
                    &&
                    (cdCardInfo.ResponseResult == ResponseCodeEn.Success)
                )
                {
                    ResponseInfo currResult = new ResponseInfo()
                    {
                        AdditionalData = cdCardInfo.POSTransactionID ?? "",
                        AID = cdCardInfo.AID ?? "",
                        Amount = Convert.ToInt32(Math.Floor((cdCardInfo.Amount * 100))).ToString().Trim(),
                        ApprovalCode = cdCardInfo.ApprovalCode ?? "",
                        CardholderName = cdCardInfo.CardHolderName ?? "",
                        CardNo = (cdCardInfo.CardNumber ?? "").Replace("*", "X"),
                        CurrencyAmount = cdCardInfo.Amount,
                        ExpiryDate = "****",
                        HostNo = "01",
                        MID = cdCardInfo.MerchantNumber ?? "",
                        ReportTime = cdCardInfo.TransactionDateTime ?? DateTime.Now,
                        RRN = cdCardInfo.RRN ?? "",
                        StatusCode = ResponseCodeDef.Approved,
                        TC = cdCardInfo.Cryptogram ?? "",
                        TID = cdCardInfo.TID ?? "",
                        TransactionTrace = cdCardInfo.STAN ?? "",
                        BatchNumber = cdCardInfo.BatchNumber ?? "",
                        CardType = cdCardInfo.CardIssuerID ?? "",
                        Tag = (cdCardInfo.TransactionDateTime ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    TransactionCat = TransactionCategoryV2.Sale;
                    SaleResult = currResult;

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TxtMacBusy.Text = "Sale is successful.";
                        TxtInProgress.Text = "Sale is successful.";
                    }));
                    IsSaleSuccess = true;

                    _exitLastProcess = true;
                    _delayCountDown = 3;
                    //--------------------------------
                    string cdNo = (currResult.CardNo ?? "*").Trim();

                    if (cdNo.Length >= 4)
                        cdNo = cdNo.Substring((cdNo.Length - 4));

                    Log.Information(LogChannel, _currProcessId, $@"Card Transaction Success with last 4 digit numbers #{cdNo}#", "A05", "pgCreditCardPayWaveV2.CrediDebitChargingReceivedResult",
                                    $@"Card Transaction Success with last 4 digit numbers #{cdNo}#");
                }
                else
                {
                    if (IsSaleSuccess)
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            TxtMacBusy.Text = "Sale is successful.";
                            TxtInProgress.Text = "Sale is successful.";
                        }));
                    }
                    else
                    {
                        string respTxt = "";
                        if (eArg.ResponseInfo is CreditDebitChargeCardResp r1)
                        {
                            respTxt = (r1.ResponseText ?? "").Trim();
                        }

                        string errMsg = (respTxt.Length > 0) ? $@"#{respTxt}#" : "Fail card transaction";

                        _firstErrorMsg = _firstErrorMsg ?? errMsg;
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            TxtMacBusy.Visibility = Visibility.Collapsed;
                            TxtError.Text = errMsg;
                            TxtError.Visibility = Visibility.Visible;
                        }));
                        DisableCancelButton();
                    }

                    _exitLastProcess = true;

                    //if (_allowedRetry)
                    //    _delayCountDown = 5;
                    //else 
                    if (_cancelTrans)
                        _delayCountDown = 3;
                    else
                        _delayCountDown = 7;
                }
            }

            void CancelStopReceived()
            {
                Log.Information(LogChannel, _currProcessId, "..Cancel / Stop", "A01", "pgCreditCardPayWaveV2.CancelStopReceived");

                string errMsg = "Cancel /Stop transaction";

                if (IsSaleSuccess)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TxtMacBusy.Text = "Sale is successful.";
                        TxtInProgress.Text = "Sale is successful.";
                    }));
                }
                else
                {
                    _firstErrorMsg = _firstErrorMsg ?? errMsg;
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TxtMacBusy.Visibility = Visibility.Collapsed;
                        TxtError.Text = errMsg;
                        TxtError.Visibility = Visibility.Visible;
                    }));
                    DisableCancelButton();
                }

                _exitLastProcess = true;

                //if (_allowedRetry)
                //    _delayCountDown = 5;
                //else 
                if (_cancelTrans)
                    _delayCountDown = 3;
                else
                    _delayCountDown = 7;
            }

            void TimeoutReceived()
            {
                Log.Information(LogChannel, _currProcessId, "..Timeout", "A01", "pgCreditCardPayWaveV2.TimeoutReceived");

                string errMsg = "Transaction timeout";

                if (IsSaleSuccess)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TxtMacBusy.Text = "Sale is successful.";
                        TxtInProgress.Text = "Sale is successful.";
                    }));
                }
                else
                {
                    _firstErrorMsg = _firstErrorMsg ?? errMsg;
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TxtMacBusy.Visibility = Visibility.Collapsed;
                        TxtError.Text = errMsg;
                        TxtError.Visibility = Visibility.Visible;
                    }));
                    DisableCancelButton();
                }

                _exitLastProcess = true;

                //if (_allowedRetry)
                //    _delayCountDown = 5;
                //else 
                if (_cancelTrans)
                    _delayCountDown = 3;
                else
                    _delayCountDown = 7;
            }

            void UnknowErrorReceived()
            {
                Log.Information(LogChannel, _currProcessId, "..Unknow Error Received", "A01", "pgCreditCardPayWaveV2.UnknowErrorReceived");

                try
                {
                    CardPayReader.StopCardTransaction(out _);
                }
                catch { }

                string errMsg = "-Error (U)~";

                if (IsSaleSuccess)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TxtMacBusy.Text = "Sale is successful.";
                        TxtInProgress.Text = "Sale is successful.";
                    }));
                }
                else
                {
                    _firstErrorMsg = _firstErrorMsg ?? errMsg;
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TxtMacBusy.Visibility = Visibility.Collapsed;
                        TxtError.Text = errMsg;
                        TxtError.Visibility = Visibility.Visible;
                    }));
                    DisableCancelButton();
                }

                _exitLastProcess = true;

                //if (_allowedRetry)
                //    _delayCountDown = 5;
                //else 
                if (_cancelTrans)
                    _delayCountDown = 3;
                else
                    _delayCountDown = 7;
            }
        }

        //private void OnECRComplete______History(TrxCallBackEventArgs e)
        //{
        //    _endTransactionFlag = true;
        //    ResponseInfo currResult = e.Result;
        //    _allowedRetry = false;

        //    if (currResult != null)
        //    {
        //        if (currResult.ResponseMsg == "SALE")
        //        {
        //            TransactionCat = TransactionCategoryV2.Sale;
        //            SaleResult = currResult;
        //        }
        //        else if (currResult.ResponseMsg == "SETTLEMENT")
        //        {
        //            TransactionCat = TransactionCategoryV2.Settlement;
        //            SettlementResult = currResult;
        //        }
        //    }

        //    if ((e.IsSuccess)
        //        && ((currResult.ResponseMsg == "SALE") || (currResult.ResponseMsg == "SETTLEMENT"))
        //        )
        //    {
        //        if (currResult.ResponseMsg == "SALE")
        //        {
        //            TxtMacBusy.Text = "Sale is successful.";
        //            TxtInProgress.Text = "Sale is successful.";
        //            IsSaleSuccess = true;

        //            // Below logic only valid if no need immediate SETTLEMENT
        //            _exitLastProcess = true;
        //            _delayCountDown = 3;
        //            //--------------------------------

        //            string cdNo = (currResult.CardNo ?? "*").Trim();

        //            if (cdNo.Length >= 4)
        //                cdNo = cdNo.Substring((cdNo.Length - 4));

        //            Log.LogText(LogChannel, _currProcessId, $@"Card Transaction Success with last 4 digit numbers #{cdNo}#", "A05", "pgCreditCardPayWaveV2.Page_Loaded",
        //                            adminMsg: $@"Card Transaction Success with last 4 digit numbers #{cdNo}#");

        //        }
        //        else if (currResult.ResponseMsg == "SETTLEMENT")
        //        {
        //            TxtInProgress.Text = "Settlement Completed";
        //            _exitLastProcess = true;
        //            _delayCountDown = 3;
        //        }
        //    }
        //    else
        //    {
        //        string errMsg = "";

        //        if (IsSaleSuccess)
        //        {
        //            TxtMacBusy.Text = "Sale is successful.";
        //            TxtInProgress.Text = "Sale is successful.";
        //        }
        //        else
        //        {
        //            if ((currResult != null) && ((currResult.StatusCode ?? "").Length > 0))
        //            {
        //                if ((_cancelTrans) && (currResult.StatusCode.Equals(ResponseStatusCode.TA)))
        //                    errMsg = TransCanceledTag;

        //                else if (currResult.StatusCode.Equals(ResponseStatusCode.Z1) || currResult.StatusCode.Equals(ResponseStatusCode.Z3))
        //                {
        //                    if (_tryWaveCount < _maxRetryCount)
        //                        _allowedRetry = true;

        //                    errMsg = currResult.ErrorMsg + $@". Try Wave Count : {_tryWaveCount.ToString()}";
        //                    currResult.ErrorMsg = errMsg;
        //                }
        //                else
        //                    errMsg = currResult.ErrorMsg;
        //            }
        //            else if ((e.Error != null) && ((e.Error.Message ?? "").Trim().Length > 0))
        //                errMsg = e.Error.Message;

        //            else
        //                errMsg = "System encounter error at the moment. Please try later.";

        //            _firstErrorMsg = _firstErrorMsg ?? errMsg;

        //            if (currResult != null)
        //                currResult.ErrorMsg = errMsg;

        //            TxtMacBusy.Visibility = Visibility.Collapsed;
        //            TxtError.Text = errMsg;
        //            TxtError.Visibility = Visibility.Visible;
        //            DisableCancelButton();
        //        }

        //        // Delay 12 seconds for exit.
        //        // Enable Cancel Button for exit immediately.
        //        _exitLastProcess = true;

        //        if (_allowedRetry)
        //            _delayCountDown = 5;
        //        else if (errMsg.Equals(TransCanceledTag))
        //            _delayCountDown = 3;
        //        else
        //            _delayCountDown = 10;
        //    }

        //}


        ///// CYA-DEBUG-----IM30 Implementation.. event IM30AccessSDK.OnTransactionResponse Extension, show card detected when touch card.
        //private void ECRWorkProg(InProgressEventArgs e)
        //{
        //    if (_payWaveResponseFound == false)
        //    {
        //        if (_endTransactionFlag == false)
        //            _delayCountDown = PayECRAccess.SaleMaxWaitingSec + 30;

        //        _payWaveResponseFound = true;

        //        _cancelTrans = false;
        //        _exitLastProcess = false;
        //        _earlyAborted = false;
        //        //_allowedRetry = false;
        //    }

        //    if (!IsSaleSuccess)
        //        TxtMacBusy.Text = "Processing ..";

        //    string msgX = e.Message ?? "..";

        //    TxtInProgress.Text = msgX;

        //    if (_cancelButtonEnabled)
        //    {
        //        DisableCancelButton();
        //    }
        //}

        ///// CYA-DEBUG-----IM30 Implementation..  Implement event IM30AccessSDK.OnTransactionResponse

        //private void OnCompletePayECRCallBack(object sender, TrxCallBackEventArgs e)
        //{
        //    if (_completeECRHandle != null)
        //        this.Dispatcher.Invoke(_completeECRHandle, e);
        //}
        //private void OnPayECRInProgressCall(object sender, InProgressEventArgs e)
        //{
        //    if (_ECRProgress != null)
        //        this.Dispatcher.Invoke(_ECRProgress, e);
        //}

        ///// CYA-DEBUG-----IM30 Implementation..  dispose IM30AccessSDK here and remove event IM30AccessSDK.OnTransactionResponse

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _isPageLoaded = false;
            // App.MarkLog.CreditCard.LogMark("#pgCard.PageUnloaded>>A01#", true);

            Log.Information(LogChannel, _currProcessId, "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX End of Card Payment Transaction XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", "B10", "IM30AccessSDK.StartCardTransaction");

            //if (_payWaveAccs != null)
            //{
            //    _payWaveAccs.OnCompleteCallback -= OnCompletePayECRCallBack;
            //    _payWaveAccs.OnInProgressCall -= OnPayECRInProgressCall;

            //    //_payWaveAccs.Dispose();
            //    _payWaveAccs = null;
            //}

            if (_cardPayReader != null)
            {
                _cardPayReader.OnTransactionResponse -= CardPayReader_OnTransactionResponse;

                //CYA-DEBUG if _cardPayReader success create transaction, then not need to stop;
                _cardPayReader.StopCardTransaction(out _);
                _cardPayReader.Dispose();
                _cardPayReader = null;
            }

            App.IsAutoTimeoutExtension = false;
        }

        ///// CYA-DEBUG-----IM30 Implementation.. IM30AccessSDK below.
        private void Cancel_Click(object sender, MouseButtonEventArgs e)
        {
            if (IsCancelAlreadyEnabled)
            {
                // App.MarkLog.CreditCard.LogMark("#pgCard.CancelClick>>A01#");
                DisableCancelButton();
                _cancelTrans = true;

                try
                {
                    CardPayReader.StopCardTransaction(out _);
                }
                catch { }

                if (App.SysParam.PrmNoPaymentNeed)
                {
                    _delayCountDown = 3;
                    RaiseOnEndPayment(NssIT.Kiosk.AppDecorator.Common.PaymentResult.Cancel, null);
                }
                else
                {
                    _delayCountDown = 20;
                }
                //  App.MarkLog.CreditCard.LogMark("#pgCard.CancelClick>>A20#");
            }
        }

        private void RaiseOnEndPayment(NssIT.Kiosk.AppDecorator.Common.PaymentResult paymentResult, ResponseInfo cardResponseResult)
        {
            try
            {
                string bankRefNo = null;

                if (cardResponseResult != null)
                    bankRefNo = cardResponseResult?.RRN;

                OnEndPayment?.Invoke(null, new EndOfPaymentEventArgs(_currProcessId, paymentResult, bankRefNo, SaleResult));
            }
            catch (Exception ex)
            {
                try
                {
                    CardPayReader.StopCardTransaction(out _);
                }
                catch { }

                Log.Information(LogChannel,
                    _currProcessId,
                    new NssIT.Kiosk.Log.DB.WithDataException($@"OnEndPayment is not handled; (EXIT10000930); {ex.Message}", ex, new List<object>(new object[] { paymentResult, cardResponseResult })),
                    "EX01", "pgCreditCardPayWaveV2.RaiseOnEndPayment", NssIT.Kiosk.AppDecorator.Log.MessageType.Error,
                     $@"{ex.Message}; (EXIT10000930)");

                //   App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000930)");
            }
        }

        ///// CYA-DEBUG-----IM30 Implementation.. IM30AccessSDK below
        //private void RetryPayment()
        //{
        //    //"Cuba lagi. Sentuh card anda / Try again. Tap your card";
        //    _tryWaveCount += 1;
        //    _delayCountDown = _defaultCountDelay;
        //    _exitLastProcess = false;
        //    _payWaveResponseFound = false;
        //    _bankRefNo = null;
        //    _earlyAborted = false;
        //    //_allowedRetry = false;
        //    _cancelTrans = false;
        //    _endTransactionFlag = false;
        //    SaleResult = null;
        //    IsSaleSuccess = false;
        //    TransactionCat = TransactionCategoryV2.Sale;

        //    DisableCancelButton();

        //    TxtError.Visibility = Visibility.Collapsed;
        //    TxtError.Text = "";
        //    EnableCancelButton();

        //    TxtPayAmount.Text = $@"{_currency} {_amount:#,###.00}";
        //    TxtPayBalance.Text = $@"{_currency} {_amount:#,###.00}";

        //    TxtMacBusy.Text = $"Cuba lagi. Sentuh (masukkan) card anda / Try again. Tap (insert) your card. Try:{_tryWaveCount.ToString()} of 3";
        //    TxtMacBusy.Visibility = Visibility.Visible;
        //    TxtInProgress.Text = "";

        //    TxtTimer.Text = $@"({(_delayCountDown - 10).ToString()})";

        //    System.Windows.Forms.Application.DoEvents();

        //    PayWaveAccs.Pay(_currProcessId, $@"{_amount:####.00}", AccountType.CreditCard, _currProcessId, null, null);

        //    _isTimerEnabled = true;
        //}

        private bool IsCancelAlreadyEnabled
        {
            get
            {
                bool ret = false;

                if (BdCancel.Tag is null)
                    ret = true;
                else if (BdCancel.Tag.Equals(_disableTag))
                    ret = false;
                else
                    ret = true;
                return ret;
            }
        }

        private bool _cancelButtonEnabled = false;
        private object _cancellingLock = new object();
        private void DisableCancelButton()
        {
            Exception err = null;

            //  App.MarkLog.CreditCard.LogMark("#pgCard.DisableCancelButton>>A01#");
            RunThreadMan tMan = new RunThreadMan(new Action(() =>
            {
                lock (_cancellingLock)
                {
                    //  App.MarkLog.CreditCard.LogMark("#pgCard.DisableCancelButton>>A01#");

                    try
                    {
                        //App.MarkLog.CreditCard.LogMark("#pgCard.DisableCancelButton>>A05#");

                        this.Dispatcher.Invoke(() =>
                        {
                            BdCancel.Background = _deactiveButtonBackgroungColor;
                            TxtName.Foreground = _deactiveButtonForegroundColor;
                            _cancelButtonEnabled = false;
                            BdCancel.Tag = _disableTag;
                            System.Windows.Forms.Application.DoEvents();
                        });
                        // App.MarkLog.CreditCard.LogMark("#pgCard.DisableCancelButton>>A10#");
                    }
                    catch (ThreadAbortException) { }
                    catch (Exception ex)
                    {
                        // App.MarkLog.CreditCard.LogMark("#pgCard.DisableCancelButton>>ErrA15#");
                        err = ex;
                    }
                }
            }), "pgCreditCardPayWaveV2.DisableCancelButton", 20, LogChannel);
            tMan.WaitUntilCompleted(waitWithWindowsDoEvents: true, sleepInterval: 50);

            //  App.MarkLog.CreditCard.LogMark("#pgCard.DisableCancelButton>>A100#", true);

            if (err != null)
                throw err;
        }


        ///// CYA-DEBUG-----IM30AccessSDK below

        private Thread _delayCancelActivationThreadWorker = null;
        private void EnableCancelButton()
        {
            // App.MarkLog.CreditCard.LogMark("#pgCard.EnableCancelButton>>A01#");

            _cancelButtonEnabled = true;
            EndOldCancelActivationThreadWorker();

            _delayCancelActivationThreadWorker = new Thread(new ThreadStart(delayCancelActivationThreadWorking));
            _delayCancelActivationThreadWorker.IsBackground = true;
            _delayCancelActivationThreadWorker.Start();

            return;

            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            void delayCancelActivationThreadWorking()
            {
                // App.MarkLog.CreditCard.LogMark("#pgCard.EnableCancelButton>>A03#");

                DateTime startTime = DateTime.Now;
                DateTime endTime = startTime.AddSeconds(7);

                while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
                {
                    Thread.Sleep(500);

                    if (_cardPayReader is null)
                        break;
                }

                //   App.MarkLog.CreditCard.LogMark("#pgCard.EnableCancelButton>>A05#");

                if (_cardPayReader != null)
                {
                    //   App.MarkLog.CreditCard.LogMark("#pgCard.EnableCancelButton>>A10#");

                    lock (_cancellingLock)
                    {
                        //     App.MarkLog.CreditCard.LogMark("#pgCard.EnableCancelButton>>A15#");

                        try
                        {
                            if (_cancelButtonEnabled)
                            {
                                //  App.MarkLog.CreditCard.LogMark("#pgCard.EnableCancelButton>>A20#");

                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    BdCancel.Background = _activeButtonBackgroungColor;
                                    TxtName.Foreground = _activeButtonForegroundColor;
                                    BdCancel.Tag = "";
                                    System.Windows.Forms.Application.DoEvents();
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            //  App.MarkLog.CreditCard.LogMark("#pgCard.EnableCancelButton>>ErrA25#");

                            Log.Information(LogChannel, _currProcessId, ex, "EX01",
                                "pgCreditCardPayWaveV2.delayCancelActivationThreadWorking", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
                        }

                        // App.MarkLog.CreditCard.LogMark("#pgCard.EnableCancelButton>>A30#");
                    }

                    // App.MarkLog.CreditCard.LogMark("#pgCard.EnableCancelButton>>A100#", true);
                }
            }

            void EndOldCancelActivationThreadWorker()
            {
                if (_delayCancelActivationThreadWorker != null)
                {
                    if (((_delayCancelActivationThreadWorker.ThreadState & ThreadState.Aborted) != ThreadState.Aborted)
                        && ((_delayCancelActivationThreadWorker.ThreadState & ThreadState.Stopped) != ThreadState.Stopped))
                    {
                        try
                        {
                            _delayCancelActivationThreadWorker.Abort();
                            Thread.Sleep(100);
                        }
                        catch { }
                        finally
                        {
                            _delayCancelActivationThreadWorker = null;
                        }
                    }
                }
            }
        }

        //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        private bool _debugAlreadyTestSubmitPayment = false;

        private void Test_SubmitPayment(object sender, MouseButtonEventArgs e)
        {
            if (App.SysParam.PrmNoPaymentNeed)
            {
                //CYA-DEMO
                if (_debugAlreadyTestSubmitPayment == false)
                {
                    SaleResult = null;
                    _bankRefNo = DateTime.Now.ToString("yyyyMMddHHmmss");
                    _debugAlreadyTestSubmitPayment = true;
                    RaiseOnEndPayment(NssIT.Kiosk.AppDecorator.Common.PaymentResult.Success, SaleResult);
                }
            }
        }

        private bool _disposed = false;
        public void Dispose()
        {
            _disposed = true;
        }

        //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

        public enum TransactionCategoryV2
        {
            Sale = 0,
            Settlement = 1
        }
    }
}
