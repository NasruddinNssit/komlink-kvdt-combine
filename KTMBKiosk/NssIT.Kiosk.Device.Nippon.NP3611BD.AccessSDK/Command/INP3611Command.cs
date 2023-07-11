using NssIT.Kiosk.Device.Nippon.NP3611BD.OrgAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command
{
    public interface INP3611Command<out P, out A>  
        where P : INP3611CommandParameter  
        where A : INP3611CommandSuccessAnswer
    {
        string ModelTag { get; }
        Guid CommandId { get; }
        CommandStatus ExecStatus { get; }
        P Parameter { get; }
        A SuccessEcho { get; }
        void StartAccessSDKCommand(NP3611BDPrinter printer);
    }
}
