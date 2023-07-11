using kvdt_kiosk.Models.Komlink;
using kvdt_kiosk.Services.Komlink;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using System.Windows.Threading;

namespace kvdt_kiosk.Views.Komlink
{
    /// <summary>
    /// Interaction logic for BigAlert.xaml
    /// </summary>
    public partial class BigAlert : UserControl
    {
        private DispatcherTimer scanTimer = new DispatcherTimer();
        private DispatcherTimer checkKomlinkCardTimer = new DispatcherTimer();

        private const string StartIM30ReaderUrl = "http://127.0.0.1:1234/Para=15&IMPR=1,2&FTVM=1";
        private const string GetDataFromIM30ReaderURL = "http://127.0.0.1:1234/Para=17";

        private int MaxScanTime = 60; // Maximum time allowed for scanning in seconds
        private const int ScanWarningTime = 5; // Time remaining to show warning message
        private bool isRetryScan = false;

        private bool isForTopup = false;

        private int retryCount = 0;

        private static Lazy<BigAlert> bigAlert = new Lazy<BigAlert>(() => new BigAlert());
        public event EventHandler onSuccessToUp;
        public event EventHandler<OnFailureScanEventArgs> onFailureToUp;
        public event EventHandler onFailureScanCard;
        public static BigAlert GetBigAlertPage()
        {
            return bigAlert.Value;
        }

        public BigAlert()
        {
            InitializeComponent();
            LoadLanguage();
            //LoadScan();
        }

