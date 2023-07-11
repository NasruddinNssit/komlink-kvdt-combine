using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PAX.IM30.AccessSDK.Base.AxCommandSet
{
    public class StartCardTransAxComm : IIM30AxCommand 
    {
        public GateDirectionEn GateDirection { get; private set; }
        public int KomLinkFirstSPNo { get; private set; }
        public int KomLinkSecondSPNo { get; private set; }

        /// <summary>
        /// Minimum is 10 Sec
        /// </summary>
        public int MaxCardDetectedWaitingTimeSec { get; private set; }
        /// <summary>
        /// Minimum is 10 Sec
        /// </summary>
        public int MaxSaleDecisionWaitingTimeSec { get; private set; }

        public StartCardTransAxComm(GateDirectionEn gateDirection,
            int komLinkFirstSPNo, int komLinkSecondSPNo,
            int maxCardDetectedWaitingTimeSec, int maxSaleDecisionWaitingTimeSec)
        {
            GateDirection = gateDirection;
            KomLinkFirstSPNo = komLinkFirstSPNo;
            KomLinkSecondSPNo = komLinkSecondSPNo;

            if (maxCardDetectedWaitingTimeSec < 10)
                MaxCardDetectedWaitingTimeSec = 10;
            else
                MaxCardDetectedWaitingTimeSec = maxCardDetectedWaitingTimeSec;

            if (maxSaleDecisionWaitingTimeSec < 10)
                MaxSaleDecisionWaitingTimeSec = 10;
            else
                MaxSaleDecisionWaitingTimeSec = maxSaleDecisionWaitingTimeSec;
        }
    }
}
