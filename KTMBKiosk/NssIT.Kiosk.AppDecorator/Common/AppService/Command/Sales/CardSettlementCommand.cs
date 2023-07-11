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
	public class CardSettlementCommand : IAccessDBCommand, IDisposable
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.CardSettlementSubmission;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

		public string HostNo { get; set; }
		public string BatchNumber { get; set; }
		public string BatchCount { get; set; }
		public decimal BatchCurrencyAmount { get; set; }
		public string StatusCode { get; set; }
		public string MachineId { get; set; }
		public string ErrorMessage { get; set; }

		public CardSettlementCommand(string processId, Guid? netProcessId,
			string hostNo, string batchNumber, string batchCount, decimal batchCurrencyAmount, string statusCode,
			string machineId, string errorMessage)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;

			HostNo = hostNo;
			BatchNumber = batchNumber;
			BatchCount = batchCount;
			BatchCurrencyAmount = batchCurrencyAmount;
			StatusCode = statusCode;
			MachineId = machineId;
			ErrorMessage = errorMessage;
		}

		public void Dispose()
		{
			NetProcessId = null;
		}
	}
}