using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.TicketSummary
{
    public interface ITicketSummaryPortal
    {
        bool IsValidToShow { get; set; }
        void UpdateDepartureDate(DateTime newDepartureDate);
        void UpdateSummary(UserSession session, TravelMode travelMode);
        bool IsActive { get; set; }
    }
}
