using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
	public class KomuterTicketTypePackageCommand : IAccessDBCommand, IDisposable
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.KomuterTicketTypePackageRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		public string OriginStationId { get; private set; }
		public string DestinationStationId { get; private set; }

		public KomuterTicketTypePackageCommand(string processId, Guid? netProcessId,
			string originStationId, string destinationStationId)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;

			OriginStationId = originStationId;
			DestinationStationId = destinationStationId;
		}

		public void Dispose()
		{
			NetProcessId = null;
		}
	}
}

