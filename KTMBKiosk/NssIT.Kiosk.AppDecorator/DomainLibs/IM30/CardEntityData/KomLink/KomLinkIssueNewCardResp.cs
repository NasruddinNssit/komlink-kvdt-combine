using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.KomLink
{
    public class KomLinkIssueNewCardResp : ICardResponse
    {
        public ulong KVDTCardNo { get; private set; } = 0;
        /// <summary>
        /// In Ringgit Malaysia
        /// </summary>
        public decimal MainPurse { get; private set; } = 0;
        public uint MainTransNo { get; private set; } = 0;
        public string IssuerSAMId { get; private set; } = null;
        public string RefillSAMId { get; private set; } = null;
        public DateTime RefillDatetime { get; private set; } = KomLinkCardInfo.MinDateTime;
        public DateTime LastTransDatetime { get; private set; } = KomLinkCardInfo.MinDateTime;

        public int SPNo { get; private set; } = 0;
        public bool IsSPAvailable { get; private set; } = false;
        public string SPSaleDocNo { get; private set; } = null;
        public DateTime SPSaleDate { get; private set; } = KomLinkCardInfo.MinDateTime;
        public string SPIssuerSAMId { get; private set; } = null;

        public KomLinkIssueNewCardResp(
            ulong kvdtCardNo,
            decimal mainPurse,
            uint mainTransNo,
            string issuerSAMId,
            string refillSAMId,
            DateTime refillDatetime,
            DateTime lastTransDatetime,
            int spNo,
            bool isSPAvailable,
            string spSaleDocNo,
            DateTime spSaleDate,
            string spIssuerSAMId)
        {
            KVDTCardNo = kvdtCardNo;
            MainPurse = mainPurse;
            MainTransNo = mainTransNo;
            IssuerSAMId = issuerSAMId;
            RefillSAMId = refillSAMId;
            RefillDatetime = refillDatetime;
            LastTransDatetime = lastTransDatetime;

            if (spNo > 0)
            {
                SPNo = spNo;
                IsSPAvailable = isSPAvailable;
                SPSaleDocNo = spSaleDocNo;
                SPSaleDate = spSaleDate;
                SPIssuerSAMId = spIssuerSAMId;
            }
        }
    }
}
