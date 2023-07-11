using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Train.Kiosk.Common.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
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
using WpfBoostTouchNGoSample.Base;
using WpfBoostTouchNGoSample.Base.Extension;
using WpfBoostTouchNGoSample.Data;
using WpfBoostTouchNGoSample.Data.Request;
using WpfBoostTouchNGoSample.Data.Response;

namespace WpfBoostTouchNGoSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LibShowMessageWindow.MessageWindow _msg = null;

        public MainWindow()
        {
            InitializeComponent();
            _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;

            Setting setting = Setting.GetSetting();
            setting.HashSecretKey = @"b7edee98eb074d8eb67b8b20f5a3ab13";
            setting.AesEncryptKey = @"0a4ef44c211f4c8dbb4f8ca73aabb5d3";
            setting.TVMKey = @"9eksd92ks9378qwjs92ks92ls02ls02l";
            setting.TimeZoneId = @"Singapore Standard Time";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (e.Source.Equals(this) == false)
                return;
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
                    client.BaseAddress = new Uri(@"https://gopayment-api-dev.azurewebsites.net/");
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
                    StrRequest = @"{""merchantId"": ""KTMB""}";
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
                        foreach (PaymentGatewayDetail pDet in oRet.Data.PaymentGatewayList)
                        {
                            CboPaymentService.Items.Add(new ComboBoxItem() { Content = $@"{pDet.PaymentGatewayName} - {pDet.PaymentGateway}", Uid = pDet.PaymentGateway });
                        }

                        _msg.ShowMessage(oRet.ToJSonStringX());
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

        private void CreateNewSale_Click(object sender, RoutedEventArgs e)
        {
            HttpResponseMessage response;
            try
            {
                _msg.ShowMessage("");

                CreateSaleReq parameters = GetNewSale();

                TimeSpan apiTimeOutSec = new TimeSpan(0, 0, 45);
                HttpClientHandler httpClientHandler = new HttpClientHandler();
                using (HttpClient client = new HttpClient(httpClientHandler))
                {
                    client.Timeout = apiTimeOutSec;
                    client.BaseAddress = new Uri(@"https://gopayment-api-dev.azurewebsites.net/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Authentication
                    /////client.DefaultRequestHeaders.Add("RequestSignature", SecurityHelper.getSignature());
                    // --------------------

                    CreateSaleResp oRet = null;
                    response = null;
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, @"api/Sales/create_sale");
                    string StrRequest = null;
                    //if (parameters != null)
                    //{
                    StrRequest = JsonConvert.SerializeObject(parameters);
                    request.Content = new System.Net.Http.StringContent(StrRequest, Encoding.UTF8, "application/json");
                    //}

                    _msg.ShowMessage("Sale JSon data is ..");
                    _msg.ShowMessage(StrRequest);

                    response = client.SendAsync(request).ConfigureAwait(false).GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        _msg.ShowMessage("Execution successful..");

                        string resultString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        _msg.ShowMessage("Result is ..");
                        _msg.ShowMessage(resultString);

                        oRet = JsonConvert.DeserializeObject<CreateSaleResp>(resultString);

                        _msg.ShowMessage("Result in proper JSon format ..");
                        _msg.ShowMessage(oRet.ToJSonStringX());

                        ShowBarCode(oRet.Data.Base64ImageQrCode);
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

            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            CreateSaleReq GetNewSale()
            {
                string docNo = "";
                if (TxtDocNo.Text.Trim().Length > 0)
                    docNo = TxtDocNo.Text.Trim();
                else
                {
                    docNo = Guid.NewGuid().ToString();
                    TxtDocNo.Text = docNo;
                }

                string payGate = "";
                if (CboPaymentService.SelectedItem is ComboBoxItem cItm)
                {
                    payGate = cItm.Uid;
                }
                else
                    throw new Exception("Please select a Payment Service");

                SaleCustomFieldInfo custInfo = new SaleCustomFieldInfo()
                {
                    MachineNetworkID = TxtSnRClientId.Text.Trim(), 
                    DocumentNo = TxtDocNo.Text.Trim(), 
                    Amount = decimal.Parse(TxtAmount.Text), 
                    MachineID = "NSSTVM01", 
                    CreationTime = DateTime.Now 
                };

                CreateSaleReq sale = new CreateSaleReq()
                {
                    MerchantId = "KTMB",
                    //PaymentGateway = "boost",
                    //PaymentGateway = "touchngo_offline",
                    PaymentGateway = payGate,
                    MerchantTransactionNo = docNo,
                    Currency = "MYR",
                    Amount = decimal.Parse(TxtAmount.Text),
                    //NotificationUrl = @"https://www.google.com",
                    NotificationUrl = @"https://ktmb-dev-api.azurewebsites.net/api/paymentgateway/dispatchPaidReceipt",
                    CustomField = JsonConvert.SerializeObject(custInfo),
                    OrderTitle = "CTS Payment Sale Test",
                    DisplayName = "CTS Payment Sale",
                    TerminalType = "Kiosk",
                    ExpirySecond = 0,
                    PayerInfo = new CustomerShortInfo()
                    {
                        FirstName = "NssIt Test Person Name 1",
                        LastName = "NssIt Person Name 1",
                        ContactNo = "0178888999"
                        //,
                        //Address = "123",
                        //City = "123",
                        //Country = "123",
                        //Email = "123"
                    }
                };

                sale.Signature = sale.GetSignatureString();

                return sale;
            }

            void ShowBarCode(string base64String)
            {
                byte[] imgBytes = Convert.FromBase64String(base64String);

                BitmapImage bitmapImage = new BitmapImage();
                MemoryStream ms = new MemoryStream(imgBytes);
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();

                //Image object
                imgBarcode.Source = bitmapImage;

            }

        }

        private void SimulateSendingPaidReceipt_Click(object sender, RoutedEventArgs e)
        {
            HttpResponseMessage response;
            try
            {
                _msg.ShowMessage("");

                SaleCustomFieldInfo custInfo = new SaleCustomFieldInfo()
                {
                    MachineNetworkID = TxtSnRClientId.Text.Trim(),
                    DocumentNo = TxtDocNo.Text.Trim(),
                    Amount = decimal.Parse(TxtAmount.Text),
                    MachineID = "NSSTVM01",
                    CreationTime = DateTime.Now
                };

                SampleBoostPaidReceipt parameters = new SampleBoostPaidReceipt() { 
                    CustomField = JsonConvert.SerializeObject(custInfo),
                    Amount = Decimal.Parse(TxtAmount.Text), 
                    Description = "Send Payment Receipt Simulator", 
                    MerchantId = "KTMB", 
                    MerchantTransactionNo = "My_Doc_n0_X0001", 
                    SalesTransactionNo = "BTnG_Unque_n0", 
                    Signature = "Test_Signature_%&^%^&%&", 
                    Status = "paid"};

                TimeSpan apiTimeOutSec = new TimeSpan(0, 0, 45);
                HttpClientHandler httpClientHandler = new HttpClientHandler();
                using (HttpClient client = new HttpClient(httpClientHandler))
                {
                    client.Timeout = apiTimeOutSec;
                    // Connect to Local KTMBWebApi URL
                    //client.BaseAddress = new Uri(@"https://localhost:44305/");
                    //client.BaseAddress = new Uri(@"https://ktmb-dev-api.azurewebsites.net/");
                    client.BaseAddress = new Uri(((ComboBoxItem)CboWebAPIUrl.SelectedItem).Content.ToString());  

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Authentication
                    //string authStr = SecurityHelper.getSignature();
                    //client.DefaultRequestHeaders.Add("RequestSignature", authStr);
                    // --------------------

                    //PaymentGatewayResp oRet = null;
                    response = null;
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, @"api/paymentgateway/dispatchPaidReceipt");
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

            return;
        }

        private void TestReqKTMBWebApiSendSnRMsg_Click(object sender, RoutedEventArgs e)
        {
            HttpResponseMessage response;
            try
            {
                _msg.ShowMessage("");

                if (string.IsNullOrWhiteSpace(TxtSnRClientId.Text))
                    throw new Exception("Please enter SignalR Destination Id");

                if (string.IsNullOrWhiteSpace(TxtSnRTextMessage.Text))
                    throw new Exception("Please enter SignalR Test Message");

                if (string.IsNullOrWhiteSpace(TxtSnRDestMethodName.Text))
                    throw new Exception("Please enter SignalR Destination Method Name");

                SnRTestMessageReq parameters = new SnRTestMessageReq() 
                { 
                    DestinationId = TxtSnRClientId.Text.Trim(), 
                    DestinationMethodName = TxtSnRDestMethodName.Text.Trim(), 
                    Message = TxtSnRTextMessage.Text.Trim() 
                };

                TimeSpan apiTimeOutSec = new TimeSpan(0, 0, 45);
                HttpClientHandler httpClientHandler = new HttpClientHandler();
                using (HttpClient client = new HttpClient(httpClientHandler))
                {
                    client.Timeout = apiTimeOutSec;
                    // Connect to Local KTMBWebApi URL
                    //client.BaseAddress = new Uri(@"https://localhost:44305/");
                    //client.BaseAddress = new Uri(@"https://ktmb-dev-api.azurewebsites.net/");
                    client.BaseAddress = new Uri(((ComboBoxItem)CboWebAPIUrl.SelectedItem).Content.ToString());

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Authentication
                    string authStr = SecurityHelper.getSignature();
                    client.DefaultRequestHeaders.Add("RequestSignature", authStr);
                    // --------------------

                    //PaymentGatewayResp oRet = null;
                    response = null;
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, @"api/RealTimeTest/sendTargetMessage");
                    string StrRequest = null;
                    //if (parameters != null)
                    //{
                    StrRequest = JsonConvert.SerializeObject(parameters);
                    request.Content = new System.Net.Http.StringContent(StrRequest, Encoding.UTF8, "application/json");
                    //}

                    _msg.ShowMessage("Request KTMBWebApi to Send Test Message to a SignalR Destination (Client) ..");
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

            return;
        }

        private void ClearBarcode_Click(object sender, RoutedEventArgs e)
        {
            imgBarcode.Source = null;
        }
    }
}
