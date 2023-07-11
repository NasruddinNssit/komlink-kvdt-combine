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
	public class CustInfoPrerequisiteRequestCommand : IAccessDBCommand, IDisposable
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.CustInfoPrerequisiteRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;
		public string TrainService { get; set; } = string.Empty;

		public CustInfoPrerequisiteRequestCommand(string processId, Guid? netProcessId, string trainService)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;
			TrainService = trainService;
		}

		public void Dispose()
		{
			NetProcessId = null;
		}
	}
}

