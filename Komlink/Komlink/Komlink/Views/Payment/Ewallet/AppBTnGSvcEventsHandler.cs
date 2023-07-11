using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;

using NssIT.Train.Kiosk.Common.Data;
using NssIT.Train.Kiosk.Common.Data.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx;
using Komlink.NetClient;

namespace Komlink.Views.Payment.Ewallet
{
    public class AppBTnGSvcEventsHandler
    {
        private string _logChannel = "AppSys";

        private NetClientService _netClientSvc = null;

        public AppBTnGSvcEventsHandler(NetClientService netClientSvc)
        {
            _netClientSvc = netClientSvc;

            _netClientSvc.BTnGService.OnDataReceived += BTnGService_OnDataReceived;
        }

        private void BTnGService_OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.ReceivedData?.MsgObject?.GetMsgData() is IUIxBTnGPaymentOngoingGroupAck)
            {
                App.komlinkCardDetailScreen?.BTnGShowPaymentInfo(e.ReceivedData.MsgObject);
            }
        }
    }
}
