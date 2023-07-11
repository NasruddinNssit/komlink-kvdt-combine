using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kvdt_kiosk.Models.Komlink
{
    
    public class KomlinkCardinfoResp
    {
        public string CSN { get; set; }
        public long KVDTCardNo { get; set; }
        public bool IsCardCancelled { get; set; }
        public bool IsCardBlacklisted { get; set; }
        public bool IsSP1Available { get; set; }
        public bool IsSP2Available { get; set; }
        public bool IsSP3Available { get; set; }
        public bool IsSP4Available { get; set; }
        public bool IsSP5Available { get; set; }
        public bool IsSP6Available { get; set; }
        public bool IsSP7Available { get; set; }
        public bool IsSP8Available { get; set; }
        public string ChkInGateNo { get; set; }
        public DateTime? ChkInDatetime { get; set; }
        public string ChkInStationNo { get; set; }
        public string ChkOutGateNo { get; set; }
        public DateTime? ChkOutDatetime { get; set; }
        public string ChkOutStationNo { get; set; }

        /// <summary>
        /// In Ringgit Malaysia
        /// </summary>
        public decimal MainPurse { get; set; }
        public int MainTransNo { get; set; }
        public string IssuerSAMId { get; set; }
        public string Gender { get; set; }
        public DateTime CardIssuedDate { get; set; }
        public DateTime CardExpireDate { get; set; }
        public string PNR { get; set; }
        public string CardType { get; set; }
        public DateTime CardTypeExpireDate { get; set; }
        public bool IsMalaysian { get; set; }
        public DateTime DOB { get; set; }
        public int LRCKey { get; set; }
        public string IDType { get; set; }
        public string IDNo { get; set; }

        /// <summary>
        /// Max. 32 Characters
        /// </summary>
        public string PassengerName { get; set; }

        public DateTime BLKStartDatetime { get; set; }
        public string RefillSAMId { get; set; }
        public DateTime RefillDatetime { get; set; }
        public DateTime LastTransDatetime { get; set; }

        ///// xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        ///// When ACG Check-in / Checkout, below 5 fields will have no value.
        public decimal BackupPurse { get; set; }
        public int BackupTransNo { get; set; }
        public string BLKSAMId { get; set; } //
        public string BLKCode { get; set; } //
        public DateTime BLKDatetime { get; set; } // 
        ///// xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        public string KomLinkSAMId { get; set; }

        public string MerchantNameAddr { get; set; }
        public DateTime? TransactionDateTime { get; set; }

        public IList<KomLinkSeasonPassData> KomLinkSeasonPassDatas { get; set; } = new List<KomLinkSeasonPassData>();
    }

    public class KomLinkSeasonPassData
    {
        public int SPNo { get; set; } = 1;
        public DateTime SPSaleDate { get; set; }
        public decimal SPMaxTravelAmtPDayPTrip { get; set; }
        public string SPIssuerSAMId { get; set; }
        public DateTime SPStartDate { get; set; }
        public DateTime SPEndDate { get; set; }
        public string SPSaleDocNo { get; set; }
        public string SPServiceCode { get; set; }
        public string SPPackageCode { get; set; }
        public string SPType { get; set; }
        public byte SPMaxTripCountPDay { get; set; }
        public bool SPIsAvoidChecking { get; set; }
        public bool SPIsAvoidTripDurationCheck { get; set; }
        public string SPOriginStationNo { get; set; }
        public string SPDestinationStationNo { get; set; }
        public DateTime SPLastTravelDate { get; set; }
        public byte SPDailyTravelTripCount { get; set; }
    }


}
