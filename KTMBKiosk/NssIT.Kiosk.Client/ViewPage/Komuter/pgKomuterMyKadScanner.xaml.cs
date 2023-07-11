using System;
using System.Collections.Generic;
using System.Linq;
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

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using NssIT.Kiosk.Client.Base.LocalDevices;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.AppDecorator.Common;

namespace NssIT.Kiosk.Client.ViewPage.Komuter
{
    /// <summary>
    /// Interaction logic for pgKomuterMyKadScanner.xaml
    /// </summary>
    public partial class pgKomuterMyKadScanner : Page
    {
        /// <summary>
        /// Return false when fail verification. 
        /// </summary>
        /// <param name="pssgId"></param>
        /// <param name="errorMsg">Return an error message when fail verification. This Error message will show on pgMyKad</param>
        /// <returns></returns>
        public delegate bool MyKadAppVerificationDelg(PassengerIdentity pssgId, out string errorMsg);

        public delegate void GetRemoveCardMessageOnSuccessDelg(out string removeCardMessage1, out string removeCardMessage2);

        private const string LogChannel = "ViewPage";

        public event EventHandler<CustInfo.EndOfMyKadScanEventArgs> OnEndScan;

        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;
        private ResourceDictionary _currentLangResource = null;

        private string _passengerLineNo = "1";
        private IIdentityReader _pssgIdReader = null;
        private PassengerIdentity _pssgId = null;
        private int _waitDelaySec = 20;
        private bool _stopReading = true;
        private bool _isPageUnloaded = true;

        private MyKadAppVerificationDelg _myKadAppVerificationDelgHandle = null;
        private GetRemoveCardMessageOnSuccessDelg _getRemoveCardMessageOnSuccessDelgHandle = null;

        private Resetor _resetor = new Resetor(); 
        private string _jobTitle = null;
        public pgKomuterMyKadScanner()
        {
            InitializeComponent();

            _resetor.Init();
            _langMal = CommonFunc.GetXamlResource(@"ViewPage\Komuter\rosKomuterMal.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\Komuter\rosKomuterEng.xaml");
            _currentLangResource = _langEng;

            _pssgIdReader = new ICPassHttpReader(@"http://localhost:1234/Para=2");

            //OnEndScan.GetInvocationList 
        }

        public void ClearEventSubscription()
        {
            if (OnEndScan != null)
            {
                Delegate[] delgList = OnEndScan.GetInvocationList();
                foreach (EventHandler<CustInfo.EndOfMyKadScanEventArgs> delg in delgList)
                    OnEndScan -= delg;
            }
        }

        public void InitPageData(LanguageCode language, string passengerLineNo,
            MyKadAppVerificationDelg appVerificationHandle = null, GetRemoveCardMessageOnSuccessDelg getRemoveCardMessageOnSuccessDelgHandle = null,
            string jobTitle = null)
        {
            _passengerLineNo = passengerLineNo;
            _language = language;
            _myKadAppVerificationDelgHandle = appVerificationHandle;
            _getRemoveCardMessageOnSuccessDelgHandle = getRemoveCardMessageOnSuccessDelgHandle;
            _jobTitle = string.IsNullOrWhiteSpace(jobTitle) ? null : jobTitle.Trim();
            _pssgId = null;
            _waitDelaySec = 30;
            _stopReading = true;
            _isPageUnloaded = false;

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);

            if (_language == LanguageCode.Malay)
                _currentLangResource = _langMal;
            else
                _currentLangResource = _langEng;

            if (string.IsNullOrWhiteSpace(_jobTitle))
            {
                TxtTitle.Text = "";
                TxtTitle.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtTitle.Text = _jobTitle;
                TxtTitle.Visibility = Visibility.Visible;
            }

            TxtPassengerNo.Text = _passengerLineNo ?? "*";

            this.Resources.MergedDictionaries.Clear();
            this.Resources.MergedDictionaries.Add(_currentLangResource);

            TxtScanTag.Text = _currentLangResource["SCAN_PASSENGER_MYKAD_Label"].ToString();
            TxtInsertMyKad.Text = _currentLangResource["INSERT_MYKAD_FOR_SCAN_Label"].ToString();

            TxtErrorMsg.Visibility = Visibility.Collapsed;
            TxtInsertMyKad.Visibility = Visibility.Visible;
            TxtRemoveMyKad.Visibility = Visibility.Collapsed;
            BdOK.Visibility = Visibility.Collapsed;

