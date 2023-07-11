using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.PopupWindow
{
    public interface ITimeout
    {
        void InitPageData(UISalesTimeoutWarningAck uiTimeoutWarn, LanguageCode? language);
    }
}
