using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData
{
    public class KomLinkCardData
    {
        public string CSN { get; set; } = null;
        public ulong KVDTCardNo { get; set; } = 0;
        public byte IsCardCancelled { get; set; } = 0;
        public byte IsCardBlacklisted { get; set; } = 0;
        public byte IsSP1Available { get; set; } = 0;
        public byte IsSP2Available { get; set; } = 0;
        public byte IsSP3Available { get; set; } = 0;
        public byte IsSP4Available { get; set; } = 0;
        public byte IsSP5Available { get; set; } = 0;
        public byte IsSP6Available { get; set; } = 0;
        public byte IsSP7Available { get; set; } = 0;
        public byte IsSP8Available { get; set; } = 0;
        public string ChkInGateNo { get; set; } = null;
        public string ChkInDatetime { get; set; } = null;
        public string ChkInStationNo { get; set; } = null;
        public string ChkOutGateNo { get; set; } = null;
        public string ChkOutDatetime { get; set; } = null;
        public string ChkOutStationNo { get; set; } = null;
        public uint MainPurse { get; set; } = 0;
        public uint BackupPurse { get; set; } = 0;
        public uint MainTransNo { get; set; } = 0;
        public uint BackupTransNo { get; set; } = 0;
        public string IssuerSAMId { get; set; } = null;
        public string Gender { get; set; } = null;
        public string CardIssuedDate { get; set; } = null;
        public string CardExpireDate { get; set; } = null;
        public string PNR { get; set; } = null;
        public string CardType { get; set; } = null;
        public string CardTypeExpireDate { get; set; } = null;
        public byte IsMalaysian { get; set; } = 0;
        public string DOB { get; set; } = null;
        public byte LRCKey { get; set; } = 0;
        public string IDType { get; set; } = null;
        public string IDNo { get; set; } = null;
        public string PassengerName1 { get; set; } = null;
        public string PassengerName2 { get; set; } = null;
        public string BLKSAMId { get; set; } = null;
        public string BLKStartDatetime { get; set; } = null;
        public string BLKCode { get; set; } = null;
        public string BLKDatetime { get; set; } = null;
        public string RefillSAMId { get; set; } = null;
        public string RefillDatetime { get; set; } = null;
        public string LastTransDatetime { get; set; } = null;


        public string SP1SaleDate { get; set; } = null;
        public int SP1MaxTravelAmtPDayPTrip { get; set; } = 0;
        public string SP1IssuerSAMId { get; set; } = null;
        public string SP1StartDate { get; set; } = null;
        public string SP1EndDate { get; set; } = null;
        public string SP1SaleDocNo { get; set; } = null;
        public string SP1ServiceCode { get; set; } = null;
        public string SP1PackageCode { get; set; } = null;
        public string SP1Type { get; set; } = null;
        public byte SP1MaxTripCountPDay { get; set; } = 0;
        public byte SP1IsAvoidChecking { get; set; } = 0;
        public byte SP1IsAvoidTripDurationCheck { get; set; } = 0;
        public string SP1OriginStationNo { get; set; } = null;
        public string SP1DestinationStationNo { get; set; } = null;
        public string SP1LastTravelDate { get; set; } = null;
        public byte SP1DailyTravelTripCount { get; set; } = 0;


        public string SP2SaleDate { get; set; } = null;
        public int SP2MaxTravelAmtPDayPTrip { get; set; } = 0;
        public string SP2IssuerSAMId { get; set; } = null;
        public string SP2StartDate { get; set; } = null;
        public string SP2EndDate { get; set; } = null;
        public string SP2SaleDocNo { get; set; } = null;
        public string SP2ServiceCode { get; set; } = null;
        public string SP2PackageCode { get; set; } = null;
        public string SP2Type { get; set; } = null;
        public byte SP2MaxTripCountPDay { get; set; } = 0;
        public byte SP2IsAvoidChecking { get; set; } = 0;
        public byte SP2IsAvoidTripDurationCheck { get; set; } = 0;
        public string SP2OriginStationNo { get; set; } = null;
        public string SP2DestinationStationNo { get; set; } = null;
        public string SP2LastTravelDate { get; set; } = null;
        public byte SP2DailyTravelTripCount { get; set; } = 0;


        public string SP3SaleDate { get; set; } = null;
        public int SP3MaxTravelAmtPDayPTrip { get; set; } = 0;
        public string SP3IssuerSAMId { get; set; } = null;
        public string SP3StartDate { get; set; } = null;
        public string SP3EndDate { get; set; } = null;
        public string SP3SaleDocNo { get; set; } = null;
        public string SP3ServiceCode { get; set; } = null;
        public string SP3PackageCode { get; set; } = null;
        public string SP3Type { get; set; } = null;
        public byte SP3MaxTripCountPDay { get; set; } = 0;
        public byte SP3IsAvoidChecking { get; set; } = 0;
        public byte SP3IsAvoidTripDurationCheck { get; set; } = 0;
        public string SP3OriginStationNo { get; set; } = null;
        public string SP3DestinationStationNo { get; set; } = null;
        public string SP3LastTravelDate { get; set; } = null;
        public byte SP3DailyTravelTripCount { get; set; } = 0;


        public string SP4SaleDate { get; set; } = null;
        public int SP4MaxTravelAmtPDayPTrip { get; set; } = 0;
        public string SP4IssuerSAMId { get; set; } = null;
        public string SP4StartDate { get; set; } = null;
        public string SP4EndDate { get; set; } = null;
        public string SP4SaleDocNo { get; set; } = null;
        public string SP4ServiceCode { get; set; } = null;
        public string SP4PackageCode { get; set; } = null;
        public string SP4Type { get; set; } = null;
        public byte SP4MaxTripCountPDay { get; set; } = 0;
        public byte SP4IsAvoidChecking { get; set; } = 0;
        public byte SP4IsAvoidTripDurationCheck { get; set; } = 0;
        public string SP4OriginStationNo { get; set; } = null;
        public string SP4DestinationStationNo { get; set; } = null;
        public string SP4LastTravelDate { get; set; } = null;
        public byte SP4DailyTravelTripCount { get; set; } = 0;


        public string SP5SaleDate { get; set; } = null;
        public int SP5MaxTravelAmtPDayPTrip { get; set; } = 0;
        public string SP5IssuerSAMId { get; set; } = null;
        public string SP5StartDate { get; set; } = null;
        public string SP5EndDate { get; set; } = null;
        public string SP5SaleDocNo { get; set; } = null;
        public string SP5ServiceCode { get; set; } = null;
        public string SP5PackageCode { get; set; } = null;
        public string SP5Type { get; set; } = null;
        public byte SP5MaxTripCountPDay { get; set; } = 0;
        public byte SP5IsAvoidChecking { get; set; } = 0;
        public byte SP5IsAvoidTripDurationCheck { get; set; } = 0;
        public string SP5OriginStationNo { get; set; } = null;
        public string SP5DestinationStationNo { get; set; } = null;
        public string SP5LastTravelDate { get; set; } = null;
        public byte SP5DailyTravelTripCount { get; set; } = 0;


        public string SP6SaleDate { get; set; } = null;
        public int SP6MaxTravelAmtPDayPTrip { get; set; } = 0;
        public string SP6IssuerSAMId { get; set; } = null;
        public string SP6StartDate { get; set; } = null;
        public string SP6EndDate { get; set; } = null;
        public string SP6SaleDocNo { get; set; } = null;
        public string SP6ServiceCode { get; set; } = null;
        public string SP6PackageCode { get; set; } = null;
        public string SP6Type { get; set; } = null;
        public byte SP6MaxTripCountPDay { get; set; } = 0;
        public byte SP6IsAvoidChecking { get; set; } = 0;
        public byte SP6IsAvoidTripDurationCheck { get; set; } = 0;
        public string SP6OriginStationNo { get; set; } = null;
        public string SP6DestinationStationNo { get; set; } = null;
        public string SP6LastTravelDate { get; set; } = null;
        public byte SP6DailyTravelTripCount { get; set; } = 0;


        public string SP7SaleDate { get; set; } = null;
        public int SP7MaxTravelAmtPDayPTrip { get; set; } = 0;
        public string SP7IssuerSAMId { get; set; } = null;
        public string SP7StartDate { get; set; } = null;
        public string SP7EndDate { get; set; } = null;
        public string SP7SaleDocNo { get; set; } = null;
        public string SP7ServiceCode { get; set; } = null;
        public string SP7PackageCode { get; set; } = null;
        public string SP7Type { get; set; } = null;
        public byte SP7MaxTripCountPDay { get; set; } = 0;
        public byte SP7IsAvoidChecking { get; set; } = 0;
        public byte SP7IsAvoidTripDurationCheck { get; set; } = 0;
        public string SP7OriginStationNo { get; set; } = null;
        public string SP7DestinationStationNo { get; set; } = null;
        public string SP7LastTravelDate { get; set; } = null;
        public byte SP7DailyTravelTripCount { get; set; } = 0;


        public string SP8SaleDate { get; set; } = null;
        public int SP8MaxTravelAmtPDayPTrip { get; set; } = 0;
        public string SP8IssuerSAMId { get; set; } = null;
        public string SP8StartDate { get; set; } = null;
        public string SP8EndDate { get; set; } = null;
        public string SP8SaleDocNo { get; set; } = null;
        public string SP8ServiceCode { get; set; } = null;
        public string SP8PackageCode { get; set; } = null;
        public string SP8Type { get; set; } = null;
        public byte SP8MaxTripCountPDay { get; set; } = 0;
        public byte SP8IsAvoidChecking { get; set; } = 0;
        public byte SP8IsAvoidTripDurationCheck { get; set; } = 0;
        public string SP8OriginStationNo { get; set; } = null;
        public string SP8DestinationStationNo { get; set; } = null;
        public string SP8LastTravelDate { get; set; } = null;
        public byte SP8DailyTravelTripCount { get; set; } = 0;
    }
}
