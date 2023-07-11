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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
using NssIT.Kiosk.Tools.CountDown;
using NssIT.Kiosk.Tools.ThreadMonitor;
using System.Windows.Interop;

namespace WpfBoostTouchNGoClient02
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LibShowMessageWindow.MessageWindow _msg = null;

        //public (string SalesTransactionNo, string MerchantTransactionNo, decimal Amount, string SnRId, string PaymentGateway, string Currency, bool? IsSuccess) _saleTrans =
        //    (SalesTransactionNo: null, MerchantTransactionNo: null, Amount: 0.0M, SnRId: null, PaymentGateway: null, Currency: null, IsSuccess: null);

        public (string BTnGSalesTransactionNo, string MerchantTransactionNo, decimal Amount, string SnRId, string PaymentGateway, string FinancePaymentMethod, string Currency,
            string FirstName, string LastName, string ContactNo,
            bool? IsSuccess) _saleTrans =
            (BTnGSalesTransactionNo: null, MerchantTransactionNo: null, Amount: 0.0M, SnRId: null, PaymentGateway: null, FinancePaymentMethod: null, Currency: null,
            FirstName: null, LastName: null, ContactNo: null,
            IsSuccess: null);

        private bool _isTransactionEnd = false;
        private object _transEndLock = new object();
        private CountDownTimer _endTransCountDown = null;

        private BTnGPaymentGatewayDetailModel[] PaymentGatewayList { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;

            _endTransCountDown = new CountDownTimer();
            _endTransCountDown.OnCountDown += _endTransCountDown_OnCountDown;
            _endTransCountDown.OnExpired += _endTransCountDown_OnExpired;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtDocNo.Text = $@"MyDocNo{DateTime.Now:yyMMdd-HHmm}";
        }

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
            _saleTrans.MerchantTransactionNo = null;
            _saleTrans.PaymentGateway = null;
            _saleTrans.Amount = 0.0M;
            _saleTrans.Currency = null;
            _saleTrans.FinancePaymentMethod = null;
            _saleTrans.FirstName = null;
            _saleTrans.LastName = null;
            _saleTrans.ContactNo = null;
            _saleTrans.BTnGSalesTransactionNo = null;
            _saleTrans.SnRId = null;
            _saleTrans.IsSuccess = null;

            _saleTrans.MerchantTransactionNo = null;
            _saleTrans.PaymentGateway = null;
            _saleTrans.Amount = 0.0M;
            _saleTrans.Currency = null;
            _saleTrans.FinancePaymentMethod = null;
            _saleTrans.FirstName = null;
            _saleTrans.LastName = null;
            _saleTrans.ContactNo = null;
            _saleTrans.BTnGSalesTransactionNo = null;
            _saleTrans.SnRId = null;
            _saleTrans.IsSuccess = null;

            this.Dispatcher.Invoke(new Action(() => {
                TxtBTnGSaleTransNo.Text = "";
            }));
        }

        private void _endTransCountDown_OnExpired(object sender, ExpiredEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                TxtCountDown.Text = "0";
            }));

            //Quit Payment
        }

        private void _endTransCountDown_OnCountDown(object sender, CountDownEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                TxtCountDown.Text = e.TimeRemainderSec.ToString();
            }));
        }

        public void BTnGShowPaymentInfo(IKioskMsg kioskMsg)
        {
            try
            {
                var payData = kioskMsg.GetMsgData();
                if (payData is UIxBTnGPaymentNewTransStartedAck newTrans)
                {
                    _saleTrans.Amount = newTrans.Amount;
                    _saleTrans.MerchantTransactionNo = newTrans.MerchantTransactionNo;
                    _saleTrans.BTnGSalesTransactionNo = newTrans.BTnGSalesTransactionNo;
                    _saleTrans.SnRId = newTrans.SnRId;

                    if (_endTransCountDown.Activated)
                        _endTransCountDown.ResetCounter();

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        BtnNewSale.IsEnabled = false;
                        BtnCancelSale.IsEnabled = true;
                        TxtBTnGSaleTransNo.Text = newTrans.BTnGSalesTransactionNo;

                        ShowBarCode(newTrans.Base64ImageQrCode);

                        TxtSnRClientId.Text = (_saleTrans.SnRId ?? "").Trim();

                        TxtInProgressMsg.Text = "Waiting for 2D Barcode scanning";
                    }));

                    //_countDown?.ChangeCountDown(_countDownCode_WaitForScanning, 75, 500);
                }
                else if (payData is UIxBTnGPaymentCountDownAck cntD)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (_isTransactionEnd == false)
                        {
                            if ((_endTransCountDown.Activated) && (cntD.CountDown > 0))
                                _endTransCountDown.ResetCounter();

                            TxtCountDown.Text = cntD.CountDown.ToString();
                        }
                    }));
                }
                else if (payData is UIxBTnGPaymentCustomerMsgAck custMsg)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (custMsg.Message != null)
                            TxtMessage.Text = custMsg.Message.Trim();
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
                                _msg.ShowMessage(prgMsg.Message.Trim());
                        }
                        else
                            if (prgMsg.Message != null)
                            TxtInProgressMsg.Text = prgMsg.Message.Trim();

                        if (prgMsg.IsCancelAllowed.HasValue) { }
                    }));
                }
                else if (payData is UIxBTnGPaymentErrorAck errMsg)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (errMsg.ErrorMsg != null)
                            TxtError.Text = errMsg.ErrorMsg.Trim();

                        TxtInProgressMsg.Text = "Error found ..";
                    }));
                }
                else if (payData is UIxBTnGPaymentEndAck endMsg)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        EndSale();

                        if (endMsg.ResultState == PaymentResult.Success)
                        {
                            /////CYA-DEBUG ---------------------------------------------------------------------------------------
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
                                _endTransCountDown.ChangeCountDown("ENDING", 3, 700);
                                TxtInProgressMsg.Text = "Payment Success";
                                TxtMessage.Text = "Payment Success";
                                _saleTrans.IsSuccess = true;
                            }
                            else
                                _msg.ShowMessage($@"Unable to set sale to success since transaction has already end");
                        }
                        else if (endMsg.ResultState == PaymentResult.Cancel)
                        {
                            if (SetEndTransaction(out _))
                            {
                                TxtInProgressMsg.Text = "Payment Transaction has been canceled";
                                TxtMessage.Text = "Transaction canceled";
                                _saleTrans.IsSuccess = false;

                                _endTransCountDown.ChangeCountDown("ENDING", 7, 700);
                            }
                            else
                                _endTransCountDown.ChangeCountDown("ENDING", 10, 700);
                        }
                        else
                        {
                            _endTransCountDown.ChangeCountDown("ENDING", 10, 700);

                            if (SetEndTransaction(out _))
                            {
                                //if (string.IsNullOrWhiteSpace(_saleTrans.SalesTransactionNo) == false)
                                //{
                                //    App.NetClientSvc.BTnGService.CancelRefundPaymentRequest(
                                //        _saleTrans.MerchantTransactionNo, _saleTrans.SalesTransactionNo,
                                //        _saleTrans.MerchantTransactionNo, _saleTrans.Currency,
                                //        _saleTrans.PaymentGateway, _saleTrans.Amount);
                                //}

                                _saleTrans.IsSuccess = false;

                                string endMessage = (endMsg.ErrorMsg ?? "").Trim();

                                if (string.IsNullOrWhiteSpace(endMessage) == false)
                                    endMessage = endMsg.Message ?? "";

                                if (endMsg.ResultState == PaymentResult.Timeout)
                                {
                                    TxtInProgressMsg.Text = $@"Timeout; Fail Payment Transaction; {endMessage}";

                                    TxtMessage.Text = "Timeout";
                                }
                                else if (endMsg.ResultState == PaymentResult.Fail)
                                {
                                    TxtInProgressMsg.Text = $@"Fail Payment Transaction; {endMessage}";
                                    TxtMessage.Text = "Fail Transaction";
                                }
                                else
                                {
                                    TxtInProgressMsg.Text = $@"Fail Payment Transaction with unknown error; {endMessage}";
                                    TxtMessage.Text = "Fail Transaction (II)";
                                }
                            }
                        }
                        TxtCountDown.Text = "END";
                    }));
                }
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"MainWindow:BTnGShowPaymentInfo => Error : {ex.ToString()}");
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

        private RunThreadMan _getAllServiceThreadWorker = null;
        private void GetAllService_Click(object sender, RoutedEventArgs e)
        {
            if ((_getAllServiceThreadWorker != null) && (_getAllServiceThreadWorker.IsEnd == false))
                _getAllServiceThreadWorker.AbortRequest(out _, 3000);

            CboPaymentService.Items.Clear();
            //(IGNORED)..... CboPaymentServiceX.Items.Clear();

            _getAllServiceThreadWorker = new RunThreadMan(new ThreadStart(GetAllServiceThreadWorker), "MainWindow.GetAllService_Click", 90, "WpfBoostTouchNGoClient");

            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void GetAllServiceThreadWorker()
            {
                try
                {
                    _msg.ShowMessage("");
                    bool dataFound = false;

                    if (App.NetClientSvc.BTnGService.QueryAllPaymentGateway(out BTnGGetPaymentGatewayResult payGateResult, out bool isServerResponded) == true)
                    {
                        PaymentGatewayList = payGateResult.Data.PaymentGatewayList;

                        this.Dispatcher.Invoke(new Action(() => {
                            foreach (BTnGPaymentGatewayDetailModel payGate in payGateResult.Data.PaymentGatewayList)
                            {
                                dataFound = true;
                                CboPaymentService.Items.Add(new ComboBoxItem() { Content = $@"{payGate.PaymentGatewayName} - {payGate.PaymentGateway}", Uid = payGate.PaymentGateway });
                                //(IGNORED)..... CboPaymentServiceX.Items.Add(new ComboBoxItem() { Content = $@"{payGate.PaymentGatewayName} - {payGate.PaymentGateway}", Uid = payGate.PaymentGateway });
                            }
                        }));
                    }

                    if (dataFound)
                        _msg.ShowMessage("GetAllServiceThreadWorker : Payment Gateway List found");
                    else
                        _msg.ShowMessage("GetAllServiceThreadWorker : No Payment Gateway has found");
                }
                catch (ThreadAbortException ex2)
                {
                    _msg.ShowMessage("GetAllServiceThreadWorker : Query Aborted");
                }
                catch (Exception ex)
                {
                    _msg.ShowMessage(ex.ToString());
                }
                finally
                {
                    _msg.ShowMessage("");
                }
            }
        }

        private void CancelSale_Click(object sender, RoutedEventArgs e)
        {
            _msg.ShowMessage("");
            try
            {
                _msg.ShowMessage($@"CancelSale_Click .. ..");

                if (SetEndTransaction(out _))
                {
                    _saleTrans.IsSuccess = false;

                    _endTransCountDown.ChangeCountDown("ENDING", 30, 700);

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        TxtMessage.Text = "CANCELED";
                        TxtMessage.Visibility = Visibility.Visible;
                        TxtInProgressMsg.Text = "Payment Transaction has been canceled";
                        System.Windows.Forms.Application.DoEvents();
                    }));

                    App.NetClientSvc.BTnGService.CancelRefundPaymentRequest(
                        _saleTrans.MerchantTransactionNo, _saleTrans.BTnGSalesTransactionNo,
                        _saleTrans.MerchantTransactionNo, _saleTrans.Currency,
                        _saleTrans.PaymentGateway, _saleTrans.Amount);

                    TxtCountDown.Text = "END";
                    EndSale();
                }
                else
                    _msg.ShowMessage($@"Unable to cancel sale. Sale has paid successfully");
                //_countDown?.ChangeCountDown(_countDownCode_WaitForCancelEnd, 3, 500);

                _msg.ShowMessage($@"CancelSale_Click .. #");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"Error when Cancel Sale Transaction. {ex.ToString()}");
            }
            return;
        }

        private void EndSale()
        {
            try
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    BtnNewSale.IsEnabled = true;
                    //BtnCancelSale.IsEnabled = false;

                    imgBarcode.Source = null;
                    System.Windows.Forms.Application.DoEvents();
                }));

                _msg.ShowMessage($@".. End Payment");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"{ex.ToString()} ");
            }
        }

        private RunThreadMan _createNewSaleThreadWorker = null;
        private void CreateNewSale_Click(object sender, RoutedEventArgs e)
        {
            _isTransactionEnd = false;
            _endTransCountDown.ResetCounter();

            TxtCountDown.Text = "..";
            _endTransCountDown.ChangeCountDown("ENDING", 90, 700);

            if ((_createNewSaleThreadWorker != null) && (_createNewSaleThreadWorker.IsEnd == false))
                _createNewSaleThreadWorker.AbortRequest(out _, 3000);

            _createNewSaleThreadWorker = new RunThreadMan(new ThreadStart(CreateNewSaleThreadWorking), "MainWindow.CreateNewSale_Click", 60, "WpfBoostTouchNGoClient");

            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void CreateNewSaleThreadWorking()
            {
                string docNo = null;
                decimal amount = 0.0M;
                string payGate = null;
                string currency = "MYR";
                string firstName = null;
                string lastName = null;
                string contactNo = null;

                _msg.ShowMessage("");
                try
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (CboPaymentService.SelectedItem is ComboBoxItem cItm)
                        {
                            /*By Pass*/
                        }
                        else
                            throw new Exception("Please select a Payment Service");

                        _msg.ShowMessage("Payment Start ..");

                        docNo = TxtDocNo.Text.Trim();
                        amount = decimal.Parse(TxtAmount.Text);
                        payGate = cItm.Uid;
                        firstName = "Testing Chong";
                        lastName = "Testing Chong";
                        contactNo = "0171234567";
                        currency = "MYR";

                        BtnCancelSale.IsEnabled = false;
                        TxtSnRClientId.Text = "";
                        TxtInProgressMsg.Text = "Starting eWallet request ..";
                        TxtCountDown.Text = "..";

                        TxtError.Text = "";
                        TxtError.Visibility = Visibility.Collapsed;

                        TxtMessage.Visibility = Visibility.Visible;
                        TxtMessage.Text = "Request BTnG Payment..";

                        System.Windows.Forms.Application.DoEvents();
                    }));

                    ResetPaymentTransData();
                    _saleTrans.PaymentGateway = payGate;
                    _saleTrans.Currency = "MYR";

                    //public bool MakeNewPaymentRequest(
                    //    string docNo,
                    //    decimal price, string paymentGateway,
                    //    string currency, string customerFirstName,
                    //    string customerLastName, string contactNo, string financePaymentMethod,
                    //    out bool isServerResponded, int waitDelaySec = 60)

                    BTnGPaymentGatewayDetailModel gateDetl = (from gateRow in PaymentGatewayList
                                                              where gateRow.PaymentGateway.Equals(payGate)
                                                              select gateRow).FirstOrDefault();

                    if ((App.NetClientSvc.BTnGService.MakeNewPaymentRequest(
                        docNo, amount, payGate, currency,
                        firstName, lastName, contactNo, gateDetl.PaymentMethod,
                        out bool isServerResponded, 50) == true)
                        )
                    {
                        _msg.ShowMessage("Payment should be started now ..");
                    }
                    else
                        throw new Exception("Unable to start 'Boost/Touch n Go'; (EXITxx)");
                }
                catch (Exception ex)
                {
                    _msg.ShowMessage(ex.ToString());

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        _isTransactionEnd = true;
                        //TxtCountDown.Text = "FAIL";

                        TxtMessage.Text = "-";
                        TxtMessage.Visibility = Visibility.Collapsed;
                        TxtError.Text = $@"{ex.Message}; (EXITxx)";
                        TxtError.Visibility = Visibility.Visible;
                        System.Windows.Forms.Application.DoEvents();
                    }));

                    _endTransCountDown.ChangeCountDown("ENDING", 10, 700);
                }
                finally
                {
                    _msg.ShowMessage("");
                }
            }
        }
    }
}
