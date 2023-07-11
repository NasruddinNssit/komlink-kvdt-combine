using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
	public class ETSInsuranceSubmissionCommand : IAccessDBCommand, IDisposable
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.ETSInsuranceSubmissionRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

		public string TransactionNo { get; private set; }
		public string InsuranceHeadersId { get; set; }

		public ETSInsuranceSubmissionCommand(string processId, Guid? netProcessId,
			string transactionNo, string insuranceHeadersId)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;

			TransactionNo = transactionNo;
			InsuranceHeadersId = insuranceHeadersId;
		}

		public void Dispose()
		{
			NetProcessId = null;
		}
	}
}
