using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData
{
    public class KomLinkCardInfo : IDisposable 
    {
        public static DateTime MaxDate = new DateTime(2099, 12, 31);
        public static DateTime MinDateTime = new DateTime(2001, 01, 01, 0, 0, 0);

        public static bool CheckIsValidDateTime(DateTime dateTime)
        {
            if (dateTime.Year == MaxDate.Year && dateTime.Month == MaxDate.Month && dateTime.Day == MaxDate.Day)
                return false;
            else if (dateTime.Year == MinDateTime.Year && dateTime.Month == MinDateTime.Month && dateTime.Day == MinDateTime.Day)
                return false;
            else
                return true;
        }

        public KomLinkCardInfo(KomLinkCardData cardData)
        {
            //....
        }

        public void Dispose()
        {
            SeasonPassList = null;
        }

        public string CSN { get; private set; } = null;
        public ulong KVDTCardNo { get; private set; } = 0;
        public bool IsCardCancelled { get; private set; } = false;
        public bool IsCardBlacklisted { get; private set; } = false;
        public bool IsSP1Available { get; private set; } = false;
        public bool IsSP2Available { get; private set; } = false;
        public bool IsSP3Available { get; private set; } = false;
        public bool IsSP4Available { get; private set; } = false;
        public bool IsSP5Available { get; private set; } = false;
        public bool IsSP6Available { get; private set; } = false;
        public bool IsSP7Available { get; private set; } = false;
        public bool IsSP8Available { get; private set; } = false;
        public string ChkInGateNo { get; private set; } = null;
        public DateTime ChkInDatetime { get; private set; } = MinDateTime;
        public string ChkInStationNo { get; private set; } = null;
        public string ChkOutGateNo { get; private set; } = null;
        public DateTime ChkOutDatetime { get; private set; } = MinDateTime;
        public string ChkOutStationNo { get; private set; } = null;

        /// <summary>
        /// In Ringgit Malaysia
        /// </summary>
        public decimal MainPurse { get; private set; } = 0;
        public uint MainTransNo { get; private set; } = 0;
        public string IssuerSAMId { get; private set; } = null;
        public GenderEn Gender { get; private set; } = GenderEn.NA;
        public DateTime CardIssuedDate { get; private set; } = MaxDate;
        public DateTime CardExpireDate { get; private set; } = MinDateTime;
        public string PNR { get; private set; } = null;
        public string CardType { get; private set; } = null;
        public DateTime CardTypeExpireDate { get; private set; } = MinDateTime;
        public bool IsMalaysian { get; private set; } = false;
        public DateTime DOB { get; private set; } = MinDateTime;
        public byte LRCKey { get; private set; } = 0;
        public IDTypeEn IDType { get; private set; } = IDTypeEn.NA;
        public string IDNo { get; private set; } = null;

        /// <summary>
        /// Max. 32 Characters
        /// </summary>
        public string PassengerName { get; private set; } = null; /* PassengerName1 + PassengerName2 */
        
        public DateTime BLKStartDatetime { get; private set; } = MaxDate;
        public string RefillSAMId { get; private set; } = null;
        public DateTime RefillDatetime { get; private set; } = MinDateTime;
        public DateTime LastTransDatetime { get; private set; } = MinDateTime;

        ///// xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        ///// When ACG Check-in / Checkout, below 5 fields will have no value.
        public decimal BackupPurse { get; private set; } = 0;
        public uint BackupTransNo { get; private set; } = 0;
        public string BLKSAMId { get; private set; } = null;
        public string BLKCode { get; private set; } = null;
        public DateTime BLKDatetime { get; private set; } = MinDateTime;
        ///// xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        public string KomLinkSAMId { get; private set; } = null;

        public KomLinkSeasonPass[] SeasonPassList { get; private set; } = new KomLinkSeasonPass[] { };
    }

    public class  KomLinkSeasonPass
    {
        public int SPNo { get; private set; } = 0;
        public DateTime SPSaleDate { get; private set; } = KomLinkCardInfo.MinDateTime;
        public decimal SPMaxTravelAmtPDayPTrip { get; private set; } = 0;
        public string SPIssuerSAMId { get; private set; } = null;
        public DateTime SPStartDate { get; private set; } = KomLinkCardInfo.MinDateTime;
        public DateTime SPEndDate { get; private set; } = KomLinkCardInfo.MinDateTime;
        public string SPSaleDocNo { get; private set; } = null;
        public string SPServiceCode { get; private set; } = null;
        public string SPPackageCode { get; private set; } = null;
        public string SPType { get; private set; } = null;
        public byte SPMaxTripCountPDay { get; private set; } = 0;
        public bool SPIsAvoidChecking { get; private set; } = false;
        public bool SPIsAvoidTripDurationCheck { get; private set; } = false;
        public string SPOriginStationNo { get; private set; } = null;
        public string SPDestinationStationNo { get; private set; } = null;
        public DateTime SPLastTravelDate { get; private set; } = KomLinkCardInfo.MinDateTime;
        public byte SPDailyTravelTripCount { get; private set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spNo"></param>
        /// <param name="spSaleDate"></param>
        /// <param name="spMaxTravelAmtPDayPTrip">Max. Travel Amount per Trip im RM</param>
        /// <param name="spIssuerSAMId"></param>
        /// <param name="spStartDate"></param>
        /// <param name="spEndDate"></param>
        /// <param name="spSaleDocNo"></param>
        /// <param name="spServiceCode"></param>
        /// <param name="spPackageCode"></param>
        /// <param name="spType"></param>
        /// <param name="spMaxTripCountPDay"></param>
        /// <param name="spIsAvoidChecking"></param>
        /// <param name="spIsAvoidTripDurationCheck"></param>
        /// <param name="spOriginStationNo"></param>
        /// <param name="spDestinationStationNo"></param>
        /// <param name="spLastTravelDate"></param>
        /// <param name="spDailyTravelTripCount"></param>
        public KomLinkSeasonPass(int spNo, DateTime spSaleDate, decimal spMaxTravelAmtPDayPTrip, string spIssuerSAMId, 
            DateTime spStartDate, DateTime spEndDate, string spSaleDocNo, string spServiceCode, 
            string spPackageCode, string spType, byte spMaxTripCountPDay, 
            bool spIsAvoidChecking, bool spIsAvoidTripDurationCheck, 
            string spOriginStationNo, string spDestinationStationNo, 
            DateTime spLastTravelDate, byte spDailyTravelTripCount)
        {
            SPNo = spNo;
            SPSaleDate = spSaleDate;
            SPMaxTravelAmtPDayPTrip = spMaxTravelAmtPDayPTrip;
            SPIssuerSAMId = spIssuerSAMId;
            SPStartDate = spStartDate;
            SPEndDate = spEndDate;
            SPSaleDocNo = spSaleDocNo;
            SPServiceCode = spServiceCode;
            SPPackageCode = spPackageCode;
            SPType = spType;
            SPMaxTripCountPDay = spMaxTripCountPDay;
            SPIsAvoidChecking = spIsAvoidChecking;
            SPIsAvoidTripDurationCheck = spIsAvoidTripDurationCheck;
            SPOriginStationNo = spOriginStationNo;
            SPDestinationStationNo = spDestinationStationNo;
            SPLastTravelDate = spLastTravelDate;
            SPDailyTravelTripCount = spDailyTravelTripCount;
        }
    }
}
