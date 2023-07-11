using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult;
using NssIT.Kiosk.AppDecorator.Log;
using NssIT.Kiosk.Device.PAX.IM30.AccessSDK;
using NssIT.Kiosk.Device.PAX.IM30.AccessSDK.Base.AxCommandSet;
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

namespace IM30TestAccessSDK01
{
    /// <summary>
    /// Interaction logic for uscGetDeviceInfo.xaml
    /// </summary>
    public partial class uscIM30GetDeviceInfo : UserControl
    {
        private const string _logChannel = "TestUI";

        private LibShowMessageWindow.MessageWindow _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;
        private ShowMessageLogDelg _showMessageDelgHandler = null;
        private string _comPort = null;
        private IM30PortManagerAx _im30PortMan = null;
        public uscIM30GetDeviceInfo()
        {
            InitializeComponent();
            _showMessageDelgHandler = new ShowMessageLogDelg(ShowMessageDelgWorking);
        }

        public void SetCOMPort(string comPort)
        {
            _comPort = comPort;
        }

        private IM30PortManagerAx IM30PortMan
        {
            get
            {
                if (_im30PortMan is null)
                {
                    if (string.IsNullOrWhiteSpace(_comPort))
                        throw new Exception("COM Port not set yet.");

                    _im30PortMan = IM30PortManagerAx.GetAxPortManager(_comPort);

                    IM30PortMan.SetOnDebugShowMessageHandler(new ShowMessageLogDelg(ShowMessageDelgWorking));
                    //IM30PortMan.SetOnClientTransactionFinishedHandler(new OnTransactionFinishedDelg(OnTransactionFinishedDelgWorking));
                    //IM30PortMan.SetOnClientCardDetectedHandler(new NssIT.ACG.AppDecorator.DomainLibs.IM30.Base.OnCardDetectedDelg(OnCardDetectedDelgWorking));
                }

                return _im30PortMan;
            }
        }

        private void ShowMessageDelgWorking(string message)
        {
            _msg.ShowMessage(message);
        }

        //private void OnTransactionFinishedDelgWorking(IIM30TransResult transResult)
        //{
        //    if (transResult is null)
        //        _msg.ShowMessage($@"----- Invalid Transaction Result -----");

        //    else /* (transResult != null) */
        //        _msg.ShowMessage($@"----- ----- Final Get Device Info Result Received ----- -----");
        //}

        //private void OnCardDetectedDelgWorking(IM30DataModel cardInfo)
        //{
        //    _msg.ShowMessage($@"===== ===== Card Info found ===== ===== ===== ===== ====={"\r\n"}");
        //}

        private void IM30GetDeviceInfo_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (string.IsNullOrWhiteSpace(_comPort))
            {
                _msg.ShowMessage("Please select and apply a COM port.");
                return;
            }

            GetDeviceInfoTest01();

            return;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            ///
            void GetDeviceInfoTest01()
            {
                try
                {
                    IIM30TransResult trnResult = IM30PortMan.RunSoloCommand(
                                                    new GetDeviceInfoAxComm(),
                                                    out bool isPendingOutstandingTransaction,
                                                    out Exception error);
                    if (trnResult?.IsSuccess == true)
                        _msg.ShowMessage("-Get Card Reader Info Transaction has done Successful (GB01)~");

                    else if (trnResult?.IsSuccess == false)
                        _msg.ShowMessage("-Fail to Get Card Reader Info (GB01)~");

                    else if (error != null)
                        _msg.ShowMessage(error.ToString());

                    else if (isPendingOutstandingTransaction)
                        _msg.ShowMessage("-Found outstanding/previous Card Reader Transaction (GB01)~Get Card Reader Info#");

                    else
                        _msg.ShowMessage("-Fail to execute Card Reader (Single) command with unknown error (GB01)~Get Card Reader Info#");
                }
                catch (Exception ex)
                {
                    _msg.ShowMessage(ex.ToString());
                }
            }
        }
    }
}
