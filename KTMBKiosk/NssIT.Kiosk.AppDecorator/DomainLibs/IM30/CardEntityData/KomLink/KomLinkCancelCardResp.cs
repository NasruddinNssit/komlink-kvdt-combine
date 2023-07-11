using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.KomLink
{
    public class KomLinkCancelCardResp : ICardResponse
    {
        public bool IsCardCancelled { get; private set; } = false;

        public KomLinkCancelCardResp(bool isCardCancelled)
        {
            IsCardCancelled = isCardCancelled; 
        }
    }
}