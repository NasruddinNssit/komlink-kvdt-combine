using Newtonsoft.Json;
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

namespace MyKadTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IIdentityReader _pssgIdReader = null;
        private PassengerIdentity _pssgId = null;
        private bool _autoReadMyKad_stopReading = true;
        private int _waitDelaySec = 20;
        private bool _isPageUnloaded = false;

        private LibShowMessageWindow.MessageWindow _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;

        public MainWindow()
        {
            InitializeComponent();

            _pssgIdReader = new ICPassHttpReader(@"http://localhost:1234/Para=2");
        }
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            _isPageUnloaded = true;
            _autoReadMyKad_stopReading = true;
        }

        private void ReadMyKad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _msg.ShowMessage($@"Start -- ReadMyKad_Click");
                _pssgId = _pssgIdReader.ReadIC(waitDelaySec: 20);
                _msg.ShowMessage($@"End -- ReadMyKad_Click");

                if (_pssgId != null)
                {
                    _msg.ShowMessage($@"{"\r\n"}MyKad Info{"\r\n"}=============================={"\r\n"}{JsonConvert.SerializeObject(_pssgId, Formatting.Indented)}");
                }
                else
                {
                    _msg.ShowMessage($@"No MyKad Info found");
                }
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void StartReading_Click(object sender, RoutedEventArgs e)
        {
            if (rbtStartReading.IsChecked == true)
            {
                _pssgId = null;
                _waitDelaySec = 45;
                btnReadMyKad.IsEnabled = false;

                StartReading(_waitDelaySec);
            }
        }

        private void EndReading_Click(object sender, RoutedEventArgs e)
        {
            EndReading();
        }

        private void EndReading()
        {
            this.Dispatcher.Invoke(() => {
                if (rbtEndReading.IsChecked == true)
                {
                    _autoReadMyKad_stopReading = true;
                    btnReadMyKad.IsEnabled = true;
                }
            });
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
                    _autoReadMyKad_stopReading = false;
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
                    while ((countDown > 0) && (_isPageUnloaded == false) && (_autoReadMyKad_stopReading == false))
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

                            //this.Dispatcher.Invoke(new Action(() => {
                            //    TxtCountDown.Text = $@"{countDown}";
                            //}));
                            Task.Delay(1000).Wait();
                        }
                        else
                        {
                            _autoReadMyKad_stopReading = true;
                        }
                        //countDown--;
                    }

                    _autoReadMyKad_stopReading = true;

                    if (_pssgId is null)
                    {
                        Task.Delay(500).Wait();
                        //_pssgId = new PassengerIdentity(false, null, null, null, null, Gender.Female, "IC not found (IV); ");
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

                    if (_pssgId != null)
                    {
                        _msg.ShowMessage($@"{"\r\n"}MyKad Info{"\r\n"}=============================={"\r\n"}{JsonConvert.SerializeObject(_pssgId, Formatting.Indented)}");
                    }
                    else
                    {
                        _msg.ShowMessage($@"No MyKad Info found");
                    }
                    EndReading();
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    //App.Log.LogError(LogChannel, "-", ex, "EX01", "pgMyKad.ReadingManagerThreadWorking");
                    _msg.ShowMessage(ex.ToString());
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
            }
        }

        private void ReadIC()
        {
            //int minWaitSec = 1500;
            int maxWaitSec = 3000;
            //int incrementWaitSec = 300;
            int waitSec = maxWaitSec;
            try
            {
                while (_autoReadMyKad_stopReading == false)
                {
                    try
                    {
                        /////CYA-TEST .. icNo = "950620065200";
                        //Thread.Sleep(5000);
                        //_pssgId = new PassengerIdentity(true, null, "950620065200", "Debug Testing xx", new DateTime(1980, 01, 01), Gender.Male, null);
                        /////--------------------------------------------------------------

                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            BdReadLED.Visibility = Visibility.Visible;
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
                        _autoReadMyKad_stopReading = true;
                        break;
                    }
                    else
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            BdReadLED.Visibility = Visibility.Hidden;
                            System.Windows.Forms.Application.DoEvents();
                        }));

                        //waitSec += (waitSec < maxWaitSec) ? incrementWaitSec : 0;
                        Task.Delay(waitSec).Wait();
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                //App.Log.LogError(LogChannel, "-", ex, "EX01", "pgMyKad.ReadIC");
                _msg.ShowMessage(ex.ToString());
            }
        }

        
    }
}
