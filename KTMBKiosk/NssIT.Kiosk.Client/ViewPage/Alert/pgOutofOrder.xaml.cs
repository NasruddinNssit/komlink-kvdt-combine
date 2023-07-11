using NssIT.Kiosk.Log.DB.StatusMonitor;
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

namespace NssIT.Kiosk.Client.ViewPage.Alert
{
    /// <summary>
    /// Interaction logic for pgOutofOrder.xaml
    /// </summary>
    public partial class pgOutofOrder : Page, IAlertPage
    {
        private const string LogChannel = "ViewPage";
        private string _lastDetailMsg = null;

        public pgOutofOrder()
        {
            InitializeComponent();
        }

        public void ShowAlertMessage(string malayShortMsg = "TIDAK BERFUNGSI", string engShortMsg = "OUT OF ORDER", string detailMsg = "")
        {

            TrimPrinterErrorMsg(detailMsg, out string trimMsg, out bool isPrinterError);

            this.Dispatcher.Invoke(new Action(() => {
                TxtMalMsg.Text = malayShortMsg;
                TxtEngMsg.Text = engShortMsg;
                TxtTimeStr.Text = DateTime.Now.ToString("yyyyMMdd-HHmmss-fffff");
                TxtProblemMsg.Text = trimMsg;
            }));

            if (_lastDetailMsg?.Trim().Equals(trimMsg ?? "") == true)
            { /*By Pass*/ }
            else if (trimMsg != null)
                App.Log.LogText(LogChannel, "*", $@"{malayShortMsg} :: {engShortMsg} :: {trimMsg}", "A01", "pgOutofOrder.ShowAlertMessage");

            _lastDetailMsg = trimMsg;

            if (isPrinterError)
            {
                App.ShowDebugMsg("Exec. TowerLight.ShowErrorStateWithBlinking .. ********______********______*********");
                App.TowerLight.ShowErrorStateWithBlinking();
                App.Log.LogText(LogChannel, "*", "Running .. TowerLight.ShowErrorStateWithBlinking", "DEBUG01", "pgOutofOrder.ShowAlertMessage", AppDecorator.Log.MessageType.Debug);
            }
            else
            {
                App.ShowDebugMsg("Exec. TowerLight.ShowErrorState");
                App.TowerLight.ShowErrorState();
                App.Log.LogText(LogChannel, "*", "Running .. TowerLight.ShowErrorState", "DEBUG02", "pgOutofOrder.ShowAlertMessage", AppDecorator.Log.MessageType.Debug);
            }

            StatusHub.GetStatusHub().zNewStatus_IsUIDisplayNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, _lastDetailMsg);

            return;

            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void TrimPrinterErrorMsg(string inMsg, out string trimMsgX, out bool isPrinterErrorX)
            {
                trimMsgX = inMsg;
                isPrinterErrorX = false;

                if (trimMsgX?.IndexOf(AppHelper.PrinterErrorTag) >= 0)
                {
                    isPrinterErrorX = true;
                    string rTag = AppHelper.PrinterErrorTag + ";";
                    if (trimMsgX?.IndexOf(rTag) >= 0)
                    {
                        trimMsgX = trimMsgX.Replace(rTag, "");
                    }

                    rTag = AppHelper.PrinterErrorTag + " ;";
                    if (trimMsgX?.IndexOf(rTag) >= 0)
                    {
                        trimMsgX = trimMsgX.Replace(rTag, "");
                    }

                    rTag = AppHelper.PrinterErrorTag;
                    if (trimMsgX?.IndexOf(rTag) >= 0)
                    {
                        trimMsgX = trimMsgX.Replace(rTag, "");
                    }
                }
            }
        }

        private Thread _systemHealthCheckThreadWorker = null;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // SystemHelper
            _lastDetailMsg = null;
            string svrVer = App.AppHelp.ServerApplicationVersion;
            string cliVer = App.SystemVersion;

            TxtSysVer.Text = $@"{cliVer} / {svrVer}";
            TxtStartTimeStr.Text = DateTime.Now.ToString("yyyyMMdd-HHmmss");

            _systemHealthCheckThreadWorker = new Thread(new ThreadStart(SystemHealthCheckThreadWorking));
            _systemHealthCheckThreadWorker.IsBackground = true;
            _systemHealthCheckThreadWorker.Start();

            App.Log.LogText(LogChannel, "*", $@"Problem : {TxtProblemMsg.Text ?? "-"}", "A01", "pgOutofOrder.Page_Loaded", 
                adminMsg: $@"Problem : {TxtProblemMsg.Text ?? "-"}");
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if ((_systemHealthCheckThreadWorker != null) && ((_systemHealthCheckThreadWorker.ThreadState & ThreadState.Stopped) != ThreadState.Stopped))
            {
                try
                {
                    _systemHealthCheckThreadWorker.Abort();
                }
                catch { }
                finally
                {
                    _lastDetailMsg = null;
                    _systemHealthCheckThreadWorker = null;
                }
            }
        }

        private void SystemHealthCheckThreadWorking()
        {
            try
            {
                bool threadAbort = false;
                App.IsLocalServerReady = false;

                Thread.Sleep(1000 * 15);

                do
                {
                    try
                    {
                        if (App.AppHelp.SystemHealthCheck())
                        {
                            App.IsLocalServerReady = true;
                        }
                        else
                            throw new Exception("Sales Server has unknown error; (EXIT10000061)");
                    }
                    catch (ThreadAbortException) { threadAbort = true; }
                    catch (Exception ex)
                    {
                        App.Log.LogError(LogChannel, "-", ex, "EX01", "pgOutofOrder.SystemHealthCheckThreadWorking");
                        ShowAlertMessage(detailMsg: ex.Message);
                        Thread.Sleep(5000);
                    }
                } while (App.IsLocalServerReady == false);

                if ((threadAbort == false) && (App.IsLocalServerReady))
                {
                    App.MainScreenControl.ShowWelcome();
                }
            }
            catch (ThreadAbortException ex)
            {
                App.ShowDebugMsg("Thread aborted; At pgOutofOrder.SystemHealthCheckThreadWorking");
                App.Log.LogError(LogChannel, "-", ex, "EX02", "pgOutofOrder.SystemHealthCheckThreadWorking");
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", ex, "EX03", "pgOutofOrder.SystemHealthCheckThreadWorking");
            }
        }
    }
}