            StartReading(_waitDelaySec);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _isPageUnloaded = true;
            StopScanning();
        }

        private void StopScanning()
        {
            _stopReading = true;

            if (_scanManThreadWorker != null)
            {
                if ((_scanManThreadWorker.ThreadState & ThreadState.Stopped) != ThreadState.Stopped)
                {
                    try
                    {
                        _scanManThreadWorker.Abort();
                        Task.Delay(300).Wait();
                    }
                    catch { }

                }
                _scanManThreadWorker = null;
            }
        }

        private void Button_Cancel(object sender, MouseButtonEventArgs e)
        {
            StopScanning();

            App.TimeoutManager.ResetTimeout();

            _pssgId = null;
            RaiseOnEndScan(_pssgId);
        }

        private Thread _scanManThreadWorker = null;
        private void StartReading(int waitDelaySec = 10)
        {
            System.Windows.Forms.Application.DoEvents();

            _waitDelaySec = waitDelaySec;
            _pssgId = null;

            bool workAborted = false;
            _scanManThreadWorker = new Thread(new ThreadStart(new Action(() => {
                ReadingManagerThreadWorking();
            })));
            _scanManThreadWorker.IsBackground = true;
            _scanManThreadWorker.Priority = ThreadPriority.AboveNormal;
            _scanManThreadWorker.Start();

            void ReadingManagerThreadWorking()
            {
                App.TimeoutManager.ResetTimeout();

                Thread tWorker = null;

                try
                {
                    _stopReading = false;
                    tWorker = new Thread(new ThreadStart(new Action(() => {
                        ReadICThreadWorking();
                    })));
                    tWorker.IsBackground = true;
                    tWorker.Priority = ThreadPriority.AboveNormal;
                    tWorker.Start();

                    DateTime startTime = DateTime.Now;
                    DateTime endTime = startTime.AddSeconds(_waitDelaySec);

                    int countDown = _waitDelaySec - 1;
                    while ((countDown > 0) && (_isPageUnloaded == false) && (_stopReading == false))
                    {
                        PassengerIdentity passgId = _pssgId;

                        if ((passgId == null) || (passgId.IsIDReadSuccess == false))
                        {
                            //if (passgId?.IsIDReadSuccess == false)
                            //{
                            //    this.Dispatcher.Invoke(new Action(() => {
                            //        txtMsg.Text = passgId.Message;
                            //    }));
                            //}

                            if (_resetor.ResetRequested == false)
                            {
                                this.Dispatcher.Invoke(new Action(() => {
                                    TxtCountDown.Text = $@"{countDown}";
                                }));
                            }

                            Task.Delay(1000).Wait();
                        }
                        countDown--;
                    }

                    _stopReading = true;

                    if (_pssgId is null)
                    {
                        Task.Delay(500).Wait();
                        _pssgId = new PassengerIdentity(false, null, null, null, null, Gender.Female, "IC not found (IV); ");
                    }

                    if ((tWorker.ThreadState & ThreadState.Stopped) != ThreadState.Stopped)
                    {
                        try
                        {
                            tWorker.Abort();
                        }
                        catch (Exception) { }
                        Task.Delay(500).Wait();

                        tWorker = null;
                    }
                }
                catch (ThreadAbortException)
                {
                    workAborted = true;
                    if ((tWorker?.ThreadState & ThreadState.Stopped) != ThreadState.Stopped)
                    {
                        try
                        {
                            tWorker?.Abort();
                        }
                        catch (Exception) { }
                        Task.Delay(500).Wait();

                        tWorker = null;
                    }
                }
                catch (Exception ex)
                {
                    App.Log.LogError(LogChannel, "-", ex, "EX01", "pgMyKad.ReadingManagerThreadWorking");
                }
                finally
                {
                    if (tWorker != null)
                    {
                        if ((tWorker.ThreadState & ThreadState.Stopped) != ThreadState.Stopped)
                        {
                            try
                            {
                                tWorker.Abort();
                                Task.Delay(100).Wait();
                            }
                            catch { }
                        }
                        tWorker = null;
                    }
                }

                if (workAborted == false)
                {
                    if (_pssgId.IsIDReadSuccess == false)
                        RaiseOnEndScan(_pssgId);
                    else
                    {
                        bool proceedToNextStepAfterSuccess = true;

                        if ((_myKadAppVerificationDelgHandle != null) && (_pssgId.IsIDReadSuccess))
                        {
                            if (_myKadAppVerificationDelgHandle(_pssgId, out string errorMsg) == false)
                            {
                                _resetor.RequestToReset();

                                proceedToNextStepAfterSuccess = false;
                                string errMsg = string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when verifing MyKad." : errorMsg.Trim();

                                this.Dispatcher.Invoke(new Action(() => {
                                    TxtErrorMsg.Visibility = Visibility.Visible;
                                    TxtInsertMyKad.Visibility = Visibility.Collapsed;
                                    TxtRemoveMyKad.Visibility = Visibility.Collapsed;
                                    BdOK.Visibility = Visibility.Collapsed;
                                    TxtErrorMsg.Text = errMsg;
                                    System.Windows.Forms.Application.DoEvents();
                                }));

                                if ((_isPageUnloaded == false) && (workAborted == false))
                                {
                                    _pssgId = null;
                                    _scanManThreadWorker = new Thread(new ThreadStart(new Action(() => {
                                        ReadingManagerThreadWorking();
                                    })));
                                    _scanManThreadWorker.IsBackground = true;
                                    _scanManThreadWorker.Priority = ThreadPriority.AboveNormal;
                                    _scanManThreadWorker.Start();
                                }
                            }
                            else
                            {
                                //_getRemoveCardMessageOnSuccessDelgHandle
                                if (_getRemoveCardMessageOnSuccessDelgHandle != null)
                                {
                                    _getRemoveCardMessageOnSuccessDelgHandle(out string removeCardMessage1, out string removeCardMessage2);

                                    this.Dispatcher.Invoke(new Action(() =>
                                    {
                                        TxtRemoveMyKad1.Text = "";
                                        TxtRemoveMyKad2.Text = "";

                                        if (string.IsNullOrWhiteSpace(removeCardMessage1) == false)
                                            TxtRemoveMyKad1.Text = removeCardMessage1;

                                        if (string.IsNullOrWhiteSpace(removeCardMessage2) == false)
                                            TxtRemoveMyKad2.Text = removeCardMessage2;
                                    }));

                                }
                            }
                        }

                        if (proceedToNextStepAfterSuccess)
                            this.Dispatcher.Invoke(new Action(() => {
                                TxtErrorMsg.Visibility = Visibility.Collapsed;
                                TxtInsertMyKad.Visibility = Visibility.Collapsed;
                                TxtRemoveMyKad.Visibility = Visibility.Visible;
                                BdOK.Visibility = Visibility.Visible;
                                System.Windows.Forms.Application.DoEvents();
                            }));
                    }
                }
            }
        }

