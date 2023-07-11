using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.KomLink
{
    public class KomLinkIssueSeasonPassResp : ICardResponse
    {
        public byte SPNo { get; private set; } = 0;
        public string SPIssuerSAMId { get; private set; } = null;
        public string SPSaleDocNo { get; private set; } = null;

        public KomLinkIssueSeasonPassResp(byte spNo, string spIssuerSAMId, string spSaleDocNo)
        {
            SPNo = spNo;
            SPIssuerSAMId = spIssuerSAMId;
            SPSaleDocNo = spSaleDocNo;
        }
    }
}
