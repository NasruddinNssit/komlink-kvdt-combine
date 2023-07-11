using System;
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
using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData;
using NssIT.Kiosk.AppDecorator.Log;
using NssIT.Kiosk.Device.PAX.IM30.AccessSDK;

namespace IM30TestAccessSDKInterface01
{
    /// <summary>
    /// Interaction logic for uscIM30GetLastTransaction.xaml
    /// </summary>
    public partial class uscIM30GetLastTransaction : UserControl
    {
        private const string _logChannel = "TestUI";

        private LibShowMessageWindow.MessageWindow _msg = LibShowMessageWindow.MessageWindow.DefaultMessageWindow;
        private ShowMessageLogDelg _showMessageDelgHandler = null;
        private string _comPort = null;
        private IM30AccessSDK _im30AccessSDK = null;

        public uscIM30GetLastTransaction()
        {
            InitializeComponent();
            //_showMessageDelgHandler = new ShowMessageDelg(ShowMessageDelgWorking);
        }

        public void SetCOMPort(string comPort)
        {
            if ((string.IsNullOrWhiteSpace(comPort) == false) && (_im30AccessSDK is null))
            {
                _comPort = comPort;

                _im30AccessSDK = new IM30AccessSDK(_comPort, _showMessageDelgHandler);
            }
        }

        private void ShowMessageDelgWorking(string message)
        {
            _msg.ShowMessage(message);
        }

        private void IM30GetLastTrans_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (string.IsNullOrWhiteSpace(_comPort))
            {
                _msg.ShowMessage("Please select and apply a COM port.");
                return;
            }
            GetLastTransactionTest01();

            return;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            ///
            void GetLastTransactionTest01()
            {
                try
                {
                    bool isGetSuccess = _im30AccessSDK.GetLastTransaction(out ICardResponse transInfo, out Exception error);

                    if ((isGetSuccess == true) && (transInfo != null))
                    {
                        _msg.ShowMessage($@"=====> Type: {transInfo.GetType().Name}");
                        _msg.ShowMessage(JsonConvert.SerializeObject(transInfo, Formatting.Indented));
                    }
                    else if (error != null)
                        _msg.ShowMessage("-Fail Get LastTransaction (A)~; " + error.ToString());

                    else
                        _msg.ShowMessage("-Fail Get LastTransaction (B)~");
                }
                catch (Exception ex)
                {
                    _msg.ShowMessage(ex.ToString());
                }
            }
        }
    }
}
