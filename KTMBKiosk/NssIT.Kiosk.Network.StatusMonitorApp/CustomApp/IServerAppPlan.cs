using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.Server.AccessDB.AxCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.StatusMonitorApp.CustomApp
{
    public interface IServerAppPlan
    {
        void InitPlan();
        void PreProcess(IKioskMsg refSvcMsg);
    }
}