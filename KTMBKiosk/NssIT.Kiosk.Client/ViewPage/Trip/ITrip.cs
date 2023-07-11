using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Trip
{
    public interface ITrip
    {
        void InitData(UserSession session, TravelMode travelMode);

        void UpdateDepartTripList(UIDepartTripListAck uiDTrip, UserSession session);
        void UpdateReturnTripList(UIReturnTripListAck uiRTrip, UserSession session);

        DateTime SelectedDay { get; }
        string SelectedTripId { get; }
        TripMode TripMode { get; }
        void UpdateShieldErrorMessage(string message);
        void ResetPageAfterError();
    }
}
