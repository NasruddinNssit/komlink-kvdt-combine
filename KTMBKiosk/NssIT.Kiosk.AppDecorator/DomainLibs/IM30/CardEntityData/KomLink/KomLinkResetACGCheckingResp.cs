using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.KomLink
{
    public class KomLinkResetACGCheckingResp : ICardResponse
    {
        public string ChkInGateNo { get; private set; } = null;
        public DateTime ChkInDatetime { get; private set; } = KomLinkCardInfo.MinDateTime;
        public string ChkInStationNo { get; private set; } = null;
        public string ChkOutGateNo { get; private set; } = null;
        public DateTime ChkOutDatetime { get; private set; } = KomLinkCardInfo.MinDateTime;
        public string ChkOutStationNo { get; private set; } = null;

        public KomLinkResetACGCheckingResp(
            string chkInGateNo, DateTime chkInDatetime, string chkInStationNo,
            string chkOutGateNo, DateTime chkOutDatetime, string chkOutStationNo)
        {
            ChkInGateNo = chkInGateNo;
            ChkInDatetime = chkInDatetime;
            ChkInStationNo = chkInStationNo;
            ChkOutGateNo = chkOutGateNo;
            ChkOutDatetime = chkOutDatetime;
            ChkOutStationNo = chkOutStationNo;
        }
    }
}