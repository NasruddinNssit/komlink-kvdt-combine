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
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Train.Kiosk.Common.Data.Response;
using NssIT.Train.Kiosk.Common.Common;
using NssIT.Train.Kiosk.Common.Data;

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
    /// <summary>
    /// Interaction logic for pgMyKad.xaml
    /// </summary>
    public partial class pgMyKad : Page
    {
        private const string LogChannel = "ViewPage";

        public event EventHandler<EndOfMyKadScanEventArgs> OnEndScan;

        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        private string _passengerLineNo = "1";
        private IIdentityReader _pssgIdReader = null;
        private PassengerIdentity _pssgId = null;
        private int _waitDelaySec = 20;
        private bool _stopReading = true;
        private bool _isPageUnloaded = true;

        public string _bookingId { get; private set; } = "";
        public Guid[] _tripScheduleSeatLayoutDetails_Ids { get; private set; } = null;

        public UICustInfoPNRTicketTypeAck _custPNRTicketType = null;

        public pgMyKad()
        {
            InitializeComponent();

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\CustInfo\rosCustInfoMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\CustInfo\rosCustInfoEnglish.xaml");

            _pssgIdReader = new ICPassHttpReader(@"http://localhost:1234/Para=2");

            //OnEndScan.GetInvocationList 
        }

        public void ClearEventSubscription()
        {
            if (OnEndScan != null)
            {
                Delegate[] delgList = OnEndScan.GetInvocationList();
                foreach (EventHandler<EndOfMyKadScanEventArgs> delg in delgList)
                    OnEndScan -= delg;
            }
        }

        public void InitPageData(LanguageCode language, string passengerLineNo, string bookingId, Guid[] tripScheduleSeatLayoutDetails_Ids)
        {
            _passengerLineNo = passengerLineNo;
            _language = language;
            _pssgId = null;
            _waitDelaySec = 45;
            _stopReading = true;
            _isPageUnloaded = false;
            _bookingId = bookingId;
            _custPNRTicketType = null;
            _tripScheduleSeatLayoutDetails_Ids = tripScheduleSeatLayoutDetails_Ids;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);

            if (_language == LanguageCode.Malay)
                this.Resources.MergedDictionaries.Add(_langMal);
            else
                this.Resources.MergedDictionaries.Add(_langEng);

            TxtPassengerNo.Text = _passengerLineNo ?? "*";

            TxtInsertMyKad.Visibility = Visibility.Visible;
            TxtRemoveMyKad.Visibility = Visibility.Collapsed;
            BdOK.Visibility = Visibility.Collapsed;
            BdReadLed.Visibility = Visibility.Hidden;

            StartReading(_waitDelaySec);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _isPageUnloaded = true;
            StopScanning();
        }

        public void UpdatePNRTicketTypeResult(UICustInfoPNRTicketTypeAck custPNRTicketTypeResult)
        {
            _custPNRTicketType = custPNRTicketTypeResult;
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

            _pssgId = null;
            RaiseOnEndScan(_pssgId);
        }

        private Thread _scanManThreadWorker = null;
        private void StartReading(int waitDelaySec = 10)
        {
            System.Windows.Forms.Application.DoEvents();

            _waitDelaySec = waitDelaySec;
            _pssgId = null;

            _scanManThreadWorker = new Thread(new ThreadStart(new Action(() => {
                ReadingManagerThreadWorking();
            })));
            _scanManThreadWorker.IsBackground = true;
            _scanManThreadWorker.Priority = ThreadPriority.AboveNormal;
            _scanManThreadWorker.Start();

            void ReadingManagerThreadWorking()
            {
                Thread tWorker = null;

                try
                {
                    _stopReading = false;
                    tWorker = new Thread(new ThreadStart(new Action(() => {
                        ReadIC();
                    })));
                    tWorker.IsBackground = true;
                    tWorker.Priority = ThreadPriority.AboveNormal;
                    tWorker.Start();

                    DateTime startTime = DateTime.Now;
                    DateTime endTime = startTime.AddSeconds(_waitDelaySec);

                    // Wait For IC Scanning
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

                            this.Dispatcher.Invoke(new Action(() => {
                                TxtCountDown.Text = $@"{countDown}";
                            }));
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

                    //Get Related PNR Ticket Type // _custPNRTicketType
                    if (_pssgId?.IsIDReadSuccess == true)
                    {
                        string icNo = _pssgId.IdNumber;
                        //CYA-TEST .. icNo = "441221025004";

                        App.NetClientSvc.SalesService.RequestPNRTicketType(_bookingId, icNo, _tripScheduleSeatLayoutDetails_Ids, out bool isServerResponse);

                        if (isServerResponse)
                        {
                            DateTime expiredTime = DateTime.Now.AddSeconds(10);
                            while ((_custPNRTicketType is null) && (expiredTime.Ticks >= DateTime.Now.Ticks))
                            {
                                Thread.Sleep(500);
                            }

                            if (_custPNRTicketType != null)
                            {
                                //CYA-TEST
                                //TicketTypeModel[] tickTypeList = new TicketTypeModel[1] { new TicketTypeModel() { Id = "PTO ", Description = "PTO/PESANAN TIKET ISTIMEWA", IsDefault = true } };
                                //_pssgId.SetPNR("*PNR*No*", tickTypeList);
                                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                                if ((_custPNRTicketType.MessageData is PassengerPNRTicketTypeResult pnr) && (pnr.Code?.Equals(WebAPIAgent.ApiCodeOK) == true))
                                {
                                    TicketTypeModel[] tickTypeList = (from typ in pnr.Data.PassengerTicketTypeModels
                                                                        select (new TicketTypeModel()
                                                                        {
                                                                            Id = typ.Id,
                                                                            Description = typ.Description,
                                                                            IsDefault = typ.IsDefault
                                                                        })).ToArray();
                                    _pssgId.SetPNR((string.IsNullOrWhiteSpace(pnr.Data.PNRNo) == true)? "*PNR": pnr.Data.PNRNo.Trim(), 
                                        tickTypeList);
                                }
                            }
                        }
                    }
                }
                catch (ThreadAbortException) { }
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

                if (_pssgId.IsIDReadSuccess == false)
                    RaiseOnEndScan(_pssgId);
                else
                {
                    this.Dispatcher.Invoke(new Action(() => {
                        TxtInsertMyKad.Visibility = Visibility.Collapsed;
                        TxtRemoveMyKad.Visibility = Visibility.Visible;
                        BdOK.Visibility = Visibility.Visible;
                    }));                    
                }
            }
        }

        private void Button_OK(object sender, MouseButtonEventArgs e)
        {
            //Check PNR
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
                    OnEndScan.Invoke(null, new EndOfMyKadScanEventArgs(pssgId));
                }
            }
            catch(Exception ex)
            {
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgMyKad.RaiseOnEndScan");
            }
        }

        public PassengerIdentity ReadPassengerId()
        {
            return _pssgId;
        }

        private void ReadIC()
        {
            //int minWaitSec = 1500;
            int maxWaitMilliSec = 3000;
            //int incrementWaitSec = 300;
            int waitMilliSec = maxWaitMilliSec;
            try
            {
                while (_stopReading == false)
                {
                    try
                    {
                        /////CYA-TEST .. icNo = "950620065200";
                        //Thread.Sleep(5000);
                        //_pssgId = new PassengerIdentity(true, null, "950620065200", "Debug Testing xx", new DateTime(1980, 01, 01), Gender.Male, null);
                        /////--------------------------------------------------------------

                        this.Dispatcher.Invoke(new Action(() => {
                            BdReadLed.Visibility = Visibility.Visible;
                            System.Windows.Forms.Application.DoEvents();
                        }));
                        Task.Delay(350).Wait();

                        //CYA-TEST .. uncomment for production..
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
                        this.Dispatcher.Invoke(new Action(() => {
                            BdReadLed.Visibility = Visibility.Hidden;
                            System.Windows.Forms.Application.DoEvents();
                        }));

                        //waitSec += (waitSec < maxWaitSec) ? incrementWaitSec : 0;
                        Task.Delay(waitMilliSec).Wait();
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgMyKad.ReadIC");
            }
        }

        
    }
}
