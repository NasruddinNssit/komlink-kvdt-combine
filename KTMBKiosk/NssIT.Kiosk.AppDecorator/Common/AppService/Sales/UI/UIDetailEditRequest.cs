﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI
{
    [Serializable]
    public class UIDetailEditRequest : IKioskMsg, INetCommandDirective
    {
		public Guid BaseNetProcessId { get; private set; }
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;

		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.DetailEditRequest;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }

		public string ErrorMessage { get; set; } = null;

		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;

		public TickSalesMenuItemCode EditItemCode { get; private set; } = TickSalesMenuItemCode.FromStation;

		public UIDetailEditRequest(string processId, DateTime timeStamp, TickSalesMenuItemCode editItemCode)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
			EditItemCode = editItemCode;
		}

		public void Dispose()
		{

		}
	}
}