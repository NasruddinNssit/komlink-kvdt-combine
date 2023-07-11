using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Constant.BTnG;
using NssIT.Kiosk.Network.PaymentGatewayApp.JobApp.BTnGJob;
using NssIT.Kiosk.Network.SignalRClient.API.Base.Extension;
using NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway;
using NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Data;
using NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Data.Request;
using NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Data.Response;
using NssIT.Kiosk.Sqlite.DB.AccessDB;
using NssIT.Kiosk.Sqlite.DB.AccessDB.Works;
using NssIT.Kiosk.AppDecorator.Common.Access;
using NssIT.Train.Kiosk.Common.Data;
using NssIT.Train.Kiosk.Common.Data.PostRequestParam.BTnG;
using NssIT.Train.Kiosk.Common.Data.Response.BTnG;
using NssIT.Train.Kiosk.Common.Helper;
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
using WpfBoostTouchNGoAPITest.Data;
using WpfBoostTouchNGoAPITest.Data.Response;

namespace WpfBoostTouchNGoAPITest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LibShowMessageWindow.MessageWindow _msg = null;

        private string _eWalletWebApiBaseURL = "";
        private string _webApiURL = "";
        private string _deviceId = "*";

        private BTnGJobMan _bTnGJob = null;
        private LocalSaleInfo _lastSaleInfo = null;
        //private CountDownTimer _countDown = null;

        private string _countDownCode_InitToStart = "BTnG_CountDown_InitToStart";
        private string _countDownCode_FailToStart = "BTnG_CountDown_FailToStart";
        private string _countDownCode_WaitForScanning = "BTnG_CountDown_WaitForScanning";
        private string _countDownCode_WaitForSuccessEnd = "BTnG_CountDown_WaitForSuccessEnd";
        private string _countDownCode_WaitForCancelEnd = "BTnG_CountDown_WaitForCancelEnd";
        // _countDownCode_WaitForFailTransactionEnd : Payment Transaction finished with error found (UI or Internal error)
        private string _countDownCode_WaitForFailTransactionEnd = "BTnG_CountDown_WaitForFailTransactionEnd";
        
        public MainWindow()
        {
            InitializeComponent();

            _deviceId = RegistrySetup.GetRegistrySetting().DeviceId;

            _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;
            _eWalletWebApiBaseURL = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting().EWalletWebApiBaseURL;
            _webApiURL = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting().WebApiURL;
            _bTnGJob = BTnGJobMan.GetJob();

            TxtWebApi.Text = _webApiURL;
        }
                
        private void GetAllService_Click(object sender, RoutedEventArgs e)
        {
            HttpResponseMessage response;
            try
            {
                _msg.ShowMessage("");

                TimeSpan apiTimeOutSec = new TimeSpan(0, 0, 45);
                HttpClientHandler httpClientHandler = new HttpClientHandler();
                using (HttpClient client = new HttpClient(httpClientHandler))
                {
                    client.Timeout = apiTimeOutSec;
                    //client.BaseAddress = new Uri(@"https://gopayment-api-dev.azurewebsites.net/");
                    client.BaseAddress = new Uri(_eWalletWebApiBaseURL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Authentication
                    /////client.DefaultRequestHeaders.Add("RequestSignature", SecurityHelper.getSignature());
                    // --------------------

                    PaymentGatewayResp oRet = null;
                    response = null;
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, @"api/Merchant/list_payment_gateway");
                    string StrRequest = null;
                    //if (parameters != null)
                    //{
                    StrRequest = @"{""merchantId"": """ + RegistrySetup.GetRegistrySetting().BTnGMerchantId + @"""}";
                    request.Content = new System.Net.Http.StringContent(StrRequest, Encoding.UTF8, "application/json");
                    //}

                    response = client.SendAsync(request).ConfigureAwait(false).GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        _msg.ShowMessage("Execution successful..");

                        string resultString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        _msg.ShowMessage("Result is ..");
                        _msg.ShowMessage(resultString);

                        oRet = JsonConvert.DeserializeObject<PaymentGatewayResp>(resultString);

                        CboPaymentService.Items.Clear();

                        if (oRet?.Data?.PaymentGatewayList?.Length > 0)
                        {
                            foreach (PaymentGatewayDetail pDet in oRet.Data.PaymentGatewayList)
                            {
                                CboPaymentService.Items.Add(new ComboBoxItem() { Content = $@"{pDet.PaymentGatewayName} - {pDet.PaymentGateway}", Uid = pDet.PaymentGateway });
                            }
                            _msg.ShowMessage(JsonConvert.SerializeObject(oRet, Formatting.Indented));
                        }
                        else
                            _msg.ShowMessage("No valid Payment Gateway Service !!");
                    }
                    else
                    {
                        _msg.ShowMessage("fail execution ..");
                    }
                }
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
                _msg.ShowMessage("");
            }
        }

        private BTnGPaymentSnRClient _payClient = null;
        private void CreateNewSale_Click(object sender, RoutedEventArgs e)
        {
            _msg.ShowMessage("");
            try
            {
                _deviceId = RegistrySetup.GetRegistrySetting().DeviceId;

                if (CboPaymentService.SelectedItem is ComboBoxItem cItm)
                {
                    //By pass
                }
                else
                    throw new Exception("Please select a Payment Service");

                _lastSaleInfo = new LocalSaleInfo()
                {
                    Amount = decimal.Parse(TxtAmount.Text), 
                    Currency = "MYR", DocNo = TxtDocNo.Text.Trim(), 
                    PaymentGateway = cItm.Uid, 
                    FirstName = "Testing Chong", 
                    LastName = "Testing Chong", 
                    ContactNo = "0171234567"
                };

                _payClient = new BTnGPaymentSnRClient(_webApiURL, _deviceId, "NSSTVM01", 
                    _lastSaleInfo.DocNo, _lastSaleInfo.Amount, _lastSaleInfo.Currency, _lastSaleInfo.PaymentGateway, 
                    _lastSaleInfo.FirstName, _lastSaleInfo.LastName, _lastSaleInfo.ContactNo, "*");
                _payClient.OnPaymentRequestResult += PayClient_OnPaymentRequestResult;
                _payClient.OnPaymentCompleted += _payClient_OnPaymentCompleted;
                _payClient.OnPaymentEchoMessageReceived += _payClient_OnPaymentEchoMessageReceived;

                _msg.ShowMessage("Payment Start ..");

                BtnNewSale.IsEnabled = false;
                BtnShowSvrTime.IsEnabled = true;
                BtnShowConnectionId.IsEnabled = true;
                BtnSendEcho.IsEnabled = true;
                //BtnCancelSale.IsEnabled = true;
                BtnEndSale.IsEnabled = true;

                BtnSimulatePaidReceipt.IsEnabled = true;
                //BtnSimulateCancelRefund.IsEnabled = true;

                TxtMessage.Visibility = Visibility.Collapsed;
                TxtError.Visibility = Visibility.Collapsed;
                TxtMessage.Visibility = Visibility.Visible;

                _payClient.StartPayment();

                TxtMessage.Text = "Request BTnG Payment..";

                if (string.IsNullOrWhiteSpace(_payClient?.CurrentSnRConnectionId) == false)
                    TxtSnRClientId.Text = _payClient.CurrentSnRConnectionId;

                //_countDown?.ChangeCountDown(_countDownCode_InitToStart, 60, 500);
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());

                //this.Dispatcher.Invoke(new Action(() =>
                //{
                TxtMessage.Text = "-";
                TxtMessage.Visibility = Visibility.Collapsed;
                TxtError.Text = $@"{ex.Message}; (EXITxx)";
                TxtError.Visibility = Visibility.Visible;
                System.Windows.Forms.Application.DoEvents();
                //}));

                //_countDown?.ChangeCountDown(_countDownCode_FailToStart, 5, 500);
                _msg.ShowMessage("");
            }
        }

        private void _payClient_OnPaymentEchoMessageReceived(object sender, NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Event.PaymentEchoMessageEventArgs e)
        {
            _msg.ShowMessage("");
            try
            {
                _msg.ShowMessage(e.EchoMessage);
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"Unknown error when showing echo message; {ex.Message}");
            }
            finally
            {
                _msg.ShowMessage("");
            }
        }

        private void _payClient_OnPaymentCompleted(object sender, NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Event.PaymentCompletedResultEventArgs e)
        {
            _msg.ShowMessage("");
            try
            {
                //CYA-DEBUG .. Log e object here..
                _msg.ShowMessage(JsonConvert.SerializeObject(e, Formatting.Indented));

                if (e.Result == NssIT.Kiosk.AppDecorator.Common.PaymentResult.Success)
                {
                    _msg.ShowMessage("Payment Success");

                    this.Dispatcher.Invoke(new Action(() => {
                        TxtMessage.Text = "Payment Success";
                    }));

                    //_countDown?.ChangeCountDown(_countDownCode_WaitForSuccessEnd, 2, 500);
                }

                else if (e.Result == NssIT.Kiosk.AppDecorator.Common.PaymentResult.Timeout)
                {
                    _msg.ShowMessage("Transaction has timeout");

                    this.Dispatcher.Invoke(new Action(() => {
                        TxtMessage.Text = "-";
                        TxtMessage.Visibility = Visibility.Collapsed;
                        TxtError.Text = "Transaction timeout";
                        TxtError.Visibility = Visibility.Visible;
                        System.Windows.Forms.Application.DoEvents();
                    }));

                    //_countDown?.ChangeCountDown(_countDownCode_WaitForScanning, 2, 500);
                }

                else if (e.Result == NssIT.Kiosk.AppDecorator.Common.PaymentResult.Cancel)
                {
                    _msg.ShowMessage("Transaction has been canceled");

                    //this.Dispatcher.Invoke(new Action(() => {
                    //    TxtMessage.Text = "Transaction has been canceled";
                    //    TxtMessage.Visibility = Visibility.Collapsed;
                    //    System.Windows.Forms.Application.DoEvents();
                    //}));

                    //_countDown.ChangeCountDown(_countDownCode_WaitToCancelQuit, 5, 500);
                }
                else if (e.Result == NssIT.Kiosk.AppDecorator.Common.PaymentResult.Fail)
                {
                    if (string.IsNullOrWhiteSpace(e.Error?.Message))
                        throw new Exception("Unknown error with fail payment result.");

                    else
                        throw new Exception(e.Error.Message);
                }
                else
                    throw new Exception($@"Unknown payment result status. Code: {e.Result}");
            }
            catch (Exception ex)
            {
                string errMsg = $@"Unknown Error on complete transaction; {ex.Message}";

                this.Dispatcher.Invoke(new Action(() => {
                    TxtMessage.Text = "-";
                    TxtMessage.Visibility = Visibility.Collapsed;
                    TxtError.Text = errMsg;
                    TxtError.Visibility = Visibility.Visible;
                    System.Windows.Forms.Application.DoEvents();
                }));

                //_countDown?.ChangeCountDown(_countDownCode_WaitForFailTransactionEnd, 9, 500);

                //CYA-DEBUG .. Need to Log here.
            }
            finally
            {
                this.Dispatcher.Invoke(new Action(() => EndSale()));
                _msg.ShowMessage("");
            }
        }

        private void ShowConnectionId_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_payClient?.CurrentSnRConnectionId) == false)
            {
                TxtSnRClientId.Text = _payClient.CurrentSnRConnectionId;

                _msg.ShowMessage($@"ShowConnectionId_Click : ConnectionId is {TxtSnRClientId.Text};");
            }
            else
                _msg.ShowMessage($@"ShowConnectionId_Click : no ConnectionId found;");
        }

        
        private void PayClient_OnPaymentRequestResult(object sender, NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Event.PaymentRequestResultEventArgs e)
        {
            _msg.ShowMessage("");
            try
            {
                if (_lastSaleInfo is null)
                    throw new Exception("Previous sale not detected");

                _msg.ShowMessage("PayClient_OnPaymentRequestResult with _lastSaleInfo object ..");
                _msg.ShowMessage(JsonConvert.SerializeObject(_lastSaleInfo, Formatting.Indented));

                MerchantSaleInfo saleInfo = e.PaymentRequestResult.Data;
                //CYA-DEBUG .. Log saleInfo obj here

                _msg.ShowMessage(JsonConvert.SerializeObject(e.PaymentRequestResult, Formatting.Indented));

                //Verify Signature
                bool isValidSignature = saleInfo.CheckSignature();

                if (isValidSignature == false)
                {
                    throw new Exception($@"Invalid Signature");
                }
                //-------------------------------------------------------------------------

                _lastSaleInfo.SalesTransactionNo = saleInfo.SalesTransactionNo;

                this.Dispatcher.Invoke(new Action(() => {
                    TxtBTnGSaleTransNo.Text = _lastSaleInfo.SalesTransactionNo;
                }));
                
                ShowBarCode(saleInfo.Base64ImageQrCode);

                DatabaseAx dbAx = DatabaseAx.GetAccess();
                using (var newPayAx = new BTnGNewPaymentAx<SuccessXEcho>(saleInfo.SalesTransactionNo, _lastSaleInfo.PaymentGateway, _lastSaleInfo.DocNo, _lastSaleInfo.Currency, saleInfo.Amount))
                {
                    using (var transResult = (BTnGNewPaymentAx<SuccessXEcho>)dbAx.ExecCommand(newPayAx, waitDelaySec: 20))
                    {
                        if (transResult.ResultStatus.IsSuccess)
                        {
                            // return transResult.SuccessEcho;
                        }
                        else if (transResult.ResultStatus.Error?.Message?.Length > 0)
                        {
                            throw transResult.ResultStatus.Error;
                        }
                        else
                        {
                            throw new Exception("Unknown error when adding new BTnG Payment record; (EXITxx)");
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(_payClient?.CurrentSnRConnectionId) == false)
                {
                    this.Dispatcher.Invoke(new Action(() => {
                        TxtSnRClientId.Text = _payClient.CurrentSnRConnectionId.Trim();
                    }));
                }

                this.Dispatcher.Invoke(new Action(() => {
                    TxtMessage.Text = "Please scan 2D barcode with your smartphone and proceed to complete payment with your smartphone's application";
                }));

                //_countDown?.ChangeCountDown(_countDownCode_WaitForScanning, 75, 500);
            }
            catch (Exception ex)
            {
                //CYA-DEBUG .. Log ex and parameter e here.
                string errMsg = $@"Error when receiving BTnG Payment Request Result. {ex.Message}; (EXITxx)";

                this.Dispatcher.Invoke(new Action(() => {
                    TxtError.Text = errMsg;
                    TxtError.Visibility = Visibility.Visible;
                    System.Windows.Forms.Application.DoEvents();
                }));

                //_countDown?.ChangeCountDown(_countDownCode_WaitForFailTransactionEnd, 5, 500);
            }
            return;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void ShowBarCode(string base64String)
            {
                byte[] imgBytes = Convert.FromBase64String(base64String);

                

                // Image object 
                this.Dispatcher.Invoke(new Action(() => {

                    BitmapImage bitmapImage = new BitmapImage();
                    MemoryStream ms = new MemoryStream(imgBytes);
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = ms;
                    bitmapImage.EndInit();

                    imgBarcode.Source = bitmapImage;
                    System.Windows.Forms.Application.DoEvents();
                }));
                //----------------------------------------------------------
            }
        }

        private void SimulateSendingPaidReceipt_Click(object sender, RoutedEventArgs e)
        {
            HttpResponseMessage response;
            try
            {
                _msg.ShowMessage("");

                //if (string.IsNullOrWhiteSpace(_payClient.CurrentSnRConnectionId))
                //    throw new Exception("Please start a new sale to get a SignalR Connection ID");

                if (string.IsNullOrWhiteSpace(TxtSnRClientId.Text))
                    throw new Exception("Please start a new sale to get a SignalR Connection ID");

                PaymentCustomInfo custInfo = new PaymentCustomInfo()
                {
                    Amount = decimal.Parse(TxtAmount.Text),
                    CreationLocalTime = DateTime.Now,
                    DeviceID = _deviceId,
                    DocumentNo = TxtDocNo.Text.Trim(),
                    MachineCode = "NSSTVM01",
                    //MachineNetworkID = TxtSnRClientId.Text
                    MachineNetworkID = _payClient.CurrentSnRConnectionId
                };

                PaymentReceipt parameters = new PaymentReceipt()
                {
                    CustomField = JsonConvert.SerializeObject(custInfo),
                    Amount = Decimal.Parse(TxtAmount.Text),
                    Description = "Send Payment Receipt Simulator",
                    MerchantId = RegistrySetup.GetRegistrySetting().BTnGMerchantId,
                    MerchantTransactionNo = "My_Doc_n0_X0001",
                    SalesTransactionNo = TxtBTnGSaleTransNo.Text,
                    Signature = "Test_Signature_%&^%^&%&",
                    Status = "paid"
                };

                TimeSpan apiTimeOutSec = new TimeSpan(0, 0, 45);
                HttpClientHandler httpClientHandler = new HttpClientHandler();
                using (HttpClient client = new HttpClient(httpClientHandler))
                {
                    client.Timeout = apiTimeOutSec;
                    // Connect to Local KTMBWebApi URL
                    //client.BaseAddress = new Uri(@"https://localhost:44305/");
                    //client.BaseAddress = new Uri(@"https://ktmb-dev-api.azurewebsites.net/");
                    client.BaseAddress = new Uri(_webApiURL.Replace("/api", ""));
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Authentication
                    //string authStr = SecurityHelper.getSignature();
                    //client.DefaultRequestHeaders.Add("RequestSignature", authStr);
                    // --------------------

                    //PaymentGatewayResp oRet = null;
                    response = null;
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, @"api/PaymentGateway/dispatchPaidReceipt");
                    string StrRequest = null;
                    //if (parameters != null)
                    //{
                    StrRequest = JsonConvert.SerializeObject(parameters);
                    request.Content = new System.Net.Http.StringContent(StrRequest, Encoding.UTF8, "application/json");
                    //}

                    _msg.ShowMessage("Send Simulated Boost Receipt JSon data is ..");
                    _msg.ShowMessage(StrRequest);

                    response = client.SendAsync(request).ConfigureAwait(false).GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        _msg.ShowMessage("Execution successful..");

                        string resultString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        _msg.ShowMessage("Result is ..");
                        _msg.ShowMessage(resultString);

                        //oRet = JsonConvert.DeserializeObject<PaymentGatewayResp>(resultString);

                        //_msg.ShowMessage(oRet.ToJSonStringX());
                    }
                    else
                    {
                        _msg.ShowMessage("fail execution ..");
                    }
                }
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
                _msg.ShowMessage("");
            }
        }

        private void SimulateCancelRefund_Click(object sender, RoutedEventArgs e)
        {
            HttpResponseMessage response;
            try
            {
                _msg.ShowMessage("");

                if (_lastSaleInfo is null)
                    throw new Exception("Previous sale not detected");

                _msg.ShowMessage("SimulateCancelRefund_Click with _lastSaleInfo object ..");
                _msg.ShowMessage(JsonConvert.SerializeObject(_lastSaleInfo, Formatting.Indented));

                //if (string.IsNullOrWhiteSpace(_payClient.CurrentSnRConnectionId))
                //    throw new Exception("Please start a new sale to get a SignalR Connection ID");

                if (string.IsNullOrWhiteSpace(TxtSnRClientId.Text))
                    throw new Exception("Please start a new sale to get a SignalR Connection ID");

                BTnGCancelRefundReqInfo req = new BTnGCancelRefundReqInfo()
                { 
                    SalesTransactionNo = _lastSaleInfo.SalesTransactionNo, 
                    MerchantTransactionNo = _lastSaleInfo.DocNo, 
                    MerchantId = RegistrySetup.GetRegistrySetting().BTnGMerchantId, 
                    Currency = _lastSaleInfo.Currency, 
                    PaymentGateway = _lastSaleInfo.PaymentGateway, 
                    Amount = _lastSaleInfo.Amount
                };
                req.Signature = req.GetSignatureString();

                CancelRefundRequest parameters = new CancelRefundRequest()
                {
                    DeviceId = _deviceId,
                    CancelRefundRequestInfo = req
                };

                TimeSpan apiTimeOutSec = new TimeSpan(0, 0, 45);
                HttpClientHandler httpClientHandler = new HttpClientHandler();
                using (HttpClient client = new HttpClient(httpClientHandler))
                {
                    client.Timeout = apiTimeOutSec;
                    // Connect to Local KTMBWebApi URL
                    //client.BaseAddress = new Uri(@"https://localhost:44305/");
                    //client.BaseAddress = new Uri(@"https://ktmb-dev-api.azurewebsites.net/");
                    client.BaseAddress = new Uri(_webApiURL.Replace("/api", ""));
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Authentication
                    string authStr = SecurityHelper.getSignature();
                    client.DefaultRequestHeaders.Add("RequestSignature", authStr);
                    // --------------------

                    //PaymentGatewayResp oRet = null;
                    response = null;
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, @"api/paymentgateway/cancelRefundSale");
                    string StrRequest = null;
                    //if (parameters != null)
                    //{
                    StrRequest = JsonConvert.SerializeObject(parameters);
                    request.Content = new System.Net.Http.StringContent(StrRequest, Encoding.UTF8, "application/json");
                    //}

                    _msg.ShowMessage("Send Simulated KTMBCTS WebAPI CancelRefund data is ..");
                    _msg.ShowMessage(StrRequest);

                    response = client.SendAsync(request).ConfigureAwait(false).GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        _msg.ShowMessage("Execution successful..");

                        string resultString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        //_msg.ShowMessage(resultString);

                        if (string.IsNullOrWhiteSpace(resultString))
                            throw new Exception("Invalid web response on Cancel/Refund eWallet transaction; (EXITxx)");

                        BTnGCancelRefundResult oRet = JsonConvert.DeserializeObject<BTnGCancelRefundResult>(resultString);

                        if (oRet is null)
                            throw new Exception("Invalid web response on Cancel/Refund eWallet transaction; (EXITxx)");

                        _msg.ShowMessage("Result is (B) ..");
                        _msg.ShowMessage(JsonConvert.SerializeObject(oRet));

                        // CYA-DEBUG - Log oRet(CancelRefundResponse) here

                        if (oRet.Status == true)
                        {
                            //By-pass
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(oRet.MessageString()) == false)
                                _msg.ShowMessage($@"Fail Web Cancel/Refund transaction; (EXITxx); {oRet.MessageString()}");
                            else
                            {
                                _msg.ShowMessage($@"Fail Web Cancel/Refund transaction with unknown error; (EXITxx)");
                            }
                        }
                    }
                    else
                    {
                        _msg.ShowMessage($@"Unable to execute Web Cancel/Refund transaction; (EXITxx);{response?.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
                _msg.ShowMessage("");
            }
        }

        private async void GetSvrTime_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_payClient is null)
                    throw new Exception("Please create sale first");

                string timeStr = await _payClient.GetServerTime();

                _msg.ShowMessage($@"{timeStr}");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"Error on GetSvrTime_Click. {ex.ToString()} ");
            }
        }

        private void EndSale_Click(object sender, RoutedEventArgs e)
        {
            EndSale();
        }

        private async void SendEcho_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_payClient is null)
                    throw new Exception("Please create sale first");

                await _payClient.SendEcho(TxtEchoMessage.Text);
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"Error on GetSvrTime_Click. {ex.ToString()} ");
            }
        }

        private void CancelSale_Click(object sender, RoutedEventArgs e)
        {
            _msg.ShowMessage("");
            try
            {
                this.Dispatcher.Invoke(new Action(() => {
                    TxtMessage.Text = "Transaction has been canceled";
                    TxtMessage.Visibility = Visibility.Collapsed;
                    System.Windows.Forms.Application.DoEvents();
                }));

                CancelSale();
                //_countDown?.ChangeCountDown(_countDownCode_WaitForCancelEnd, 3, 500);
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"Error when receiving Payment Request Result. {ex.ToString()}");
            }
            return;
        }

        private string _lastCanceledSaleTransactionNo = null;
        private void CancelSale()
        {
            if (_lastSaleInfo is null)
                throw new Exception("Previous sale not detected");

            if (_lastCanceledSaleTransactionNo?.Equals(_lastSaleInfo.SalesTransactionNo) == true)
                return;

            _msg.ShowMessage("CancelSale_Click with _lastSaleInfo object ..");
            _msg.ShowMessage(JsonConvert.SerializeObject(_lastSaleInfo, Formatting.Indented));

            _bTnGJob.CancelRefundSale(_lastSaleInfo.SalesTransactionNo, _lastSaleInfo.DocNo, _lastSaleInfo.Currency, _lastSaleInfo.PaymentGateway, _lastSaleInfo.Amount, BTnGKioskVoidTransactionState.CancelRefundRequest);
            _lastCanceledSaleTransactionNo = _lastSaleInfo.SalesTransactionNo;

            _msg.ShowMessage($@"CancelRefundSale '{_lastSaleInfo.SalesTransactionNo}' Sent");
        }

        private void EndSale()
        {
            try
            {
                if (_payClient is null)
                    throw new Exception("No valid Sale Instant found");

                _payClient.Dispose();

                this.Dispatcher.Invoke(new Action(() => {
                    BtnNewSale.IsEnabled = true;
                    BtnShowSvrTime.IsEnabled = false;
                    BtnSendEcho.IsEnabled = false;
                    //BtnCancelSale.IsEnabled = false;
                    BtnEndSale.IsEnabled = false;
                    BtnShowConnectionId.IsEnabled = false;

                    BtnSimulatePaidReceipt.IsEnabled = false;
                    //BtnSimulateCancelRefund.IsEnabled = false;

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

        class LocalSaleInfo
        {
            public string SalesTransactionNo { get; set; }
            public string DocNo { get; set; }
            public string Currency { get; set; }
            public string PaymentGateway { get; set; }
            public decimal Amount { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string ContactNo { get; set; }
        }
    }
}
