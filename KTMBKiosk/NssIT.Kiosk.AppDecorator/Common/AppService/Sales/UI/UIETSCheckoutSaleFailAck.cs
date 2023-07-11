﻿using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI
{
	[Serializable]
	public class UIETSCheckoutSaleFailAck : IKioskMsg, IUserSession
	{
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;

		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.ETSCheckoutSaleFailAck;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }

		public string ErrorMessage { get; set; } = null;

		public UserSession Session { get; private set; } = null;

		public bool IsRequestAmendPassengerInfo { get; private set; } = false;

		public UIETSCheckoutSaleFailAck(Guid? refNetProcessId, string processId, DateTime timeStamp, string failErrorMessage)
		{
			RefNetProcessId = refNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
			ErrorMessage = failErrorMessage;
		}

		public void UpdateSession(UserSession session) => Session = session;

		public void Dispose()
		{
			
		}
	}
}