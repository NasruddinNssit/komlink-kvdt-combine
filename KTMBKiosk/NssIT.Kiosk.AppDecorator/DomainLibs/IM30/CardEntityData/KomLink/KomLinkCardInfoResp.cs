using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.KomLink
{
    /// <summary>
    /// Response KomlLink Card Info
    /// </summary>
    public class KomLinkCardInfoResp : ICardResponse
    {
        public KomLinkCardInfo CardInfo { get; private set; }

        public KomLinkCardInfoResp(KomLinkCardInfo cardInfo)
        {
            if (cardInfo is null)
                throw new Exception("--Invalid KomLink Card Info; Insert new Card Info--");

            CardInfo = cardInfo;
        }
    }
}
