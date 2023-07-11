﻿using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI
{
	[Serializable]
	public class UIDepartCustInfoUpdateResult : IKioskMsg, IUserSession
	{
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;

		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.CustInfoUpdateResult;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }

		public string ErrorMessage { get; set; } = null;

		/// <summary>
		/// return NssIT.Train.Kiosk.Common.Data.Response.PassengerSubmissionResult
		/// </summary>
		public object MessageData { get; private set; } = null;

		public UserSession Session { get; private set; } = null;

		public bool IsRequestAmendPassengerInfo { get; private set; } = false;

		public UIDepartCustInfoUpdateResult(Guid? refNetProcessId, string processId, DateTime timeStamp, object messageData, bool isRequestAmendPassengerInfo = false)
		{
			RefNetProcessId = refNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
			MessageData = messageData;
			IsRequestAmendPassengerInfo = isRequestAmendPassengerInfo;
		}

		public void UpdateSession(UserSession session) => Session = session;

		public void Dispose()
		{
			MessageData = null;
		}
	}
}