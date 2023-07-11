using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
	/// <summary>
	/// Extend Booking Timeout Command
	/// </summary>
	public class ExtendBookingTimeoutCommand : IAccessDBCommand, IDisposable
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.SalesBookingTimeoutExtensionRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

		public string BookingId { get; private set; }

		public ExtendBookingTimeoutCommand(string processId, Guid? netProcessId, string bookingId)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;

			BookingId = bookingId;
		}

		public void Dispose()
		{
			NetProcessId = null;
		}
	}
}