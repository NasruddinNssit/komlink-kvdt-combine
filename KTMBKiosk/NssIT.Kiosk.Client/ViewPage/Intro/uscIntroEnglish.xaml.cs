using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Config.ConfigConstant;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Client.Kvdt.Services;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NssIT.Kiosk.Client.ViewPage.Intro
{
    public partial class uscIntroEnglish : UserControl
    {
        public APIServices aPIServices = new APIServices(new APIURLServices(), new APISignatureServices());

        private const string LogChannel = "ViewPage";
        private const string _komuterTag = "KOMUTER";

        public event EventHandler<StartRunningEventArgs> OnBegin;

        private Style _buttonEnabledStyle = null;
        private Style _buttonDisabledStyle = null;

        private bool _buttonEnabled = true;

        private string _clientVersion = null;
        private string _serverVersion = "-";

        public uscIntroEnglish()
        {
            InitializeComponent();

            _buttonEnabledStyle = this.FindResource("GreenButton") as Style;
            _buttonDisabledStyle = this.FindResource("DisabledButton") as Style;

            _clientVersion = App.SystemVersion;
            TxtSysVer.Text = (_clientVersion ?? "*") + " / " + App.NextCardSettlementTimeString;

            //DisplayKvdtAndKomlinkButtonAsync();
        }

        public void UpdateServerVersionTag(string serverAppVersion)
        {
            NssIT.Kiosk.AppDecorator.Config.Setting sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

            this.Dispatcher.Invoke(new Action(() =>
            {
                _serverVersion = serverAppVersion ?? "*";
                TxtSysVer.Text = $@"{_clientVersion} / {_serverVersion} / {App.NextCardSettlementTimeString}";

                sysSetting.GetWebApiSummary(out bool isLiveWebApi, out string webApiTag, out WebAPISiteCode webAPISiteCode);

                if (isLiveWebApi)
                {
                    StkTesting.Visibility = Visibility.Collapsed;
                }
                else
                {
                    TxtTestingSideName.Text = webApiTag ?? "A Testing Side";
                    StkTesting.Visibility = Visibility.Visible;
                    App.WebAPICode = webAPISiteCode;
                }
            }));
        }
        private void BtnBegin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                e.Handled = true;

                if (!_buttonEnabled)
                    return;

                if (sender is Button btn)
                {
                    if ((btn.Tag is string tag) && (tag.Equals(_komuterTag)))
                    {
                        App.CurrentTransStage = TicketTransactionStage.Komuter;
                        RaiseOnBegin(TransportGroup.Komuter, btn);
                        BtnStart3.IsEnabled = false;
                        BtnStart4.IsEnabled = false;
                    }
                    else
                    {
                        App.CurrentTransStage = TicketTransactionStage.ETS;
                        RaiseOnBegin(TransportGroup.EtsIntercity, btn);
                        BtnStart3.IsEnabled = false;
                        BtnStart4.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", ex, "EX01", "uscIntroEnglish.BtnBegin_Click");
            }
        }

        private void RaiseOnBegin(TransportGroup vehicleGroup, Button callerButton)
        {
            if (OnBegin != null)
            {
                OnBegin.Invoke(this, new StartRunningEventArgs(vehicleGroup, callerButton));
            }
        }

        public void SetStartButtonEnabled(bool enabled, Button callerButton)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                _buttonEnabled = enabled;

                if (_buttonEnabled)
                {
                    if ((callerButton is null) || callerButton.Name.Equals(BtnStart.Name))
                    {
                        BtnStart.Style = _buttonEnabledStyle;
                        TxtShowNormal.Visibility = Visibility.Visible;
                        TxtDisabled.Visibility = Visibility.Collapsed;

                        //BtnStart3.IsEnabled = true;
                        //BtnStart4.IsEnabled = true;
                    }

                    if ((callerButton is null) || callerButton.Name.Equals(BtnStart2.Name))
                    {
                        BtnStart2.Style = _buttonEnabledStyle;
                        TxtShowNormal2.Visibility = Visibility.Visible;
                        TxtDisabled2.Visibility = Visibility.Collapsed;

                        //BtnStart3.IsEnabled = true;
                        //BtnStart4.IsEnabled = true;
                    }
                }
                else
                {
                    if ((callerButton is null) || callerButton.Name.Equals(BtnStart.Name))
                    {
                        BtnStart.Style = _buttonDisabledStyle;
                        TxtShowNormal.Visibility = Visibility.Collapsed;
                        TxtDisabled.Visibility = Visibility.Visible;

                        //BtnStart3.IsEnabled = false;
                        //BtnStart4.IsEnabled = false;
                    }

                    if ((callerButton is null) || callerButton.Name.Equals(BtnStart2.Name))
                    {
                        BtnStart2.Style = _buttonDisabledStyle;
                        TxtShowNormal2.Visibility = Visibility.Collapsed;
                        TxtDisabled2.Visibility = Visibility.Visible;

                        //BtnStart3.IsEnabled = false;
                        //BtnStart4.IsEnabled = false;
                    }
                }
            }));
        }

        public void ShowSalesButton(bool isETSIntercityValid, bool isKomuterValid)
        {
            if (isETSIntercityValid)
                BtnStart.Visibility = Visibility.Visible;
            else
                BtnStart.Visibility = Visibility.Collapsed;

            if (isKomuterValid)
                BtnStart2.Visibility = Visibility.Visible;
            else
                BtnStart2.Visibility = Visibility.Collapsed;
        }

        private void BtnReqMaintenance_Click(object sender, RoutedEventArgs e)
        {
            Thread submitWorker = new Thread(new ThreadStart(new Action(() =>
            {
                try
                {
                    App.NetClientSvc.SalesService.RequestMaintenance(out bool isServerResponded);

                    if (isServerResponded == false)
                        App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000045B)");
                }
                catch (Exception ex)
                {
                    // App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000046)");
                    App.Log.LogError(LogChannel, "", new Exception("(EXIT10000046B)", ex), "EX01", "uscIntroEnglish.BtnReqMaintenance_Click");
                }
            })));
            submitWorker.IsBackground = true;
            submitWorker.Start();
        }

        private void BtnStart3_Click(object sender, RoutedEventArgs e)
        {
            BtnStart.IsEnabled = false;
            BtnStart2.IsEnabled = false;
            BtnStart4.IsEnabled = false;

            Process[] pname = Process.GetProcessesByName("kvdt-kiosk");

            if (pname.Length == 0)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = @"C:\NssITKiosk\kvdt-kiosk\kvdt-kiosk.exe";

                Process.Start(startInfo);
            }
        }

        private void BtnStart4_Click(object sender, RoutedEventArgs e)
        {
            BtnStart.IsEnabled = false;
            BtnStart2.IsEnabled = false;
            BtnStart3.IsEnabled = false;

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"C:\NssITKiosk\Komlink\Komlink.exe";

            Process.Start(startInfo);
        }

        private async void DisplayKvdtAndKomlinkButtonAsync()
        {

            var kioskId = await Task.Run(() => System.IO.File.ReadAllText(@"C:\NssITKiosk\LocalServer\Parameter.txt"));

            var kioskIdSplit = kioskId.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            var kioskIdValue = kioskIdSplit[0].Split('=')[1];

            var response = await aPIServices.GetFCServiceByCounter(kioskId);

            if (response == null)
            {
                BtnStart3.Visibility = Visibility.Collapsed;
                BtnStart4.Visibility = Visibility.Collapsed;
            }
            else
            {
                BtnStart3.Visibility = Visibility.Visible;
                BtnStart4.Visibility = Visibility.Visible;
            }
        }
    }
}