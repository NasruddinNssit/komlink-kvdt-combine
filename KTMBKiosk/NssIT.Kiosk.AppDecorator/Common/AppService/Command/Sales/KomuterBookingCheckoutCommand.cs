using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
    public class KomuterBookingCheckoutCommand : IAccessDBCommand, IDisposable
    {
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.KomuterBookingCheckoutRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

		public string BookingId { get; set; }
		public decimal TotalAmount { get; set; }
		public string FinancePaymentMethod { get; set; }

		public KomuterBookingCheckoutCommand(string processId, Guid? netProcessId,
			string bookingId,
			decimal totalAmount,
			string financePaymentMethod)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;

			BookingId = bookingId;
			TotalAmount = totalAmount;
			FinancePaymentMethod = financePaymentMethod;
		}

		public void Dispose()
		{
			NetProcessId = null;
		}
	}
}


