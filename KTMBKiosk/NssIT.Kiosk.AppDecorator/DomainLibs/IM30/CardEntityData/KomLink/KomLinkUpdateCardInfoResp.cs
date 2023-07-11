using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.KomLink
{
    public class KomLinkUpdateCardInfoResp : ICardResponse
    {
        public GenderEn Gender { get; private set; } = GenderEn.NA;
        public string PNR { get; private set; } = null;
        public string CardType { get; private set; } = null;
        public DateTime CardTypeExpireDate { get; private set; } = KomLinkCardInfo.MinDateTime;
        public bool IsMalaysian { get; private set; } = false;
        public DateTime DOB { get; private set; } = KomLinkCardInfo.MinDateTime;
        public IDTypeEn IDType { get; private set; } = IDTypeEn.NA;
        public string IDNo { get; private set; } = null;

        /// <summary>
        /// Max. 32 Characters
        /// </summary>
        public string PassengerName { get; private set; } = null; /* PassengerName1 + PassengerName2 */
        public DateTime LastTransDatetime { get; private set; } = KomLinkCardInfo.MinDateTime;

        public KomLinkUpdateCardInfoResp(
            GenderEn gender, string pnrNo,
            string cardType, DateTime cardTypeExpireDate,
            bool isMalaysian, DateTime dob,
            IDTypeEn idType, string idNo,
            string passengerName, DateTime lastTransDatetime)
        {
            Gender = gender;
            PNR = pnrNo;
            CardType = cardType;
            CardTypeExpireDate = cardTypeExpireDate;
            IsMalaysian = isMalaysian;
            DOB = dob;
            IDType = idType;
            IDNo = idNo;
            PassengerName = passengerName;
            LastTransDatetime = lastTransDatetime;
        }
    }
}
