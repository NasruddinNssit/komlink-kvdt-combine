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
	public class UIOriginSubmission : IKioskMsg, INetCommandDirective
	{
		public Guid BaseNetProcessId { get; private set; }
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;
		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.OriginSubmission;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		public string ErrorMessage { get; set; } = null;
		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;
		//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
		public string OriginCode { get; private set; } = null;
		public string OriginName { get; private set; } = null;
		public IList<string> TrainService { get; set; } = null;

		public UIOriginSubmission(string processId, DateTime timeStamp, string originCode, string originName, IList<string> trainService)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
			OriginCode = originCode;
			OriginName = originName;
			TrainService = trainService;
		}

		public void Dispose()
		{ }
	}
}