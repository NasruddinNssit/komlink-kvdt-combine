using NssIT.Kiosk.AppDecorator.Common.AppService.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Events
{
	public class SendMessageEventArgs : EventArgs, ISendMessageEventArgs, IDisposable 
	{
		public Guid? NetProcessId { get; } = null;
		public string ProcessId { get; } = null;

		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }

		public ResultStatus ResultState { get; private set; } = ResultStatus.New;

		private string _message = null;
		public string Message 
		{ 
			get
			{
				if ((string.IsNullOrWhiteSpace(KioskDataPack?.ErrorMessage) == false) && (KioskDataPack?.ErrorMessage?.Trim().Length > 0))
					return KioskDataPack.ErrorMessage;
				else
					return _message;
			}
			set
			{
				_message = value;
			}
		}

		public IKioskMsg KioskDataPack { get; private set; } = null;

		public AppModule ModuleAppctGroup { get; set; } = AppModule.Unknown;

		public SendMessageEventArgs(Guid? netProcessId, string processId, ResultStatus resultState, IKioskMsg kioskMessage)
		{
			NetProcessId = netProcessId;
			ProcessId = string.IsNullOrWhiteSpace(processId) ? "-" : processId;
			ResultState = resultState;
			KioskDataPack = kioskMessage;

			if (KioskDataPack != null)
			{
				ModuleAppctGroup = KioskDataPack.Module;
				Message = kioskMessage.ErrorMessage;
				Module = KioskDataPack.Module;
			}
		}

		public void Dispose()
		{
			KioskDataPack = null;
		}
	}
}
