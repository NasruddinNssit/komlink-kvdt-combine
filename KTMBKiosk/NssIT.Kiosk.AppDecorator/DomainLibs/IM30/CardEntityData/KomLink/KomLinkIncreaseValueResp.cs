using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.KomLink
{
    public class KomLinkIncreaseValueResp : ICardResponse
    {
        public decimal MainPurse { get; private set; } = 0;
        public uint MainTransNo { get; private set; } = 0;
        public DateTime LastTransDatetime { get; private set; } = KomLinkCardInfo.MinDateTime;
        public KomLinkIncreaseValueResp(decimal mainPurse, uint mainTransNo, DateTime lastTransDatetime)
        {
            MainPurse = mainPurse;
            MainTransNo = mainTransNo;
            LastTransDatetime = lastTransDatetime;
        }
    }
}
