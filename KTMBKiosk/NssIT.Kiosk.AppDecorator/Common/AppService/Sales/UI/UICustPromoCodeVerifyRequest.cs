using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI
{
	[Serializable]
	public class UICustPromoCodeVerifyRequest : IKioskMsg, INetCommandDirective
	{
		public Guid BaseNetProcessId { get; private set; }
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;
		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.CustPromoCodeVerifyRequest;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		public string ErrorMessage { get; set; } = null;
		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

		public string TrainSeatModelId { get; private set; } = "";
		public Guid SeatLayoutModelId { get; private set; } = Guid.Empty;
		public string TicketTypesId { get; private set; } = "";
		public string PassengerIC { get; private set; } = "";
		public string PromoCode { get; private set; } = "";

		public UICustPromoCodeVerifyRequest(string processId, DateTime timeStamp,
			string trainSeatModelId,
			Guid seatLayoutModelId,
			string ticketTypesId,
			string passengerIC,
			string promoCode)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;

			TrainSeatModelId = trainSeatModelId;
			SeatLayoutModelId = seatLayoutModelId;
			TicketTypesId = ticketTypesId;
			PassengerIC = passengerIC;
			PromoCode = promoCode;
		}

		public void Dispose()
		{ }
	}
}

