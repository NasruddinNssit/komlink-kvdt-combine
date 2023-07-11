using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Client.ViewPage.Trip;
using NssIT.Train.Kiosk.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Seat
{
    public interface ISeat2
    {
        void InitDepartSeatData(UIDepartSeatListAck uiDepartSeatList, UserSession session);
        void InitReturnSeatData(UIReturnSeatListAck uiReturnSeatList, UserSession session);
        TripMode TravalMode { get; }
    }
}
