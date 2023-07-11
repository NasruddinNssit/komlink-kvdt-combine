using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult;
using NssIT.Kiosk.Tools.ThreadMonitor;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.GetDeviceInfo;
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
using NssIT.Kiosk.AppDecorator.Log;

namespace IM30TestAPI01
{
    /// <summary>
    /// Interaction logic for uscGetDeviceInfo.xaml
    /// </summary>
    public partial class uscGetDeviceInfo : UserControl
    {
        private const string _logChannel = "TestUI";

        private LibShowMessageWindow.MessageWindow _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;
        private ShowMessageLogDelg _showMessageDelgHandler = null;

        private string _comPort = null;
        public uscGetDeviceInfo()
        {
            InitializeComponent();
            _showMessageDelgHandler = new ShowMessageLogDelg(ShowMessageDelgWorking);
        }

        private void ShowMessageDelgWorking(string message)
        {
            _msg.ShowMessage(message);
        }

        public void SetCOMPort(string comPort)
        {
            _comPort = comPort;
        }

        IM30GetDeviceInfo _im30Trans = null;
        private void IM30GetDeviceInfo_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (string.IsNullOrWhiteSpace(_comPort))
            {
                _msg.ShowMessage("Please select and apply a COM port.");
                return;
            }

            Guid processId = Guid.NewGuid();

            GetDeviceInfoTest01();

            return;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            ///
            void GetDeviceInfoTest01()
            {
                try
                {
                    if (_im30Trans?.IsCurrentWorkingEnded == false)
                    {
                        _msg.ShowMessage("Dispose existing Get Device Info transaction .. Please wait 5 seconds");
                        _im30Trans.Dispose();

                        Thread.Sleep(5 * 1000);
                    }

                    _im30Trans = new IM30GetDeviceInfo(_comPort, processId.ToString(), new OnTransactionFinishedDelg(OnTransactionFinishedDelgWorking)
                        , noActionMaxWaitSec: 20, _showMessageDelgHandler);
                    _im30Trans.StartTransaction(out Exception ex2);
                    if (ex2 != null)
                        throw new Exception("Error when run GetDeviceInfoTest01; (A)", ex2);
                }
                catch (Exception ex)
                {
                    _msg.ShowMessage(ex.ToString());
                }
            }

            void OnTransactionFinishedDelgWorking(IIM30TransResult transResult)
            {
                if (_im30Trans == null)
                    return;

                RunThreadMan tMan = new RunThreadMan(new Action(() =>
                {
                    try
                    {
                        if (_im30Trans.FinalResult != null)
                        {
                            if (_im30Trans.FinalResult.IsSuccess)
                                _msg.ShowMessage($@"Get Card Reader Info Success; Process Id : {processId}");

                            else
                            {
                                if (_im30Trans.FinalResult.Error != null)
                                    _msg.ShowMessage($@"Error when Get Card Reader Info; Process Id : {processId}{"\r\n"}" + _im30Trans.FinalResult.Error.ToString());

                                else
                                    _msg.ShowMessage($@"Error when Get Card Reader Info; Unknown error; Process Id : {processId}\r\n");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //CYA-DEBUG .. Log Error Here
                    }
                }), processId.ToString(), 5, _logChannel
                , threadPriority: System.Threading.ThreadPriority.AboveNormal);
            }
        }
    }
}
