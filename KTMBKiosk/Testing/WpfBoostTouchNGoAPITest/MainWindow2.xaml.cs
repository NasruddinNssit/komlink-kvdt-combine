using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Constant.BTnG;
using NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx;
using NssIT.Kiosk.Network.PaymentGatewayApp.JobApp.BTnGJob;
using NssIT.Kiosk.Network.SignalRClient.API.Base.Extension;
using NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway;
using NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Data;
using NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Data.Request;
using NssIT.Kiosk.Network.SignalRClient.API.PaymentGateway.Data.Response;
using NssIT.Kiosk.Sqlite.DB.AccessDB;
using NssIT.Kiosk.Sqlite.DB.AccessDB.Works;
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
using NssIT.Kiosk.Network.PaymentGatewayApp;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Train.Kiosk.Common.Data;
using NssIT.Kiosk.Network.PaymentGatewayApp.CustomApp;
using NssIT.Kiosk.AppDecorator.UI;

namespace WpfBoostTouchNGoAPITest
{
    /// <summary>
    /// Interaction logic for MainWindow2.xaml
    /// </summary>
    public partial class MainWindow2 : Window
    {
        private LibShowMessageWindow.MessageWindow _msg = null;
        private const AppModule appModule = AppModule.UIBTnG;

        //private string _eWalletWebApiBaseURL = "";
        private string _webApiURL = "";
        private string _deviceId = "*";

        private string _countDownCode_InitToStart = "BTnG_CountDown_InitToStart";
        private string _countDownCode_FailToStart = "BTnG_CountDown_FailToStart";
        private string _countDownCode_WaitForScanning = "BTnG_CountDown_WaitForScanning";
        private string _countDownCode_WaitForSuccessEnd = "BTnG_CountDown_WaitForSuccessEnd";
        private string _countDownCode_WaitForCancelEnd = "BTnG_CountDown_WaitForCancelEnd";
        // _countDownCode_WaitForFailTransactionEnd : Payment Transaction finished with error found (UI or Internal error)
        private string _countDownCode_WaitForFailTransactionEnd = "BTnG_CountDown_WaitForFailTransactionEnd";

        private IUIApplicationJob _bTnGApp = null;

        private AppModule _appModule = AppModule.UIBTnG;

        public (string SalesTransactionNo, string MerchantTransactionNo, decimal Amount, string SnRId, string PaymentGateway, string Currency) _saleTrans =
            (SalesTransactionNo: null, MerchantTransactionNo: null, Amount: 0.0M, SnRId: null, PaymentGateway: null, Currency: null);


        public MainWindow2()
        {
            InitializeComponent();

            _deviceId = RegistrySetup.GetRegistrySetting().DeviceId;

            _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;
            //_eWalletWebApiBaseURL = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting().EWalletWebApiBaseURL;
            _webApiURL = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting().WebApiURL;

            TxtWebApi.Text = _webApiURL;

            _bTnGApp = new BTnGApplication(_msg);
            _bTnGApp.OnShowResultMessage += _bTnGApp_OnShowResultMessage;
        }