        private void LoadLanguage()
        {
            try
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (App.Language == "ms")
                    {
                        AlertTitle.Text = "Sila imbas Kad Komlink anda di pembaca";
                    }
                }));
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in BigAlertScreen.xaml.cs");
            }
        }

        public void scanFor(string caseFor)
        {
            if (caseFor == "TopUp")
            {
                isForTopup = true;
            }
            else
            {
                isForTopup = false;
            }
        }

        public void CheckIsRetry(bool isRetryScan)
        {
            this.isRetryScan = isRetryScan;
        }

        public void IncrementRetryScan(int retryCount)
        {
            this.retryCount += retryCount;
        }

        public void SetRetryScanToZerro()
        {
            this.retryCount = 0;
        }

        private void ScanWaitTimer()
        {
            SystemConfig.IsResetIdleTimer = true;

            try
            {
                int count = MaxScanTime;
                scanTimer.Tick += async (sender, e) =>
                {
                    count--;



                    if (count <= 0)
                    {
                        scanTimer.Stop();
                        if (isForTopup)
                        {
                            //OnFailureScanEventArgs onFailureScanEventArgs = new OnFailureScanEventArgs(retryCount: retryCount);
                            //onFailureToUp.Invoke(null, onFailureScanEventArgs);
                        }
                        else
                        {
                            BacktoLanguageScreen();
                        }
                    }
                    else
                    {
                        await Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ScanTimer.Text = count.ToString();
                        }));
                    }
                };

                scanTimer.Interval = TimeSpan.FromSeconds(1);
                scanTimer.Start();
            }
            catch (Exception ex)
            {
                scanTimer.Stop();
                MessageBox.Show("Scan Timer Error");
            }
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
        private void BacktoLanguageScreen()
        {
            UserSession.ClearSession();
            Application.Current.Shutdown();

            StopTransaction();
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


        private async Task<string> StartIM30ReaderForTopUp()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync($"http://127.0.0.1:1234/Para=15&IMMT=7&IMPR=1,2,{UserSession.TotalTicketPrice * 100},{DateTime.Now},{UserSession.requestAddTopUpModel.TransactionNo}");
                    string responsedata = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        string responseString = responsedata;

                        if (!string.IsNullOrEmpty(responseString))
                        {
                            string[] responseStringArray = responseString.Split(',');

                            if (responseStringArray[0] == "0")
                            {
                                SetTimeInterval(IM30CardResult, 1000, 100);
                                //await IM30CardResult();  
                            }
                            else
                            {

                                if (App.Language == "ms")
                                {
                                    ScanText.Text = "Tunggu sebentar";
                                }
                                else
                                {
                                    ScanText.Text = "Please wait...";
                                }
                                checkKomlinkCardTimer.Stop();
                                await StartIM30ReaderForTopUp();
                            }
                        }
                    }
                    return responsedata;
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }



        private async Task<string> StartIM30Reader()
        {
            SystemConfig.IsResetIdleTimer = true;

            try
            {
                using (HttpClient client = new HttpClient())
                {

                    var response = await client.GetStringAsync(StartIM30ReaderUrl);

                    if (response != null)
                    {

                        string[] responseStringArray = response.Split(',');

                        if (responseStringArray[0] == "0")
                        {
                            SetTimeInterval(IM30CardResult, 1000, 100);
                        }
                        else
                        {
                            if (App.Language == "ms")
                            {
                                ScanText.Text = "Tunggu sebentar";
                            }
                            else
                            {
                                ScanText.Text = "Please wait...";
                            }

                            checkKomlinkCardTimer.Stop();
                            await StartIM30Reader();
                        }
                    }

                    return response;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception as needed
                return string.Empty; // Or return a default value
            }
        }

        private async Task IM30CardResult()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync(GetDataFromIM30ReaderURL);

                    if (response != null)
                    {
                        string cardParam = response;
                        if (!string.IsNullOrEmpty(cardParam))
                        {
                            string[] responseData = cardParam.Split(',');

                            if (responseData[0] == "200" && !isForTopup)
                            {
                                scanTimer.Stop();
                                checkKomlinkCardTimer.Stop();
                                int commaIndex = cardParam.IndexOf(',');
                                string cardDetails = cardParam.Substring(commaIndex + 1);
                                var jsonObj = JObject.Parse(cardDetails);

                                await GetKomlinkCardFromApi(jsonObj);
                            }
                            else if (responseData[0] == "0" && isForTopup)
                            {
                                scanTimer.Stop();
                                checkKomlinkCardTimer.Stop();

                                onSuccessToUp.Invoke(null, null);
                            }

                            else if (responseData[0] == "9" || responseData[0] == "11" || responseData[0] == "10")
                            {
                                ScanText.Text = responseData[1];
                            }
                            else if (responseData[0] == "1" && isForTopup)
                            {
                                ScanText.Text = responseData[1];

                                // Thread.Sleep(4000);
                                retryCount += 1;

                                OnFailureScanEventArgs onFailureScanEventArgs = new OnFailureScanEventArgs(retryCount: retryCount);

                                onFailureToUp.Invoke(null, onFailureScanEventArgs);

                                scanTimer.Stop();
                                checkKomlinkCardTimer.Stop();

                                return;


                            }
                            else if (responseData[0] == "1" && !isForTopup)
                            {
                                scanTimer.Stop();
                                checkKomlinkCardTimer.Stop();
                                ScanText.Text = responseData[1];
                                onFailureScanCard.Invoke(null, null);

                            }
                        }
                        else
                        {
                            ScanText.Text = "System error: Empty response";
                            scanTimer.Stop();
                            checkKomlinkCardTimer.Stop();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception as needed
                MessageBox.Show("Error occurred: " + ex.Message);
            }
        }

        private async Task GetKomlinkCardFromApi(JObject jsonObj)
        {
            try
            {
                APIServices aPIServices = new APIServices(new APIURLServices(), new APISignatureServices());
                var result = await aPIServices.SearchKomlinkCard(jsonObj);

                var data = result?.Data;

                UserSession.CreateSessionAction(data);

                KomlinkCardDetailScreen komlinkCardDetailScreen = new KomlinkCardDetailScreen();
                Window.GetWindow(this).Content = komlinkCardDetailScreen;
                App.komlinkCardDetailScreen = komlinkCardDetailScreen;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Scan Timer Error");
            }
        }

        private async void LoadScan()
        {
            SystemConfig.IsResetIdleTimer = true;
            await Dispatcher.InvokeAsync(new Action(async () =>
            {
                MaxScanTime = 60;
                ScanWaitTimer();

                if (isForTopup)
                {
                    await StartIM30ReaderForTopUp();
                }
                else
                {
                    await StartIM30Reader();
                }

                //CheckDataThread();
            }));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            LoadScan();
            retryCount = 0;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {


        }
    }
}
