using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
    public class CustPromoCodeVerifyCommand : IAccessDBCommand, IDisposable
    {
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.CustPromoCodeVerifyRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		public string TrainSeatModelId { get; private set; } = "";
		public Guid SeatLayoutModelId { get; private set; } = Guid.Empty;
		public string TicketTypesId { get; private set; } = "";
		public string PassengerIC { get; private set; } = "";
		public string PromoCode { get; private set; } = "";

		public CustPromoCodeVerifyCommand(string processId, Guid? netProcessId,
			string trainSeatModelId,
			Guid seatLayoutModelId,
			string ticketTypesId,
			string passengerIC,
			string promoCode)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;

			TrainSeatModelId = trainSeatModelId;
			SeatLayoutModelId = seatLayoutModelId;
			TicketTypesId = ticketTypesId;
			PassengerIC = passengerIC;
			PromoCode = promoCode;
		}

		public void Dispose()
		{
			NetProcessId = null;
		}
	}
}