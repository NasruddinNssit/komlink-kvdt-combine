using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.Sale.IM30CardSale;

namespace NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.Sale
{
    interface ICardSaleProcess
    {
        void ProcessResponseData(byte[] recData, CardSaleProcessState currentProcessState,
            out bool? isEndWithOutStopCommand,
            out IIM30TransResult finalResult, out CardSaleProcessState expectedNextProcessState);
        void ProceedAction(CardSaleProcessState currentState, CardSaleProcessState proceedActionState, bool isRepeatACKRequested = false);
        void Send2ndCommand(bool isResend = false);
        void Shutdown();
    }
}
