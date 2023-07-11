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
	public class UICustInfoPNRTicketTypeRequest : IKioskMsg, INetCommandDirective
	{
		public Guid BaseNetProcessId { get; private set; }
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;
		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.CustInfoPNRTicketTypeRequest;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		public string ErrorMessage { get; set; } = null;
		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;
		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		public string BookingId { get; private set; } = "";
		public string PassenggerIdentityNo { get; private set; } = "";
		public Guid[] TripScheduleSeatLayoutDetails_Ids { get; private set; } = null;

		public UICustInfoPNRTicketTypeRequest(string processId, DateTime timeStamp, string bookingId, string passenggerId, Guid[] tripScheduleSeatLayoutDetails_Ids)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
			BookingId = bookingId;
			PassenggerIdentityNo = passenggerId;
			TripScheduleSeatLayoutDetails_Ids = tripScheduleSeatLayoutDetails_Ids;
		}

		public void Dispose()
		{
			TripScheduleSeatLayoutDetails_Ids = null;
		}
	}
}

