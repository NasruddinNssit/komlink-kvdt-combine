using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
	public class ETSIntercityTicketRequestCommand : IAccessDBCommand, IDisposable
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.ETSIntercityTicketRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		public string BookingNo { get; set; }

		public ETSIntercityTicketRequestCommand(string processId, Guid? netProcessId,
			string bookingNo)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;
			BookingNo = bookingNo;
		}

		public void Dispose()
		{
			NetProcessId = null;
		}
	}
}
