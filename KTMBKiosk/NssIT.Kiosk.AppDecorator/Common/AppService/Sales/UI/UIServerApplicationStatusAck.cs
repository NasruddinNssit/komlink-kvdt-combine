using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.AppDecorator.Config.ConfigConstant;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI
{
	[Serializable]
	public class UIServerApplicationStatusAck : IKioskMsg
    {
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;

		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.ServerApplicationStatusAck;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }

		public string ErrorMessage { get; set; } = null;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

		public bool ServerAppHasDisposed { get; private set; } = false;
		public bool ServerAppHasShutdown { get; private set; } = false;
		/// <summary>
		/// return NssIT.Train.Kiosk.Common.Data.Response.CounterConfigCompiledResult
		/// </summary>
		public object MessageData { get; private set; } = null;
		public string MachineId { get; private set; } = null;
		public bool IsResultSuccess { get; private set; } = false;
		public string ApplicationVersion { get; private set; } = "#####";

		public string WebApiURL { get; private set; } = "#####";
		public WebAPISiteCode WebApiCode { get; private set; } = WebAPISiteCode.Unknown;

		public UIServerApplicationStatusAck(Guid? refNetProcessId, string processId, DateTime timeStamp,
			string applicationVersion, bool serverAppHasDisposed, bool serverAppHasShutdown, object messageData, 
			string machineId, string webApiURL, WebAPISiteCode webApiCode, bool isResultSuccess)
		{
			RefNetProcessId = refNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;

			ApplicationVersion = (applicationVersion?.Trim().Length > 0) ? applicationVersion?.Trim() : "#####";
			ServerAppHasDisposed = serverAppHasDisposed;
			ServerAppHasShutdown = serverAppHasShutdown;
			MessageData = messageData;
			MachineId = machineId;
			WebApiURL = webApiURL;
			WebApiCode = webApiCode;
			IsResultSuccess = isResultSuccess;
		}

		public void Dispose()
		{ }
	}
}
