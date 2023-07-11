using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
	public class ReturnSeatListCommand : IAccessDBCommand, IDisposable
	{
		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.ReturnSeatListRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;
		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		public string TripId { get; private set; } = null;

		public ReturnSeatListCommand(string processId, Guid? netProcessId,
			string tripId)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;

			TripId = tripId;
		}

		public void Dispose()
		{
			NetProcessId = null;
		}
	}
}

