using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
	public class CustInfoUpdateCommand : IAccessDBCommand, IDisposable
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.DepartCustInfoUpdateELSEReleaseSeatRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		
		public CustSeatDetail[] DepartPassengerSeatDetail { get; private set; }
		public CustSeatDetail[] ReturnPassengerSeatDetail { get; private set; }
		public string TransactionNo { get; private set; }

		public CustInfoUpdateCommand(string processId, Guid? netProcessId, 
			CustSeatDetail[] departPassengerSeatDetail, CustSeatDetail[] returnPassengerSeatDetail,
			string transactionNo)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;

			DepartPassengerSeatDetail = departPassengerSeatDetail;
			ReturnPassengerSeatDetail = returnPassengerSeatDetail;
			TransactionNo = transactionNo;
		}

		public void Dispose()
		{
			NetProcessId = null;
			DepartPassengerSeatDetail = null;
			ReturnPassengerSeatDetail = null;
		}
	}
}
