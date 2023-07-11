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
	public class CustInfoPNRTicketTypeRequestCommand : IAccessDBCommand, IDisposable
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.CustInfoPNRTicketTypeRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		public string BookingId { get; private set; } = "";
		public string PassenggerIdentityNo { get; private set; } = "";
		public Guid[] TripScheduleSeatLayoutDetails_Ids { get; private set; } = null;

		public CustInfoPNRTicketTypeRequestCommand(string processId, Guid? netProcessId, 
			string bookingId, string passenggerIdentityNo, Guid[] tripScheduleSeatLayoutDetails_Ids)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;

			BookingId = bookingId;
			PassenggerIdentityNo = passenggerIdentityNo;
			TripScheduleSeatLayoutDetails_Ids = tripScheduleSeatLayoutDetails_Ids;
		}

		public void Dispose()
		{
			NetProcessId = null;
		}
	}
}

