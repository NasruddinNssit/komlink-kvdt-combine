using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI
{
	[Serializable]
	public class UISalesTimeoutWarningAck : IKioskMsg
	{
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;

		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.SalesTimeoutWarningAck;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }

		public string ErrorMessage { get; set; } = null;
		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

		public TimeoutMode ModeOfTimeout { get; private set; } = TimeoutMode.UIResponseTimeoutWarning;

		//////// return NssIT.Kiosk.AppDecorator.Common.AppService.Sales.--
		/// <summary>
		/// Null
		/// </summary>
		//xxpublic object MessageData { get; private set; } = null;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="refNetProcessId"></param>
		/// <param name="processId"></param>
		/// <param name="timeStamp"></param>
		/// <param name="messageData">Refer to NssIT.Kiosk.AppDecorator.Common.AppService.Sales.SalesPaymentData</param>
		public UISalesTimeoutWarningAck(Guid? refNetProcessId, string processId, DateTime timeStamp, TimeoutMode modeOfTimeout = TimeoutMode.UIResponseTimeoutWarning)
		{
			RefNetProcessId = refNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
			ModeOfTimeout = modeOfTimeout;
		}

		public void Dispose()
		{ }
	}
}
