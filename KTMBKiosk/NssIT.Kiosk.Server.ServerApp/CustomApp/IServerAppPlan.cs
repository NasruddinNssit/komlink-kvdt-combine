using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.ServerApp.CustomApp
{
    public interface IServerAppPlan
    {
        UISalesInst NextInstruction(string procId, Guid? netProcId, IKioskMsg svcMsg, UserSession session, out bool releaseSeatRequest);
        UserSession UpdateUserSession(UserSession userSession, IKioskMsg svcMsg);
        UserSession SetEditingSession(UserSession userSession, TickSalesMenuItemCode detailItemCode);
        UserSession SetUIPageNavigateSession(UserSession userSession, UIPageNavigateRequest pageNav);
    }
}
