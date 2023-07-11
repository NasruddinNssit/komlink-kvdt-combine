using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.KomLink
{
    public class KomLinkRemoveCancelCardResp : ICardResponse
    {
        public bool IsCardCancelled { get; private set; } = false;
        public DateTime LastTransDatetime { get; private set; } = KomLinkCardInfo.MinDateTime;

        public KomLinkRemoveCancelCardResp(bool isCardCancelled, DateTime lastTransDatetime)
        {
            IsCardCancelled = isCardCancelled;
            LastTransDatetime = lastTransDatetime;
        }
    }
}