using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.KomLink
{
    public class KomLinkDeductValueResp: ICardResponse
    {
        public decimal MainPurse { get; private set; } = 0;
        public uint MainTransNo { get; private set; } = 0;
        public DateTime LastTransDatetime { get; private set; } = KomLinkCardInfo.MinDateTime;
        public KomLinkDeductValueResp(decimal mainPurse, uint mainTransNo, DateTime lastTransDatetime)
        {
            MainPurse = mainPurse;
            MainTransNo = mainTransNo;
            LastTransDatetime = lastTransDatetime;
        }
    }
}
