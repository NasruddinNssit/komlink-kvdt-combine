using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
	public class ETSCheckoutSaleCommand : IAccessDBCommand, IDisposable
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.ETSCheckoutSaleRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

		public string TransactionNo { get; private set; }
		public int TotalSeatCount { get; set; }

		public ETSCheckoutSaleCommand(string processId, Guid? netProcessId,
			string transactionNo, int totalSeatCount)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;

			TransactionNo = transactionNo;
			TotalSeatCount = totalSeatCount;
		}

		public void Dispose()
		{
			NetProcessId = null;
		}
	}
}
