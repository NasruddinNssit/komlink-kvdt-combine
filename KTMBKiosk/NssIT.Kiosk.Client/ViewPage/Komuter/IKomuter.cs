using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Client.ViewPage.Trip;
using NssIT.Train.Kiosk.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Komuter
{
    public interface IKomuter
    {
        void InitData(UISalesStartSellingAck uiStartSellingAck);
        void UpdateTicketTypePackage(UIKomuterTicketTypePackageAck uiTickPack);
        void UpdateBookingData(UIKomuterTicketBookingAck uiBookingAck);
        void UpdateBookingCheckoutResult(UIKomuterBookingCheckoutAck bookingCheckoutAck);
        void UpdateKomuterTicketPaymentStatus(UIKomuterCompletePaymentAck paymentStatusAck);
        void BTnGShowPaymentInfo(IKioskMsg kioskMsg);
    }
}
