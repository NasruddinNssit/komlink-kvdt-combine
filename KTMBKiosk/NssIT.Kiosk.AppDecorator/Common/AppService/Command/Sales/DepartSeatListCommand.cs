using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
	public class DepartSeatListCommand : IAccessDBCommand, IDisposable
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.DepartSeatListRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		//string date, string from, string to

		public string TripId { get; private set; } = null;

		public DepartSeatListCommand(string processId, Guid? netProcessId,
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
