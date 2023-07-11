using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Delegate.App;
using NssIT.Train.Kiosk.Common.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.AccessDB.AxCommand
{
    public interface IAxCommand<TResult> : IAx
        where TResult : UIxKioskDataAckBase
    {
        string ProcessId { get; }
        Guid? NetProcessId { get;  }
        AppCallBackEvent CallBackEvent { get; }
    }
}
