using Komlink.Models;
using Komlink.Services;
using Komlink.Views.Error;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Komlink.Views.Komlink
{
    public partial class BigAlert : UserControl
    {
        private DispatcherTimer scanTimer = new DispatcherTimer();
        private DispatcherTimer checkKomlinkCardTimer = new DispatcherTimer();

        private const string StartIM30ReaderUrl = "http://127.0.0.1:1234/Para=15&IMPR=1,2&FTVM=1";
        private const string GetDataFromIM30ReaderURL = "http://127.0.0.1:1234/Para=17";

        private const int MaxScanTime = 60; // Maximum time allowed for scanning in seconds
        private const int ScanWarningTime = 5; // Time remaining to show warning message

        private bool isRetry = false;

        public BigAlert()
        {
            InitializeComponent();
            LoadLanguage();
            LoadScan();
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

        private void ScanWaitTimer()
        {
            try
            {
                int count = MaxScanTime;
                scanTimer.Tick += async (sender, e) =>
                {
                    count--;

                    if (isRetry)
                    {
                        await StartIM30Reader();
                    }
                    isRetry = false;

                    if (count <= 0)
                    {
                        scanTimer.Stop();
                        checkKomlinkCardTimer.Stop();
                        BacktoLanguageScreen();
                    }
                    else
                    {
                        //Dispatcher.BeginInvoke(new Action(() =>
                        //{
                           
                        //}));

                        ScanTimer.Text = count.ToString();
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

        private async void CheckDataThread()
        {
            await IM30CardResult();
        }

        private void BacktoLanguageScreen()
        {
            LanguageScreen languageScreen = new LanguageScreen();
            Window.GetWindow(this).Content = languageScreen;
        }

        private async Task<string> StartIM30Reader()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(StartIM30ReaderUrl);
                    string responsedata = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        string responseString = responsedata;

                        if (!string.IsNullOrEmpty(responseString))
                        {
                            string[] responseStringArray = responseString.Split(',');

                            if (responseStringArray[0] == "0")
                            {
                                CheckDataThread();
                            }
                        }
                    }

                    return responsedata;
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
                    HttpResponseMessage response = await client.GetAsync(GetDataFromIM30ReaderURL);

                    if (response.IsSuccessStatusCode)
                    {
                        string cardParam = await response.Content.ReadAsStringAsync();
                        //cardParam = "200,{\"CSN\":\"04057202DB\",\"KVDTCardNo\":599621776138682479,\"IsCardCancelled\":false,\"IsCardBlacklisted\":true,\"IsSP1Available\":false,\"IsSP2Available\":false,\"IsSP3Available\":false,\"IsSP4Available\":false,\"IsSP5Available\":false,\"IsSP6Available\":false,\"IsSP7Available\":false,\"IsSP8Available\":false,\"ChkInGateNo\":\"\",\"ChkInDatetime\":\"2001-01-01T00:00:00\",\"ChkInStationNo\":\"\",\"ChkOutGateNo\":\"\",\"ChkOutDatetime\":\"2001-01-01T00:00:00\",\"ChkOutStationNo\":\"\",\"MainPurse\":49.1,\"MainTransNo\":64990,\"IssuerSAMId\":\"0CA510231FCC92DD\",\"Gender\":\"M\",\"CardIssuedDate\":\"2024-06-21T00:00:00\",\"CardExpireDate\":\"2001-01-01T00:00:00\",\"PNR\":\"PNR-24EC8186\",\"CardType\":\"S100\",\"CardTypeExpireDate\":\"2024-04-26T00:00:00\",\"IsMalaysian\":true,\"DOB\":\"2001-01-01T00:00:00\",\"LRCKey\":76,\"IDType\":\"IdentityNo\",\"IDNo\":\"001025142019\",\"PassengerName\":\"MOO ZHI HUNG\",\"BLKStartDatetime\":\"2023-06-23T00:00:00\",\"RefillSAMId\":\"0CA510231FCC92DD\",\"RefillDatetime\":\"2023-06-23T11:29:52\",\"LastTransDatetime\":\"2023-06-23T11:29:52\",\"BackupPurse\":49.1,\"BackupTransNo\":64990,\"BLKSAMId\":\"0CA510231FCC92DD\",\"BLKDatetime\":\"2023-06-23T11:29:34\",\"KomLinkSAMId\":\"0CA510231FCC92DD\",\"MerchantNameAddr\":null,\"TransactionDateTime\":null,\"KomLinkSeasonPassDatas\":[]}";
                        if (!string.IsNullOrEmpty(cardParam))
                        {
                            string[] responseData = cardParam.Split(',');

                            if (responseData[0] == "200")
                            {
                                scanTimer.Stop();
                                isRetry = false;
                                int commaIndex = cardParam.IndexOf(',');
                                string cardDetails = cardParam.Substring(commaIndex + 1);
                                var jsonObj = JObject.Parse(cardDetails);

                                await GetKomlinkCardFromApi(jsonObj);
                            }
                            else if (responseData[0] == "9" || responseData[0] == "10" || responseData[0] == "11")
                            {
                                CheckDataThread();
                               
                            }
                            else if (responseData[0] == "1")
                            {
                                

                                isRetry = true;
                            }
                        }
                        else
                        {
                            MessageBox.Show("System error: Empty response");
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Scan Timer Error");
            }
        }

        private async void LoadScan()
        {

            ScanWaitTimer();
            await StartIM30Reader();
           
        }
    }
}