        private void _bTnGApp_OnShowResultMessage(object sender, NssIT.Kiosk.AppDecorator.UI.UIMessageEventArgs e)
        {
            //CYA-DEBUG .. need to log here with e ..

            if (e.KioskMsg.GetMsgData() is UIxGnBTnGAck<BTnGGetPaymentGatewayResult> uixData)
            {
                if (uixData.IsDataReadSuccess)
                {
                    this.Dispatcher.Invoke(new Action(() => 
                    {
                        CboPaymentService.Items.Clear();
                        foreach (BTnGPaymentGatewayDetailModel payGate in uixData.Data.Data.PaymentGatewayList)
                        {
                            CboPaymentService.Items.Add(new ComboBoxItem() { Content = $@"{payGate.PaymentGatewayName} - {payGate.PaymentGateway}", Uid = payGate.PaymentGateway });
                        }
                    }));
                }
                else 
                {
                    if (string.IsNullOrWhiteSpace(uixData.Error?.Message))
                        _msg.ShowMessage($@"_bTnGApp_OnShowResultMessage : Fail for reading available Payment Gateway; (EXITxx)");
                    else
                        _msg.ShowMessage($@"_bTnGApp_OnShowResultMessage : {uixData.Error.Message}");
                }
            }
            else if (e.KioskMsg.GetMsgData() is IUIxBTnGPaymentGroupAck)
            {
                var payData = e.KioskMsg.GetMsgData();
                if (payData is UIxBTnGPaymentNewTransStartedAck newTrans)
                {
                    _saleTrans.Amount = newTrans.Amount;
                    _saleTrans.MerchantTransactionNo = newTrans.MerchantTransactionNo;
                    _saleTrans.SalesTransactionNo = newTrans.BTnGSalesTransactionNo;
                    _saleTrans.SnRId = newTrans.SnRId;

                    ShowBarCode(newTrans.Base64ImageQrCode);

                    this.Dispatcher.Invoke(new Action(() => {
                        TxtSnRClientId.Text = (newTrans.SnRId ?? "").Trim();
                        TxtBTnGSaleTransNo.Text = _saleTrans.SalesTransactionNo;
                    }));

                    //_countDown?.ChangeCountDown(_countDownCode_WaitForScanning, 75, 500);
                }
                else if (payData is UIxBTnGPaymentCountDownAck cntD)
                {
                    this.Dispatcher.Invoke(new Action(() => {
                        TxtCountDown.Text = cntD.CountDown.ToString();
                    }));
                }
                else if (payData is UIxBTnGPaymentCustomerMsgAck custMsg)
                {
                    this.Dispatcher.Invoke(new Action(() => {
                        if (custMsg.Message != null)
                            TxtMessage.Text = custMsg.Message.Trim();
                    }));
                }
                else if (payData is UIxBTnGPaymentInProgressMsgAck prgMsg)
                {
                    this.Dispatcher.Invoke(new Action(() => {
                        if (prgMsg.Message != null)
                            TxtInProgressMsg.Text = prgMsg.Message.Trim();

                        if (prgMsg.IsCancelAllowed.HasValue) {}
                    }));
                }
                else if (payData is UIxBTnGPaymentErrorAck errMsg)
                {
                    this.Dispatcher.Invoke(new Action(() => {
                        if (errMsg.ErrorMsg != null)
                            TxtError.Text = errMsg.ErrorMsg.Trim();
                    }));
                }
                else if (payData is UIxBTnGPaymentEndAck endMsg)
                {
                    this.Dispatcher.Invoke(new Action(() => {

                        EndSale();

                        if (endMsg.ResultState == PaymentResult.Success)
                        {
                            TxtInProgressMsg.Text = "Payment Success";
                        }
                        else if (endMsg.ResultState == PaymentResult.Cancel)
                        {
                            TxtInProgressMsg.Text = "Payment Transaction has been canceled";
                        }
                        else
                        {
                            string endMessage = (endMsg.ErrorMsg ?? "").Trim();

                            if (string.IsNullOrWhiteSpace(endMessage) == false)
                                endMessage = endMsg.Message ?? "";

                            if (endMsg.ResultState == PaymentResult.Timeout)
                            {
                                TxtInProgressMsg.Text = $@"Timeout; Fail Payment Transaction; {endMessage}";
                            }
                            else if (endMsg.ResultState == PaymentResult.Fail)
                            {
                                TxtInProgressMsg.Text = $@"Fail Payment Transaction; {endMessage}";
                            }
                            else 
                            {
                                TxtInProgressMsg.Text = $@"Fail Payment Transaction with unknown error; {endMessage}";
                            }
                        }

                        TxtMessage.Text = TxtInProgressMsg.Text;
                        TxtCountDown.Text = "END";
                    }));
                }
            }

            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
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

        private void GetAllService_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _msg.ShowMessage("");

                _bTnGApp.SendInternalCommand("*", Guid.NewGuid(), new UIReq<UIxBTnGGetAvailablePaymentGatewayRequest>("*", _appModule, DateTime.Now, new UIxBTnGGetAvailablePaymentGatewayRequest("*")));

                _msg.ShowMessage("GetAllService_Click : Done");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
                _msg.ShowMessage("");
            }
        }

