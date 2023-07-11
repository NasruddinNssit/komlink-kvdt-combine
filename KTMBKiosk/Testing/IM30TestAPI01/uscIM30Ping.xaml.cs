using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult;
using NssIT.Kiosk.Tools.ThreadMonitor;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.Ping;
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
using NssIT.Kiosk.AppDecorator.Log;

namespace IM30TestAPI01
{
    /// <summary>
    /// Interaction logic for uscIM30Ping.xaml
    /// </summary>
    public partial class uscIM30Ping : UserControl
    {
        private const string _logChannel = "TestUI";

        private LibShowMessageWindow.MessageWindow _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;
        private ShowMessageLogDelg _showMessageDelgHandler = null;

        private string _comPort = null;

        public uscIM30Ping()
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

        private void PingIM30_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (string.IsNullOrWhiteSpace(_comPort))
            {
                _msg.ShowMessage("Please select and apply a COM port.");
                return;
            }

            Guid processId = Guid.NewGuid();
            IM30Ping newIM30Trans = null;

            CreateNewPingTest01();

            return;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            ///
            void CreateNewPingTest01()
            {
                try
                {

                    newIM30Trans = new IM30Ping(_comPort, new OnTransactionFinishedDelg(OnTransactionFinishedDelgWorking), processId.ToString()
                        , noActionMaxWaitSec: 10, _showMessageDelgHandler);

                    newIM30Trans.StartTransaction(out Exception ex2);
                    if (ex2 != null)
                        throw new Exception("Error when run CreateNewPingTest01; (A)", ex2);

                }
                catch (Exception ex)
                {
                    _msg.ShowMessage(ex.ToString());
                }
            }

            void OnTransactionFinishedDelgWorking(IIM30TransResult transResult)
            {
                if (newIM30Trans == null)
                    return;

                RunThreadMan tMan = new RunThreadMan(new Action(() => 
                {
                    try
                    {
                        if (newIM30Trans.FinalResult != null)
                        {
                            if (newIM30Trans.FinalResult.IsSuccess)
                                _msg.ShowMessage($@"Ping Card Reader Success; Process Id : {processId}");

                            else
                            {
                                if (newIM30Trans.FinalResult.Error != null)
                                    _msg.ShowMessage($@"Error when ping Card Reader; Process Id : {processId}{"\r\n"}" + newIM30Trans.FinalResult.Error.ToString());

                                else
                                    _msg.ShowMessage($@"Error when ping Card Reader; Unknown error; Process Id : {processId}\r\n");
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
