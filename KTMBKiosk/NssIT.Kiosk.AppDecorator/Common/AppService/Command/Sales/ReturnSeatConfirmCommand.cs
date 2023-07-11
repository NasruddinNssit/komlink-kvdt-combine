using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
	public class ReturnSeatConfirmCommand : IAccessDBCommand, IDisposable
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.ReturnSeatConfirmRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		public string BookingId { get; private set; }
		public string TrainSeatModelId { get; private set; }
		public CustSeatDetail[] PassengerSeatDetail { get; private set; }
		public int TrainIndex { get; private set; }
		public ReturnSeatConfirmCommand(string processId, Guid? netProcessId,
					string bookingId,
					string trainSeatModelId,
					CustSeatDetail[] passengerSeatDetail,
					int trainIndex = 0)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;
			BookingId = bookingId;
			TrainSeatModelId = trainSeatModelId;
			PassengerSeatDetail = passengerSeatDetail;
			TrainIndex = trainIndex;

			if (PassengerSeatDetail is null)
				PassengerSeatDetail = new CustSeatDetail[0];
		}

		public void Dispose()
		{
			NetProcessId = null;
			PassengerSeatDetail = null;
		}
	}
}