        private void SimulateSendingPaidReceipt_Click(object sender, RoutedEventArgs e)
        {
            HttpResponseMessage response;
            try
            {
                _msg.ShowMessage("");

                if (string.IsNullOrWhiteSpace(TxtSnRClientId.Text))
                    throw new Exception("Please start a new sale to get a SignalR Connection ID");

                PaymentCustomInfo custInfo = new PaymentCustomInfo()
                {
                    Amount = decimal.Parse(TxtAmount.Text),
                    CreationLocalTime = DateTime.Now,
                    DeviceID = _deviceId,
                    DocumentNo = TxtDocNo.Text.Trim(),
                    MachineCode = "NSSTVM01",
                    MachineNetworkID = _saleTrans.SnRId
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

                if (string.IsNullOrWhiteSpace(_saleTrans.SalesTransactionNo))
                    throw new Exception("Previous sale not found");

                _msg.ShowMessage("SimulateCancelRefund_Click with _lastSaleInfo object ..");
                _msg.ShowMessage(JsonConvert.SerializeObject(_saleTrans, Formatting.Indented));

                if (string.IsNullOrWhiteSpace(TxtSnRClientId.Text))
                    throw new Exception("Please start a new sale to get a SignalR Connection ID");

                BTnGCancelRefundReqInfo req = new BTnGCancelRefundReqInfo()
                {
                    SalesTransactionNo = _saleTrans.SalesTransactionNo,
                    MerchantTransactionNo = _saleTrans.MerchantTransactionNo,
                    MerchantId = RegistrySetup.GetRegistrySetting().BTnGMerchantId,
                    Currency = _saleTrans.Currency,
                    PaymentGateway = _saleTrans.PaymentGateway,
                    Amount = _saleTrans.Amount
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
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, @"api/PaymentGateway/cancelRefundSale");
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

        private void GetSvrTime_Click(object sender, RoutedEventArgs e)
        {
            _msg.ShowMessage("");
            try
            {
                _bTnGApp.SendInternalCommand("*", Guid.NewGuid(),
                    new UIReq<UIxBTnGTestReadServerTimeRequest>("*", _appModule, DateTime.Now,
                        new UIxBTnGTestReadServerTimeRequest(_saleTrans.MerchantTransactionNo)));

                //_countDown?.ChangeCountDown(_countDownCode_WaitForCancelEnd, 3, 500);
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"Error when receiving Payment Request Result. {ex.ToString()}");
            }
            return;
        }

        private void EndSale_Click(object sender, RoutedEventArgs e)
        {
            EndSale();
        }

        private void SendEcho_Click(object sender, RoutedEventArgs e)
        {
            _msg.ShowMessage("");
            try
            {
                _bTnGApp.SendInternalCommand("*", Guid.NewGuid(),
                    new UIReq<UIxBTnGTestEchoMessageSendRequest>("*", _appModule, DateTime.Now,
                        new UIxBTnGTestEchoMessageSendRequest(_saleTrans.MerchantTransactionNo, TxtEchoMessage.Text)));

                //_countDown?.ChangeCountDown(_countDownCode_WaitForCancelEnd, 3, 500);
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"Error when receiving Payment Request Result. {ex.ToString()}");
            }
            return;
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

                _bTnGApp.SendInternalCommand("*", Guid.NewGuid(), 
                    new UIReq<UIxBTnGCancelRefundPaymentRequest>("*", _appModule, DateTime.Now,
                        new UIxBTnGCancelRefundPaymentRequest(
                            _saleTrans.MerchantTransactionNo, _saleTrans.SalesTransactionNo, 
                            _saleTrans.MerchantTransactionNo, _saleTrans.Currency, 
                            _saleTrans.PaymentGateway, _saleTrans.Amount)));

                //_countDown?.ChangeCountDown(_countDownCode_WaitForCancelEnd, 3, 500);
            }
            catch (Exception ex)
            {
                _msg.ShowMessage($@"Error when receiving Payment Request Result. {ex.ToString()}");
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
                    BtnShowSvrTime.IsEnabled = false;
                    BtnSendEcho.IsEnabled = false;
                    //BtnCancelSale.IsEnabled = false;

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

        private void CreateNewSale_Click(object sender, RoutedEventArgs e)
        {
            _msg.ShowMessage("");
            try
            {
                if (CboPaymentService.SelectedItem is ComboBoxItem cItm)
                {
                    //By pass
                }
                else
                    throw new Exception("Please select a Payment Service");

                _msg.ShowMessage("Payment Start ..");
                BtnNewSale.IsEnabled = false;
                BtnShowSvrTime.IsEnabled = true;
                BtnSendEcho.IsEnabled = true;
                ////BtnCancelSale.IsEnabled = true;
                BtnSimulatePaidReceipt.IsEnabled = true;
                ////BtnSimulateCancelRefund.IsEnabled = true;

                TxtMessage.Visibility = Visibility.Collapsed;
                TxtError.Visibility = Visibility.Collapsed;
                TxtMessage.Visibility = Visibility.Visible;
                TxtMessage.Text = "Request BTnG Payment..";

                _saleTrans.Amount = 0.0M;
                _saleTrans.MerchantTransactionNo = null;
                _saleTrans.SalesTransactionNo = null;
                _saleTrans.SnRId = null;
                _saleTrans.PaymentGateway = cItm.Uid;
                _saleTrans.Currency = "MYR";

                _bTnGApp.SendInternalCommand("*", Guid.NewGuid(), 
                    new UIReq<UIxBTnGPaymentMakeNewPaymentRequest>("*", _appModule, DateTime.Now,
                        new UIxBTnGPaymentMakeNewPaymentRequest(TxtDocNo.Text.Trim(), TxtDocNo.Text.Trim(), decimal.Parse(TxtAmount.Text), _saleTrans.PaymentGateway, "MYR", "Testing Chong",
                            "Testing Chong", "0171234567", "*")));

                TxtCountDown.Text = "..";
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
            finally
            {
                TxtCountDown.Text = "..";
            }
        }
    }
}