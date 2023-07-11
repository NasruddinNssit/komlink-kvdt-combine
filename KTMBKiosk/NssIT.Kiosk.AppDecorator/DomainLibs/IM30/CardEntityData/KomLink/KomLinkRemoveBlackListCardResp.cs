using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.KomLink
{
    public class KomLinkRemoveBlackListCardResp : ICardResponse
    {
        public bool IsCardBlacklisted { get; private set; } = false;
        public DateTime BLKStartDatetime { get; private set; } = KomLinkCardInfo.MaxDate;
        public DateTime BLKDatetime { get; private set; } = KomLinkCardInfo.MinDateTime;

        public KomLinkRemoveBlackListCardResp(
            bool isCardBlacklisted, DateTime blkStartDatetime, DateTime blkDatetime)
        {
            IsCardBlacklisted = isCardBlacklisted;
            BLKStartDatetime = blkStartDatetime;
            BLKDatetime = blkDatetime;
        }
    }
}
