using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.StatusMonitorApp.CustomApp.KTMBApp
{
    /// <summary>
    /// ClassCode:EXIT65.07
    /// </summary>
    public class KTMBApp_AccessCallBackPlan : IAppAccessCallBackPlan
    {
        private IUIApplicationJob _uiApp = null;
        private AppModule _appModule = AppModule.UIKioskStatusMonitor;

        public KTMBApp_AccessCallBackPlan(IUIApplicationJob uiApp)
        {
            _uiApp = uiApp;
        }

        /// <summary>
        /// FuncCode:EXIT65.0705
        /// </summary>
        public async Task DeliverAccessResult(UIxKioskDataAckBase accessResult)
        {
            await Task.Delay(new TimeSpan(1));
            //if (accessResult is UIxGnBTnGAck<BTnGGetPaymentGatewayResult> uix1)
            //{
            //    await _uiApp.SendInternalCommand(accessResult.BaseProcessId, accessResult.BaseRefNetProcessId, new UIAck<UIxGnBTnGAck<BTnGGetPaymentGatewayResult>>(uix1.BaseRefNetProcessId, uix1.BaseProcessId, _appModule, DateTime.Now, uix1));
            //}
            //else
            //{
            //    throw new Exception($@"Unrecognized Kiosk Data Type to deliver access result; (EXIT60.0705.X01)");
            //}

        }
    }
}
