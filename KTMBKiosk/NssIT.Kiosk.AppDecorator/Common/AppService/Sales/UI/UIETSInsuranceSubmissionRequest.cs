﻿using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI
{
	[Serializable]
	public class UIETSInsuranceSubmissionRequest : IKioskMsg, INetCommandDirective
	{
		public Guid BaseNetProcessId { get; private set; }
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;

		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.ETSInsuranceSubmissionRequest;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }

		public string ErrorMessage { get; set; } = null;

		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;

		public string TransactionNo { get; private set; }
		public string InsuranceHeadersId { get; private set; }
		public bool IsAgreeToBuyInsurance { get; private set; }

		public UIETSInsuranceSubmissionRequest(string processId, DateTime timeStamp, string transactionNo, string insuranceHeadersId, bool isAgreeToBuyInsurance)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;

			TransactionNo = transactionNo;
			InsuranceHeadersId = insuranceHeadersId;
			IsAgreeToBuyInsurance = isAgreeToBuyInsurance;
		}

		public void Dispose()
		{

		}
	}
}