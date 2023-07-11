using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
	/// <summary>
	/// Sales Get Destination List Command
	/// </summary>
	public class KomuterTicketBookingRequestCommand : IAccessDBCommand, IDisposable
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.KomuterTicketBookingRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

		public string OriginStationId { get; set; }

		public string DestinationStationId { get; set; }

		public string DestinationStationName { get; set; }

		public string OriginStationName { get; set; }

		public string KomuterPackageId { get; set; }
		public TicketItem[] TicketItemList { get; set; }

		public KomuterTicketBookingRequestCommand(string processId, Guid? netProcessId,
			string originStationId,
			string originStationName,
			string destinationStationId,
			string destinationStationName,
			string komuterPackageId,		
			TicketItem[] ticketItemList)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;

			OriginStationId = originStationId;
			OriginStationName = originStationName;
			DestinationStationId = destinationStationId;
			DestinationStationName = destinationStationName;
			KomuterPackageId = komuterPackageId;
			TicketItemList = ticketItemList;
		}

		public void Dispose()
		{
			NetProcessId = null;
		}
	}
}


