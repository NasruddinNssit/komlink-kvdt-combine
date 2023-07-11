using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30
{
	[Serializable()]
	public class ImmediateTerminateException : System.Exception
	{
		public ImmediateTerminateException(string errMsg = "Immediate Termination Exception")
			: base((errMsg ?? "Immediate Termination Exception"))
		{ }
	}

	[Serializable()]
	public class SystemTimeOutException : System.Exception
	{
		public SystemTimeOutException(string errMsg = "System Timeout Exception")
			: base((errMsg ?? "System Timeout Exception"))
		{ }
	}
}
