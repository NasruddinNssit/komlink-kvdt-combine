using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Constant.BTnG;
using NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx;
using NssIT.Train.Kiosk.Common.Data.PostRequestParam.BTnG;
using NssIT.Train.Kiosk.Common.Data.Response.BTnG;
using NssIT.Train.Kiosk.Common.Helper;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Train.Kiosk.Common.Data;
using NssIT.Kiosk.AppDecorator.UI;
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
using System.IO;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Tools.CountDown;
using NssIT.Kiosk.Tools.ThreadMonitor;
using Komlink.Views.Payment.PayWave;
using Komlink.Views.Base;
using Serilog;
using Komlink.Models;

namespace Komlink.Views.Payment.Ewallet
{
    /// <summary>
    /// Interaction logic for pgBTnGPayment.xaml; ClassCode:EXIT80.07
    /// </summary>
    public partial class pgBTnGPayment : Page
    {
        public event EventHandler<EndOfPaymentEventArgs> OnEndPayment;

        private string LogChannel = "BTnGPaymentUI";

        private Brush _activeButtonBackgroungColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xF4, 0x82, 0x20));
        private Brush _activeButtonForegroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

        private Brush _deactiveButtonBackgroungColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD));
        private Brush _deactiveButtonForegroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xBB, 0xBB, 0xBB));

        public (string BTnGSalesTransactionNo, string MerchantTransactionNo, decimal Amount, string SnRId, string PaymentGateway, string FinancePaymentMethod, string Currency, 
            string FirstName, string LastName, string ContactNo,
            bool? IsSuccess) _saleTrans =
            (BTnGSalesTransactionNo: null, MerchantTransactionNo: null, Amount: 0.0M, SnRId: null, PaymentGateway: null, FinancePaymentMethod: null, Currency: null, 
            FirstName: null, LastName: null, ContactNo: null,
            IsSuccess: null);

        private string _disableTag = "DISABLED";
        private bool _isTransactionEnd = false;
        private object _transEndLock = new object();
        private CountDownTimer _endTransCountDown = null;
        private string _paymentGatewayLogoUrl = null;

        private ResourceDictionary _languageResource = null;
        private string _currProcessId = "-";

        private WebImageCacheX _imageCache = null;

        /// <summary>
        /// FuncCode:EXIT80.0701
        /// </summary>
        private pgBTnGPayment()
        {
            InitializeComponent();

            _endTransCountDown = new CountDownTimer();
            _endTransCountDown.OnCountDown += _endTransCountDown_OnCountDown;
            _endTransCountDown.OnExpired += _endTransCountDown_OnExpired;

            _imageCache = new WebImageCacheX(12);
        }

        private RunThreadMan _createNewSaleThreadWorker = null;

        /// <summary>
        /// FuncCode:EXIT80.0702
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //App.TimeoutManager.ResetTimeout();

            _imageCache.ClearCacheOnTimeout();

            App.IsAutoTimeoutExtension = true;
            _endTransCountDown.ResetCounter();

            if (_languageResource != null)
            {
                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(_languageResource);
            }

            TxtMacBusy.Visibility = Visibility.Collapsed;
            TxtError.Visibility = Visibility.Collapsed;
            TxtError.Text = "";

            TxtPayAmount.Text = $@"{_saleTrans.Currency} {_saleTrans.Amount:#,###.00}";
            TxtPayBalance.Text = $@"{_saleTrans.Currency} {_saleTrans.Amount:#,###.00}";
            TxtBTnGSaleTransNo.Text = "";

            TxtMacBusy.Text = "Memohon pembayaran eWallet .. / Request eWallet payment ..";
            TxtInProgress.Text = "Request eWallet payment ..";

            TxtTimer.Text = $@"..";
            imgBTnG.Source = null;
            UpdateImage(_paymentGatewayLogoUrl, imgBTnG);

            Reset2DBarcodeArea();
            DisableCancelButton();

            Bd2DBarcodeLoading.Visibility = Visibility.Visible;

            _endTransCountDown.ChangeCountDown("ENDING", 90, 500);
            _isTransactionEnd = false;

            Log.Information(LogChannel, _currProcessId, $@"Start - {_saleTrans.PaymentGateway} payment; Customer Name: {_saleTrans.FirstName}", 
                "B01", "pgBTnGPayment.Page_Loaded",  $@"========================= Start sWallet - {_saleTrans.PaymentGateway} payment; Customer Name: {_saleTrans.FirstName} =========================");

            if ((_createNewSaleThreadWorker != null) && (_createNewSaleThreadWorker.IsEnd == false))
                _createNewSaleThreadWorker.AbortRequest(out _, 3000);

            _createNewSaleThreadWorker = new RunThreadMan(new ThreadStart(CreateNewSaleThreadWorking), "pgBTnGPayment.Page_Loaded", 60, LogChannel);

            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            /// <summary>
            /// FuncCode:EXIT80.078A
            /// </summary>
            void CreateNewSaleThreadWorking()
            {
                try
                {
                    if ((App.NetClientSvc.BTnGService.MakeNewPaymentRequest(
                        _saleTrans.MerchantTransactionNo, _saleTrans.Amount, _saleTrans.PaymentGateway, _saleTrans.Currency,
                        _saleTrans.FirstName, _saleTrans.LastName, _saleTrans.ContactNo, _saleTrans.FinancePaymentMethod,
                        out bool isServerResponded, 50) == true)
                        )
                    {
                        App.ShowDebugMsg("BTnG Payment should be started now ..");
                    }
                    else
                        throw new Exception("Unable to start 'Boost/Touch n Go'; (EXIT80.078A.X01)");
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg(ex.ToString());
                    
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        _isTransactionEnd = true;

                        TxtMacBusy.Text = "-";
                        TxtMacBusy.Visibility = Visibility.Collapsed;
                        TxtError.Text = $@"{ex.Message}; (EXIT80.078A.EX01)";
                        TxtError.Visibility = Visibility.Visible;
                        System.Windows.Forms.Application.DoEvents();
                    }));

                    _endTransCountDown.ChangeCountDown("ENDING", 10, 700);
                }
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _endTransCountDown.ResetCounter();

            App.IsAutoTimeoutExtension = false;
        }

        private static Lazy<pgBTnGPayment> _bTnGPaymentPage = new Lazy<pgBTnGPayment>(() => new pgBTnGPayment());
        public static pgBTnGPayment GetBTnGPage()
        {
            return _bTnGPaymentPage.Value;
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

        public void InitPaymentData(string currency, decimal amount, string refNo, string paymentGateway, string firstName, string lastName, string contactNo,
            string paymentGatewayLogoUrl, string financePaymentMethod,
            ResourceDictionary languageResource)
        {
            ResetPaymentTransData();

            _currProcessId = refNo;
            _saleTrans.MerchantTransactionNo = refNo;
            _saleTrans.Currency = currency;
            _saleTrans.Amount = amount;
            _saleTrans.PaymentGateway = paymentGateway;
            _saleTrans.FinancePaymentMethod = financePaymentMethod;
            _saleTrans.FirstName = firstName;
            _saleTrans.LastName = lastName;
            _saleTrans.ContactNo = contactNo;
            _paymentGatewayLogoUrl = paymentGatewayLogoUrl;
            _languageResource = languageResource;
        }

        /// <summary>
        /// FuncCode:EXIT80.0705
        /// </summary>
        private void Cancel_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                App.ShowDebugMsg($@"CancelSale_Click .. ..");

                if (SetEndTransaction(out _))
                {
                    Reset2DBarcodeArea();

                    _saleTrans.IsSuccess = false;

                    _endTransCountDown.ChangeCountDown("ENDING", 30, 500);

                    Log.Information(LogChannel, _currProcessId, $@"Customer cancel transaction",
                        "B01", "pgBTnGPayment.Cancel_Click", $@"Customer cancel transaction");

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        BdFinalFail.Visibility = Visibility.Visible;
                        TxtFinalFail.Text = _languageResource?["QR_CANCELED_Label"]?.ToString() ?? "Canceled";
                        //TxtMacBusy.Text = "";
                        //TxtMacBusy.Visibility = Visibility.Visible;
                        TxtInProgress.Text = "Payment Transaction has been canceled";
                        System.Windows.Forms.Application.DoEvents();
                    }));

                    App.NetClientSvc.BTnGService.CancelRefundPaymentRequest(
                        _saleTrans.MerchantTransactionNo, _saleTrans.BTnGSalesTransactionNo,
                        _saleTrans.MerchantTransactionNo, _saleTrans.Currency,
                        _saleTrans.PaymentGateway, _saleTrans.Amount);

                    DisableCancelButton();

                    TxtTimer.Text = "END";
                    EndSale();
                }
                else
                    App.ShowDebugMsg($@"Unable to cancel sale. Sale has paid successfully");

                App.ShowDebugMsg($@"pgBTnGPayment.Cancel_Click : Click .. #");
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"Error when receiving Payment Request Result. {ex.ToString()}");
            }
            return;
        }

        private void Test_SubmitPayment(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }


        private bool _isEndPayment = false;
        /// <summary>
        /// FuncCode:EXIT80.0707
        /// </summary>
        private void _endTransCountDown_OnExpired(object sender, ExpiredEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                TxtTimer.Text = "(0)";
            }));

            //Quit Payment
            if ((_saleTrans.IsSuccess.HasValue) && (_saleTrans.IsSuccess.Value == true))
                RaiseOnEndPayment(PaymentResult.Success, _saleTrans.BTnGSalesTransactionNo, _saleTrans.FinancePaymentMethod);

            else
                RaiseOnEndPayment(PaymentResult.Fail, _saleTrans.BTnGSalesTransactionNo, _saleTrans.FinancePaymentMethod);

            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            /// <summary>
            /// FuncCode:EXIT80.078B
            /// </summary>
            void RaiseOnEndPayment(NssIT.Kiosk.AppDecorator.Common.PaymentResult paymentResult, string bTnGSaleTransactionNo, string paymentMethod)
            {
                Log.Information(LogChannel, _currProcessId, $@"End Transaction; Payment Gateway Sale No : {bTnGSaleTransactionNo}; Payment Status : {paymentResult.ToString()}",
                    "D01", "pgBTnGPayment._endTransCountDown_OnExpired", $@"End Transaction; Payment Gateway Sale No : {bTnGSaleTransactionNo}; Payment Status : {paymentResult.ToString()}");

                if (_isEndPayment)
                    return;

                try
                {
                    _isEndPayment = true;

                    OnEndPayment?.Invoke(null, new EndOfPaymentEventArgs(_currProcessId, paymentResult, bTnGSaleTransactionNo, paymentMethod));
                }
                catch (Exception ex)
                {
                    Log.Information(LogChannel,
                        _currProcessId, "OnEndPayment is not handled; (EXIT80.078B.EX01)", "EX01",
                        "pgCreditCardPayWave.RaiseOnEndPayment", NssIT.Kiosk.AppDecorator.Log.MessageType.Error, $@"End Transaction; Error: {ex.Message}");
                    //App.MainScreenControl.Alert(detailMsg: $@"OnEndPayment is not handled; (EXIT80.078B.EX01)");
                }
            }
        }

        private void _endTransCountDown_OnCountDown(object sender, CountDownEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                TxtTimer.Text = $@"({e.TimeRemainderSec.ToString()})";
            }));
        }

        /// <summary>
        /// FuncCode:EXIT80.0709
        /// </summary>
        public void BTnGShowPaymentInfo(IKioskMsg kioskMsg)
        {
            try
            {
                var payData = kioskMsg.GetMsgData();
                if (payData is UIxBTnGPaymentNewTransStartedAck newTrans)
                {
                    if (_isTransactionEnd == false)
                    {
                        if (_endTransCountDown.Activated)
                            _endTransCountDown.ResetCounter();

                        _saleTrans.Amount = newTrans.Amount;
                        _saleTrans.MerchantTransactionNo = newTrans.MerchantTransactionNo;
                        _saleTrans.BTnGSalesTransactionNo = newTrans.BTnGSalesTransactionNo;
                      
                        _saleTrans.SnRId = newTrans.SnRId;


                        UserSession.PaymentGateWaySalesTransactionNo = newTrans.BTnGSalesTransactionNo;
                        Reset2DBarcodeArea();

                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            TxtBTnGSaleTransNo.Text = newTrans.BTnGSalesTransactionNo;

                            Bd2DBarcode.Visibility = Visibility.Visible;
                            EnableCancelButton();
                            ShowBarCode(newTrans.Base64ImageQrCode);
                            TxtInProgress.Text = "Waiting for 2D Barcode scanning ..";
                        }));

                        //App.Log.LogText(LogChannel, _currProcessId, $@"Show Barcode; Payment Gateway Sale No: {_saleTrans.BTnGSalesTransactionNo}",
                        //    "B01", "pgBTnGPayment.BTnGShowPaymentInfo", adminMsg: $@"Show Barcode; Payment Gateway Sale No: {_saleTrans.BTnGSalesTransactionNo} ");

                        _isTransactionEnd = false;
                    }
                }
                else if (payData is UIxBTnGPaymentCountDownAck cntD)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (_isTransactionEnd == false)
                        {
                            if ((_endTransCountDown.Activated) && (cntD.CountDown > 0))
                                _endTransCountDown.ResetCounter();

                            TxtTimer.Text = $@"({cntD.CountDown.ToString()})";
                        }
                    }));
                }
                else if (payData is UIxBTnGPaymentCustomerMsgAck custMsg)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (custMsg.Message != null)
                            TxtMacBusy.Text = custMsg.Message.Trim().Replace("PleaseScan2D;", "");
                    }));
                }
                else if (payData is UIxBTnGPaymentInProgressMsgAck prgMsg)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if ((prgMsg.Message?.Contains("SignalREchoMessage;") == true)
                            || (prgMsg.Message?.Contains("ReadServerTime;") == true))
                        {
                            if (prgMsg.Message != null)
                                App.ShowDebugMsg(prgMsg.Message.Trim());
                        }
                        else
                            if (prgMsg.Message != null)
                                TxtInProgress.Text = prgMsg.Message.Trim().Replace("PleaseScan2D;", "");

                        if (prgMsg.IsCancelAllowed.HasValue) { }
                    }));
                }
                else if (payData is UIxBTnGPaymentErrorAck errMsg)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (errMsg.ErrorMsg != null)
                        {
                            TxtError.Visibility = Visibility.Visible;
                            TxtError.Text = errMsg.ErrorMsg.Trim();
                        }
                        TxtInProgress.Text = "Error found ..";
                    }));
                }
                else if (payData is UIxBTnGPaymentEndAck endMsg)
                {

                    //App.Log.LogText(LogChannel, _currProcessId, endMsg, "D01", "pgBTnGPayment.BTnGShowPaymentInfo", extraMsg: "Ending; MsgObj: UIxBTnGPaymentEndAck");

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        EndSale();

                        if (endMsg.ResultState == PaymentResult.Success)
                        {
                            /////CYA-TEST ---------------------------------------------------------------------------------------
                            ////Task.Factory.StartNew(new Action(() => 
                            ////{
                            ////    Thread.Sleep(5 * 1000);
                            ////    if (SetEndTransaction(out _))
                            ////    {
                            ////        _endTransCountDown.ChangeCountDown("ENDING", 3, 700);
                            ////        this.Dispatcher.Invoke(new Action(() => 
                            ////        {
                            ////            TxtInProgressMsg.Text = "Payment Success";
                            ////            TxtMessage.Text = "Payment Success";
                            ////        }));
                            ////    }
                            ////    else
                            ////        _msg.ShowMessage($@"Unable to set sale to success since transaction has already end");
                            ////}));
                            /////--------------------------------------------------------------------------------------------------

                            if (SetEndTransaction(out _))
                            {
                                Reset2DBarcodeArea();

                                BdPaidSuccess.Visibility = Visibility.Visible;
                                _endTransCountDown.ChangeCountDown("ENDING", 3, 700);
                                TxtInProgress.Text = "Payment Success";
                                TxtMacBusy.Text = "Payment Success";
                                _saleTrans.IsSuccess = true;

                                //App.Log.LogText(LogChannel, _currProcessId, $@"Paid Successuful",
                                //    "E01", "pgBTnGPayment.BTnGShowPaymentInfo", adminMsg: $@"Paid Successuful");
                            }
                            else
                            {
                                App.ShowDebugMsg($@"BTnG Payment : Unable to set sale to success since transaction has already end");

                                //App.Log.LogText(LogChannel, _currProcessId, $@"Customer has paid; Transaction may has issue; (EXIT80.0709.E02)",
                                //    "E02", "pgBTnGPayment.BTnGShowPaymentInfo", adminMsg: $@"Customer has paid; Transaction may has issue; (EXIT80.0709.E02)");
                            }
                        }
                        else if (endMsg.ResultState == PaymentResult.Cancel)
                        {
                            if (SetEndTransaction(out _))
                            {
                                Reset2DBarcodeArea();
                                BdFinalFail.Visibility = Visibility.Visible;
                                TxtFinalFail.Text = _languageResource?["QR_CANCELED_Label"]?.ToString() ?? "Canceled";
                                TxtInProgress.Text = "Payment Transaction has been canceled";
                                TxtMacBusy.Text = "";
                                _saleTrans.IsSuccess = false;

                                _endTransCountDown.ChangeCountDown("ENDING", 3, 700);

                                //App.Log.LogText(LogChannel, _currProcessId, $@"Cancel Confirmed",
                                //    "E03", "pgBTnGPayment.BTnGShowPaymentInfo", adminMsg: $@"Cancel Confirmed");
                            }
                            else
                                _endTransCountDown.ChangeCountDown("ENDING", 3, 700);
                        }
                        else
                        {
                            _endTransCountDown.ChangeCountDown("ENDING", 10, 700);

                            if (SetEndTransaction(out _))
                            {
                                //////////if (string.IsNullOrWhiteSpace(_saleTrans.SalesTransactionNo) == false)
                                //////////{
                                //////////    App.NetClientSvc.BTnGService.CancelRefundPaymentRequest(
                                //////////        _saleTrans.MerchantTransactionNo, _saleTrans.SalesTransactionNo,
                                //////////        _saleTrans.MerchantTransactionNo, _saleTrans.Currency,
                                //////////        _saleTrans.PaymentGateway, _saleTrans.Amount);
                                //////////}

                                _saleTrans.IsSuccess = false;

                                string endMessage = (endMsg.ErrorMsg ?? "").Trim();

                                if (string.IsNullOrWhiteSpace(endMessage) == false)
                                    endMessage = endMsg.Message ?? "";

                                if (endMsg.ResultState == PaymentResult.Timeout)
                                {
                                    Reset2DBarcodeArea();
                                    BdFinalFail.Visibility = Visibility.Visible;
                                    TxtFinalFail.Text = _languageResource?["QR_TIMEOUT_Label"]?.ToString() ?? "Timeout";

                                    TxtInProgress.Text = $@"Timeout; Fail Payment Transaction; {endMessage}";
                                    TxtMacBusy.Text = "";

                                    //App.Log.LogText(LogChannel, _currProcessId, $@"Transaction Timeout; {endMessage}",
                                    //    "E04", "pgBTnGPayment.BTnGShowPaymentInfo", adminMsg: $@"Transaction Timeout; {endMessage}");
                                }
                                else if (endMsg.ResultState == PaymentResult.Fail)
                                {
                                    Reset2DBarcodeArea();
                                    BdFinalFail.Visibility = Visibility.Visible;
                                    TxtFinalFail.Text = _languageResource?["QR_FAIL_Label"]?.ToString() ?? "Fail";

                                    TxtInProgress.Text = $@"Fail Payment Transaction; {endMessage}";
                                    TxtMacBusy.Text = "";

                                    //App.Log.LogText(LogChannel, _currProcessId, $@"Transaction Failed; {endMessage}; (EXIT80.0709.E05)",
                                    //    "E05", "pgBTnGPayment.BTnGShowPaymentInfo", adminMsg: $@"Transaction Failed; {endMessage}; (EXIT80.0709.E05)");
                                }
                                else
                                {
                                    Reset2DBarcodeArea();
                                    BdFinalFail.Visibility = Visibility.Visible;
                                    TxtFinalFail.Text = _languageResource?["QR_FAIL_Label"]?.ToString() ?? "Fail"; ;

                                    TxtInProgress.Text = $@"Fail Payment Transaction with unknown error; {endMessage}";
                                    TxtMacBusy.Text = "";

                                    //App.Log.LogText(LogChannel, _currProcessId, $@"Transaction Failed with unknown error; {endMessage} (EXIT80.0709.E06)",
                                    //    "E06", "pgBTnGPayment.BTnGShowPaymentInfo", adminMsg: $@"Transaction Failed with unknown error; {endMessage} (EXIT80.0709.E06)");
                                }
                            }
                        }
                        TxtTimer.Text = "END";
                    }));
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"MainWindow:BTnGShowPaymentInfo => Error : {ex.ToString()}");

                //App.Log.LogError(LogChannel, _currProcessId, new WithDataException(ex.Message, ex, kioskMsg), 
                //    "EX01", "pgBTnGPayment.BTnGShowPaymentInfo", adminMsg: $@"{ex.Message}");
            }

            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void ShowBarCode(string base64String)
            {
                byte[] imgBytes = Convert.FromBase64String(base64String);

                // Image object 
                //this.Dispatcher.Invoke(new Action(() => {
                BitmapImage bitmapImage = new BitmapImage();
                MemoryStream ms = new MemoryStream(imgBytes);
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();

                imgBarcode.Source = bitmapImage;
                System.Windows.Forms.Application.DoEvents();
                //}));
                //----------------------------------------------------------
            }
        }

        private void Reset2DBarcodeArea()
        {
            this.Dispatcher.Invoke(new Action(() => 
            {
                Bd2DBarcode.Visibility = Visibility.Collapsed;
                Bd2DBarcodeLoading.Visibility = Visibility.Collapsed;
                BdPaidSuccess.Visibility = Visibility.Collapsed;
                BdFinalFail.Visibility = Visibility.Collapsed;
                TxtFinalFail.Text = "";
            }));
        }
                      

        /// <summary>
        /// Return true when success set the _isTransactionEnd flag. Else return false.; FuncCode:EXIT80.0711
        /// </summary>
        /// <param name="transHasBeenEnded">Return true when _isTransactionEnd has already set to true in previous stage</param>
        /// <returns></returns>
        private bool SetEndTransaction(out bool transHasBeenEnded)
        {
            transHasBeenEnded = true;

            bool retResult = false;
            bool transHasBeenEndedX = _isTransactionEnd;

            if (_isTransactionEnd == false)
            {
                Thread tWorker = new Thread(new ThreadStart(new Action(() =>
                {
                    lock (_transEndLock)
                    {
                        transHasBeenEndedX = _isTransactionEnd;

                        if (_isTransactionEnd == false)
                        {
                            _isTransactionEnd = true;
                            retResult = true;
                        }
                    }
                })));
                tWorker.IsBackground = true;
                tWorker.Start();
                tWorker.Join();
            }

            transHasBeenEnded = transHasBeenEndedX;
            return retResult;
        }

        private void ResetPaymentTransData()
        {
            _isEndPayment = false;
            _saleTrans.MerchantTransactionNo = null;
            _saleTrans.PaymentGateway = null;
            _saleTrans.Amount = 0.0M;
            _saleTrans.Currency = null;
            _saleTrans.PaymentGateway = null;
            _saleTrans.FinancePaymentMethod = null;
            _saleTrans.FirstName = null;
            _saleTrans.LastName = null;
            _saleTrans.ContactNo = null;
            _saleTrans.BTnGSalesTransactionNo = null;
            _saleTrans.SnRId = null;
            _saleTrans.IsSuccess = null;
        }

        private void EndSale()
        {
            try
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    DisableCancelButton();
                    imgBarcode.Source = null;
                    System.Windows.Forms.Application.DoEvents();
                }));

                App.ShowDebugMsg($@".. End BTnG Payment");
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"pgBTnGPayment.EndSale => Error:{ex.ToString()} ");
            }
        }

        private bool _cancelButtonEnabled = false;
        private void DisableCancelButton()
        {
            this.Dispatcher.Invoke(new Action(() => {
                BdCancel.Background = _deactiveButtonBackgroungColor;
                TxtName.Foreground = _deactiveButtonForegroundColor;
                _cancelButtonEnabled = false;
                BdCancel.Tag = _disableTag;
                System.Windows.Forms.Application.DoEvents();
            }));
        }

        private Thread _delayCancelActivationThreadWorker = null;
        private void EnableCancelButton()
        {
            _cancelButtonEnabled = true;

            try
            {
                if (_cancelButtonEnabled)
                {
                    this.Dispatcher.Invoke(new Action(() => {
                        BdCancel.Background = _activeButtonBackgroungColor;
                        TxtName.Foreground = _activeButtonForegroundColor;
                        BdCancel.Tag = "";
                        System.Windows.Forms.Application.DoEvents();
                    }));
                }
            }
            catch (Exception ex)
            {
                //App.Log.LogText(LogChannel, _currProcessId, ex, "EX01",
                //    "pgCreditCardPayWave.EnableCancelButton", AppDecorator.Log.MessageType.Error);
            }

            return;
        }

        private async void UpdateImage(string url, Image img)
        {
            try
            {
                BitmapImage bitImp = await _imageCache.GetImage(url);
                this.Dispatcher.Invoke(new Action(() =>
                {
                    imgBTnG.Source = bitImp;
                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                //App.Log.LogError(LogChannel, "*", ex, "EX01", "pgBTnGPayment.UpdateImage");
            }
        }
    }
}
