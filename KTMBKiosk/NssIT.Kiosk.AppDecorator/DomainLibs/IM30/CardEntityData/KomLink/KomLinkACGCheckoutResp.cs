using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.KomLink
{
    public class KomLinkACGCheckoutResp : ICardResponse
    {
        public decimal MainPurse { get; private set; } = 0;
        public uint MainTransNo { get; private set; } = 0;
        public DateTime LastTransDatetime { get; private set; } = KomLinkCardInfo.MinDateTime;

        public string ChkInGateNo { get; private set; } = null;
        public DateTime ChkInDatetime { get; private set; } = KomLinkCardInfo.MinDateTime;
        public string ChkInStationNo { get; private set; } = null;
        public string ChkOutGateNo { get; private set; } = null;
        public DateTime ChkOutDatetime { get; private set; } = KomLinkCardInfo.MinDateTime;
        public string ChkOutStationNo { get; private set; } = null;

        public byte SPNo { get; private set; } = 0;
        public DateTime SPLastTravelDate { get; private set; } = KomLinkCardInfo.MinDateTime;
        public byte SPDailyTravelTripCount { get; private set; } = 0;

        public KomLinkACGCheckoutResp(
            decimal mainPurse, uint mainTransNo, DateTime lastTransDatetime, 
            string chkInGateNo, DateTime chkInDatetime, string chkInStationNo, 
            string chkOutGateNo, DateTime chkOutDatetime, string chkOutStationNo, 
            byte spNo, DateTime spLastTravelDate, byte spDailyTravelTripCount)
        {
            MainPurse = mainPurse;
            MainTransNo = mainTransNo;
            LastTransDatetime = lastTransDatetime;
            ChkInGateNo = chkInGateNo;
            ChkInDatetime = chkInDatetime;
            ChkInStationNo = chkInStationNo;
            ChkOutGateNo = chkOutGateNo;
            ChkOutDatetime = chkOutDatetime;
            ChkOutStationNo = chkOutStationNo;
            SPNo = spNo;
            SPLastTravelDate = spLastTravelDate;
            SPDailyTravelTripCount = spDailyTravelTripCount;
        }
    }
}
