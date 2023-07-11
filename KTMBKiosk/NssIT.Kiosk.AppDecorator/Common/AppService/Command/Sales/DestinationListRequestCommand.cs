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
	public class DestinationListRequestCommand : IAccessDBCommand, IDisposable 
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.DestinationListRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		public string OriginStationId { get; private set; }

		public DestinationListRequestCommand(string processId, Guid? netProcessId, string originStationId)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;
			OriginStationId = originStationId;
		}

		public void Dispose()
		{
			NetProcessId = null;
		}
	}
}
