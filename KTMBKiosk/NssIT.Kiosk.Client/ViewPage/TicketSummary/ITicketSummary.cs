using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.TicketSummary
{
    public interface ITicketSummary
    {
		void UpdateSummary(UserSession sessionData, TickSalesMenuItemCode currentEditItemCode);
		void UpdateDepartDate(DateTime newDepartDate);
		void UpdateReturnDate(DateTime newReturntDate);
	}
}
