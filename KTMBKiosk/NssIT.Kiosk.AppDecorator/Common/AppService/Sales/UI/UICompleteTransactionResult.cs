using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI
{
	/// <summary>
	/// Result of Sales Payment Submission
	/// </summary>
	[Serializable]
	public class UICompleteTransactionResult : IKioskMsg, IUserSession
	{
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;
		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.CompleteTransactionResult;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		public string ErrorMessage { get; set; } = null;

		public dynamic GetMsgData() => NotApplicable.Object;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

		/// <summary>
		/// return NssIT.Train.Kiosk.Common.Data.Response.PaymentSubmissionResult
		/// </summary>
		public object MessageData { get; private set; } = null;
		public ProcessResult ProcessState { get; private set; } = ProcessResult.Fail;
		public UserSession Session { get; private set; } = null;
		public string MachineId { get; private set; } = null;

		public UICompleteTransactionResult(Guid? refNetProcessId, string processId, DateTime timeStamp, object messageData, ProcessResult processState, string machineId)
		{
			RefNetProcessId = refNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
			MessageData = messageData;
			ProcessState = processState;
			MachineId = machineId;
		}

		public void UpdateSession(UserSession session) => Session = session;

		public void Dispose()
		{
			MessageData = null;
		}
	}
}
