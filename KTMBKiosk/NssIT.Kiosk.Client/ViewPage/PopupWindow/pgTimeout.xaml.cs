using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Client.Base;
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

namespace NssIT.Kiosk.Client.ViewPage.PopupWindow
{
    /// <summary>
    /// Interaction logic for pgTimeout.xaml
    /// </summary>
    public partial class pgTimeout : Page, ITimeout
    {
        private SemaphoreSlim _asyncLock = new SemaphoreSlim(1);

        private string _logChannel = "ViewPage";

        private Brush _activeButtonColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFB, 0xD0, 0x12));
        private Brush _deactivateConfirmButtomColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x99, 0x99));

        private bool _allButtonEnabled = true;
        private bool _isCountingDown = false;
        private Thread _countDownThreadWorker = null;

        private TimeoutMode _timeoutMode = TimeoutMode.UIResponseTimeoutWarning;
        private LanguageCode _language = LanguageCode.Malay;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        public pgTimeout()
        {
            InitializeComponent();

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\PopupWindow\rosPopupWindowMal.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\PopupWindow\rosPopupWindowEng.xaml");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ThreadWorkerCleanUp();

            _allButtonEnabled = true;
            BdExit.Background = _activeButtonColor;
            BdContinue.Background = _activeButtonColor;

            ResourceDictionary resLang = null;
            this.Resources.MergedDictionaries.Clear();
            if (_language == LanguageCode.Malay)
                resLang = _langMal;
            else
                resLang = _langEng;
            this.Resources.MergedDictionaries.Add(resLang);

            if (_timeoutMode == TimeoutMode.BookingTimeoutAck)
            {
                TxtTimeoutMessage.Text = resLang["BOOKING_TIMEOUT_Label"].ToString();
                BdContinue.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtTimeoutMessage.Text = resLang["TIMEOUT_IN_Label"].ToString();
                BdContinue.Visibility = Visibility.Visible;
            }

            if (_asyncLock.CurrentCount == 0)
                _asyncLock.Release();

            _isCountingDown = true;
            _countDownThreadWorker = new Thread(new ThreadStart(CountDownThreadWorking));
            _countDownThreadWorker.IsBackground = true;
            _countDownThreadWorker.Start();
        }


        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _isCountingDown = false;
        }

        private void BdExit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                e.Handled = true;

                _asyncLock.Wait();

                if (_allButtonEnabled == true)
                {
                    _isCountingDown = false;
                    _allButtonEnabled = false;

                    BdExit.Background = _deactivateConfirmButtomColor;
                    BdContinue.Background = _deactivateConfirmButtomColor;

                    ExitCurrentTransaction();
                }
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message};", ex), "EX01", "uscLegend.BdExit_MouseLeftButtonDown");
            }
            finally
            {
                if (_asyncLock.CurrentCount == 0)
                    _asyncLock.Release();
            }
        }

        private void BdContinue_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                e.Handled = true;

                _asyncLock.Wait();

                if (_allButtonEnabled == true)
                {
                    _isCountingDown = false;
                    _allButtonEnabled = false;

                    BdExit.Background = _deactivateConfirmButtomColor;
                    BdContinue.Background = _deactivateConfirmButtomColor;

                    ContinueCurrentTransaction();
                }                                
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message};", ex), "EX01", "uscLegend.BdContinue_MouseLeftButtonDown");
            }
            finally
            {
                if (_asyncLock.CurrentCount == 0)
                    _asyncLock.Release();
            }
        }

        private void CountDownThreadWorking()
        {
            int countDown = 30;

            if (_timeoutMode == TimeoutMode.BookingTimeoutAck)
                countDown = 15;

            while ((_isCountingDown == true) && (countDown >= -1))
            {
                try
                {
                    this.Dispatcher.Invoke(new Action(() => {
                        TxtCountDown.Text = countDown.ToString();
                    }));
                    Thread.Sleep(1000);
                    countDown--;

                    if (countDown == 1)
                    {
                        // Disable all button
                        _allButtonEnabled = false;

                        this.Dispatcher.Invoke(new Action(() => {
                            BdExit.Background = _deactivateConfirmButtomColor;
                            BdContinue.Background = _deactivateConfirmButtomColor;
                        }));

                    }
                    else if (countDown == 0)
                    {
                        //Exit Session Transaction
                        ExitCurrentTransaction();
                    }
                }
                catch (Exception ex)
                {
                    App.Log.LogError(_logChannel, "", ex, "EX01", "pgTimeout.CountDownThreadWorking");
                }                
            }

            _isCountingDown = false;
            _countDownThreadWorker = null;
        }

        private void ThreadWorkerCleanUp()
        {
            if (_countDownThreadWorker != null)
            {
                try
                {
                    if (((_countDownThreadWorker?.ThreadState & ThreadState.Stopped) == ThreadState.Stopped)
                    || ((_countDownThreadWorker?.ThreadState & ThreadState.Aborted) == ThreadState.Aborted)
                    || ((_countDownThreadWorker?.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted)
                    )
                    { /* BY Pass */ }
                    else
                    {
                        try
                        {
                            _countDownThreadWorker?.Abort();

                            if (_countDownThreadWorker != null)
                                Thread.Sleep(10);
                        }
                        catch { }
                    }
                }
                catch { /* BY Pass */ }
                finally
                {
                    _countDownThreadWorker = null;
                }
            }
        }

        private void ExitCurrentTransaction()
        {
            Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
                try
                {
                    App.BookingTimeoutMan.ResetCounter();
                    App.NetClientSvc.SalesService.NavigateToPage(AppDecorator.Common.AppService.Sales.PageNavigateDirection.Exit);
                }
                catch (Exception ex)
                {
                    App.Log.LogError(_logChannel, "", ex, "EX01", "pgTimeout.ExitCurrentTransaction");
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000361)");
                }
            })));
            submitWorker.IsBackground = true;
            submitWorker.Start();
        }

        private void ContinueCurrentTransaction()
        {
            Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
                try
                {
                    App.TimeoutManager.ResetTimeout();
                    App.MainScreenControl.UnLoadTimeoutNoticePage();
                }
                catch (Exception ex)
                {
                    App.Log.LogError(_logChannel, "", ex, "EX01", "pgTimeout.ContinueCurrentTransaction");
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000362)");
                }
            })));
            submitWorker.IsBackground = true;
            submitWorker.Start();
        }

        public void InitPageData(UISalesTimeoutWarningAck uiTimeoutWarn, LanguageCode? language)
        {
            if (language.HasValue == true)
                _language = language.Value;
            else
                _language = LanguageCode.Malay;

            _timeoutMode = uiTimeoutWarn.ModeOfTimeout;
        }
    }
}
