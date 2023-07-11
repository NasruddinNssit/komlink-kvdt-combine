﻿using NssIT.Kiosk.AppDecorator.Common.AppService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.StatusMonitorApp.CustomApp
{
    interface IAppAccessCallBackPlan
    {
        Task DeliverAccessResult(UIxKioskDataAckBase accessResult);
    }
}
