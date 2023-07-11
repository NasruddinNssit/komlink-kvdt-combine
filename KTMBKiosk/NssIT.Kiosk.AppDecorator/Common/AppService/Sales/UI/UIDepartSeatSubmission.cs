using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI
{
	[Serializable]
	public class UIDepartSeatSubmission : IKioskMsg, INetCommandDirective
	{
		public Guid BaseNetProcessId { get; private set; }
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;
		public AppModule Module { get; } = AppModule.UIKioskSales;
		public dynamic GetMsgData() => NotApplicable.Object;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.DepartSeatSubmission;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		public string ErrorMessage { get; set; } = null;

		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;

		public CustSeatDetail[] PassengerSeatDetailList { get; private set; } = null;
		public string TrainSeatModelId { get; private set; } = null;

		public UIDepartSeatSubmission(string processId, DateTime timeStamp, CustSeatDetail[] custSeatDetailList, string trainSeatModelId)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;

			PassengerSeatDetailList = custSeatDetailList;
			TrainSeatModelId = trainSeatModelId;
		}

		public void Dispose()
		{ }
	}
}