        private void Button_OK(object sender, MouseButtonEventArgs e)
        {
            App.TimeoutManager.ResetTimeout();

            RaiseOnEndScan(_pssgId);
        }

        private void RaiseOnEndScan(PassengerIdentity pssgId)
        {
            if (_isPageUnloaded)
                return;

            try
            {
                if (OnEndScan != null)
                {
                    OnEndScan.Invoke(null, new CustInfo.EndOfMyKadScanEventArgs(pssgId));
                }
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgMyKad.RaiseOnEndScan");
            }
        }

        public PassengerIdentity ReadPassengerId()
        {
            return _pssgId;
        }

        private void ReadICThreadWorking()
        {
            try
            {
                while (_stopReading == false)
                {
                    try
                    {
                        _pssgId = _pssgIdReader.ReadIC(waitDelaySec: (_waitDelaySec));
                    }
                    catch (Exception ex)
                    {
                        _pssgId = new PassengerIdentity(false, null, null, null, null, Gender.Female, "Error when IC reading (III); " + ex.Message);
                    }

                    if (_pssgId?.IsIDReadSuccess == true)
                    {
                        _stopReading = true;
                        break;
                    }
                    else
                    {
                        if (_resetor.ResetRequested)
                        {
                            this.Dispatcher.Invoke(new Action(() => {
                                TxtErrorMsg.Visibility = Visibility.Collapsed;
                                TxtInsertMyKad.Visibility = Visibility.Visible;
                                TxtRemoveMyKad.Visibility = Visibility.Collapsed;
                            }));
                            _resetor.Init();
                        }
                    
                        Task.Delay(800).Wait();
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgMyKad.ReadIC");
            }
        }


        /// <summary>
        /// Used to reset MyKad Entry
        /// </summary>
        class Resetor
        {
            private bool _requestToReset = false;
            public bool ResetRequested => _requestToReset;

            public void RequestToReset()
            {
                _requestToReset = true;
            }

            public void Init()
            {
                _requestToReset = false;
            }
        }
    }
}
