
using Komlink.Models;
using Komlink.Services;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Komlink.Views.Payment.PayWave
{
    /// <summary>
    /// Interaction logic for CardPayWave.xaml
    /// </summary>
    public partial class CardPayWaveexe : Page
    {
        private Brush _activeButtonBackgroungColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xF4, 0x82, 0x20));
        private Brush _activeButtonForegroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

        private Brush _deactiveButtonBackgroungColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD));
        private Brush _deactiveButtonForegroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xBB, 0xBB, 0xBB));

        public event EventHandler<EndOfPaymentEventArgs> OnEndPayment;

        public event EventHandler OnStartPayment;

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

        private APIServices _apIServices = null;

        private DispatcherTimer checkKomlinkCardTimer = new DispatcherTimer();
        private DispatcherTimer scanTimer = new DispatcherTimer();

        private string _firstErrorMsg = null;



        private static Lazy<CardPayWaveexe> _creditCardPayWavePage = new Lazy<CardPayWaveexe>(() => new CardPayWaveexe());

        public static CardPayWaveexe GetCreditCardPayWavePage()
        {
            return _creditCardPayWavePage.Value;
        }

        private bool isIM30Ready = false;
        int count;
        public CardPayWaveexe()
        {
            InitializeComponent();
            //InitPaymentData(UserSession.CurrencyCode, UserSession.TotalTicketPrice, UserSession.SessionId, App.PayWaveCOMPORT, null);
            LoadLanguage();
            _apIServices = new APIServices(new APIURLServices(), new APISignatureServices());
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
            _amount = amount;
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
        private void Page_Loaded(object sender, RoutedEventArgs e)
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

            TxtTimer.Text = $@"(60)";
            lblCardPaymentMessage.Text = "Pembayaran";

            TxtRefNo.Text = $@"Ref.No.: {UserSession.requestAddTopUpModel.TransactionNo}    Time: {DateTime.Now: HH:mm:ss}";

            scanTimerCounter();
           
            if (App.SysParam?.PrmNoPaymentNeed == false)
            {
                Log.Information(LogChannel, _currProcessId, $@"Initiate Card App. (Amount : {_amount:#,###.00})", "A05", "pgCreditCardPayWaveV2.Page_Loaded",
                                     $@"========== ========== Initiate Card App. (Amount : {_amount:#,###.00})========== ========== ");

                StartIM30();

                _isTimerEnabled = true;
            }

            System.Windows.Forms.Application.DoEvents();



            this.Focus();
        }

        private async void StopTransaction()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = "http://127.0.0.1:1234/Para=18";
                    var response = await client.GetStringAsync(url);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void StartIM30()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"http://127.0.0.1:1234/Para=16&IMPR=1,2,{UserSession.requestAddTopUpModel.TransactionNo},{UserSession.TotalTicketPrice}";



                    var response = await client.GetStringAsync(url);

                    if (response != null)
                    {
                        //await Task.Delay(4000);
                        string[] arrayData = response.Split(',');

                        if (arrayData[0] == "0")
                        {
                            SetTimeInterval(GetIM30Result, 1000, 100);
                            isIM30Ready = true;

                            EnableCancelButton();
                        }
                        else
                        {
                            StartIM30();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async Task GetIM30Result()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync("http://127.0.0.1:1234/Para=17");
                    if (response != null)
                    {
                        string[] responseArray = response.Split(',');
                        if (responseArray[0] == "0" && responseArray.Count() > 3)
                        {
                            TxtMacBusy.Text = "Sale is successfull. ";
                            checkKomlinkCardTimer.Stop();
                            ResponseInfo currResult = new ResponseInfo()
                            {
                                AdditionalData = "",
                                AID = responseArray[12],
                                Amount = responseArray[17],
                                ApprovalCode = responseArray[2],
                                CardholderName = responseArray[11],
                                CardNo = responseArray[6],
                                CurrencyAmount = Convert.ToDecimal(responseArray[17]),
                                ExpiryDate = responseArray[8],
                                HostNo = "01",
                                MID = responseArray[4],
                                ReportTime = DateTime.Parse(responseArray[15]),
                                RRN = responseArray[10],
                                StatusCode = ResponseCodeDef.Approved,
                                TC = responseArray[16],
                                TID = responseArray[5],
                                TransactionTrace = responseArray[18],
                                BatchNumber = responseArray[9],
                                CardType = responseArray[14],
                                Tag = DateTime.Parse(responseArray[15]).ToString("yyyy-MM-dd HH:mm:ss"),


                            };
                            SaleResult = currResult;

                            count = 3;
                            SetTextToEndingTransaction();

                        }
                        else if (responseArray[0] == "11")
                        {
                            TxtMacBusy.Text = "Card found ..";
                            DisableCancelButton();
                        }
                        else if (responseArray[0] == "9" || responseArray[0] == "11" || responseArray[0] == "10")
                        {
                            TxtMacBusy.Text = responseArray[1];
                        }
                        else
                        {
                            if (responseArray[0] == "1")
                            {
                                string errMsg = responseArray[1];
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    TxtMacBusy.Visibility = Visibility.Collapsed;
                                    TxtError.Text = errMsg;
                                    TxtError.Visibility = Visibility.Visible;
                                }));

                                count = 3;
                                checkKomlinkCardTimer.Stop();
                                SetTextToEndingTransaction();
                            }

                        }
                    }
                }
            }
            catch
            {

            }
        }

        private void SetTextToEndingTransaction()
        {
            this.Dispatcher.Invoke(() =>
            {
                TxtMacBusy.Visibility = Visibility.Visible;
                TxtMacBusy.Text = "Ending Transaction .. ";
                TxtInProgress.Text = "Wait for Ending Transaction.";

            });
        }

        private void SetTimeInterval(Func<Task> callBack, int delay, int repetitions)
        {
            int x = 0;
            checkKomlinkCardTimer.Tick += async (sender, e) =>
            {
                await callBack();

                if (++x > repetitions)
                {
                    checkKomlinkCardTimer.Stop();
                }
            };

            checkKomlinkCardTimer.Interval = TimeSpan.FromMilliseconds(delay);

            checkKomlinkCardTimer.Start();
        }

        private void scanTimerCounter()
        {
            count = 60;
            try
            {
                scanTimer.Interval = TimeSpan.FromSeconds(1);
                scanTimer.Start();
               
                scanTimer.Tick += async (sender, e) =>
                {
                    count --;

                    if (count <= 0)
                    {
                        scanTimer.Stop();
                        count = 60;
                        TxtTimer.Text = $@"({count.ToString()})";
                       

                        if (SaleResult != null)
                        {
                            if (SaleResult?.StatusCode?.Equals(ResponseCodeDef.Approved) == true)
                            {
                                RaiseOnEndPayment(PaymentResult.Success, SaleResult);
                            }
                        }
                        else if (_cancelTrans)
                        {
                            RaiseOnEndPayment(PaymentResult.Cancel, null);
                        }
                        else
                        {
                            RaiseOnEndPayment(PaymentResult.Fail, null);
                        }
                    }
                    else
                    {
                        TxtTimer.Text = $@"({count.ToString()})";
                        if (count <= 3)
                        {
                            checkKomlinkCardTimer.Stop();
                            StopTransaction();
                        }

                        if (count <= 10)
                        {

                            if (SaleResult == null)
                            {
                                checkKomlinkCardTimer.Stop();
                                this.Dispatcher.Invoke(() =>
                                {
                                    TxtTimer.Text = $@"({count.ToString()})";
                                    TxtMacBusy.Text = "Timeout; No customer response.";
                                    TxtInProgress.Text = "Timeout; No customer response.";

                                    if (_cancelButtonEnabled)
                                    {
                                        DisableCancelButton();
                                    }
                                });
                            }

                            if (!isIM30Ready)
                            {
                                TxtTimer.Text = $@"({count.ToString()})";
                                TxtMacBusy.Text = "Timeout; No customer response.";
                                TxtInProgress.Text = "Timeout; No customer response.";

                                if (_cancelButtonEnabled)
                                {
                                    DisableCancelButton();
                                }
                            }
                        }
                    }
                };

               
            }
            catch (Exception ex)
            {
            }

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _isPageLoaded = false;


            Log.Information(LogChannel, _currProcessId, "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX End of Card Payment Transaction XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", "B10", "IM30AccessSDK.StartCardTransaction");


          
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

                string errMsg = "Cancel /Stop transaction";

                this.Dispatcher.Invoke(new Action(() =>
                {
                    TxtMacBusy.Visibility = Visibility.Collapsed;
                    TxtError.Text = errMsg;
                    TxtError.Visibility = Visibility.Visible;
                }));
                count = 3;

                checkKomlinkCardTimer.Stop();
                StopTransaction();
                SetTextToEndingTransaction();

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

                }
                catch { }

                Log.Information(LogChannel,
                    _currProcessId,
                    new NssIT.Kiosk.Log.DB.WithDataException($@"OnEndPayment is not handled; (EXIT10000930); {ex.Message}", ex, new List<object>(new object[] { paymentResult, cardResponseResult })),
                    "EX01", "pgCreditCardPayWaveV2.RaiseOnEndPayment", NssIT.Kiosk.AppDecorator.Log.MessageType.Error,
                     $@"{ex.Message}; (EXIT10000930)");


            }
        }


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
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    BdCancel.Background = _deactiveButtonBackgroungColor;
                    TxtName.Foreground = _deactiveButtonForegroundColor;
                    _cancelButtonEnabled = false;
                    BdCancel.Tag = _disableTag;
                    _cancelButtonEnabled = false;
                    System.Windows.Forms.Application.DoEvents();
                });

            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {

            }
        }


        ///// CYA-DEBUG-----IM30AccessSDK below

        private void EnableCancelButton()
        {
            // App.MarkLog.CreditCard.LogMark("#pgCard.EnableCancelButton>>A01#");


            if (isIM30Ready)
            {
                try
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        BdCancel.Background = _activeButtonBackgroungColor;
                        TxtName.Foreground = _activeButtonForegroundColor;
                        BdCancel.Tag = "";
                        _cancelButtonEnabled = true;
                        System.Windows.Forms.Application.DoEvents();

                    }));
                }
                catch (Exception ex)
                {

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


        //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

        public enum TransactionCategoryV2
        {
            Sale = 0,
            Settlement = 1
        }
    }
}
