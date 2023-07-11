using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService
{
	/// <summary>
	/// UISsAck UI Session Ack
	/// </summary>
	/// <typeparam name="MData"></typeparam>
	[Serializable]
	public class UISessionAck<MData> : IKioskMsg, IUserSession
		where MData : UIxKioskDataAckBase
	{
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;
		public AppModule Module { get; private set; } = AppModule.Unknown;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public string ErrorMessage { get; set; } = null;
		public MData MsgData { get; private set; } = default;
		public dynamic GetMsgData() => MsgData;
		public UserSession Session { get; private set; } = null;
        UserSession IUserSession.Session => throw new NotImplementedException();

		//Obsolete start on 7th May 2021
		public CommInstruction Instruction { get; } = CommInstruction.ReferToGenericsUIObj;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		//------------------------------------------------------------------------------------------------

		public UISessionAck(Guid? refNetProcessId, string processId, AppModule module, DateTime timeStamp, MData messageData)
		{
			//messageData?.Base_UpdateProcessInfo(processId, refNetProcessId);

			RefNetProcessId = refNetProcessId;
			ProcessId = processId;
			MsgData = messageData;
			Module = module;
			TimeStamp = timeStamp;
		}

		private UISessionAck() { /*To disable constructor for public and protected type*/ }

		public void UpdateSession(UserSession session) => Session = session;

		public void Dispose()
		{ }
    }
}
