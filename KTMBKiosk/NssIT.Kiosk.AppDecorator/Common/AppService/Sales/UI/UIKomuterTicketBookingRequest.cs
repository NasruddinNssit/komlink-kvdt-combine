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
	public class UIKomuterTicketBookingRequest : IKioskMsg, INetCommandDirective
	{
		public Guid BaseNetProcessId { get; private set; }
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;

		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.KomuterTicketBookingRequest;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }

		public string ErrorMessage { get; set; } = null;

		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		public string OriginStationId { get; set; }
		public string DestinationStationId { get; set; }
		public string DestinationStationName { get; set; }
		public string OriginStationName { get; set; }
		public string KomuterPackageId { get; set; }
		public TicketItem[] TicketItemList { get; set; }
		public UIKomuterTicketBookingRequest(string processId, DateTime timeStamp,
			string originStationId,
			string originStationName,
			string destinationStationId,
			string destinationStationName,
			string komuterPackageId,
			TicketItem[] ticketItemList)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;

			OriginStationId = originStationId;
			KomuterPackageId = komuterPackageId;
			DestinationStationId = destinationStationId;
			DestinationStationName = destinationStationName;
			OriginStationName = originStationName;
			TicketItemList = ticketItemList;
        }

		public void Dispose()
		{

		}
	}
}
