using NssIT.Train.Kiosk.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class PassengerResultDetailModel
    {
        public Guid TripBookingDetails_Id { get; set; } = Guid.Empty;
        public Guid SeatLayoutModel_Id { get; set; } = Guid.Empty;
        public string PassengerName { get; set; }
        public string PassengerIC { get; set; }
        public string Gender { get; set; }
        public string PNR { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public string TicketTypes_Id { get; set; }
        public string BookingType { get; set; }
        public string TrainNo { get; set; }
        public string TrainLabel { get; set; }
        public string TrainService { get; set; }
        public string CoachLabel { get; set; }
        public string SeatNo { get; set; }
        public DateTime DepartureLocalDateTime { get; set; }
        public string DepartureDateTimeFormat { get; set; }
        public string DepartureStationDescription { get; set; }
        public string DepartureStationId { get; set; }
        public DateTime ArrivalLocalDateTime { get; set; }
        public string ArrivalDateTimeFormat { get; set; }
        public string ArrivalStationDescription { get; set; }
        public string ArrivalStationId { get; set; }
        public string Currency { get; set; }
        public string PromoCode { get; set; }
        public string TicketPriceFormat { get; set; }
        public decimal TicketPrice { get; set; }
        public string BeforePromoDiscountTicketPriceFormat { get; set; }
        public decimal BeforePromoDiscountTicketPrice { get; set; }
        public string PromoDiscountAmountFormat { get; set; }
        public decimal PromoDiscountAmount { get; set; }
        public string TicketNo { get; set; }
        public string CoachId { get; set; }
        public int SeatIndex { get; set; }
        public string SeatTypeDescription { get; set; }
        public string ServiceTypeDescription { get; set; }
        public string QRCodeValue { get; set; }
        public string IsOKUSeatType { get; set; }
        public string IsLedger { get; set; }
        public string FreeDutyPass { get; set; }
        public string IsCheckIn { get; set; }
        public string IsPenalty { get; set; }
        public string Penalties_Id { get; set; }
        public string PenaltyDescription { get; set; } = string.Empty;
        public decimal PenaltyAmount { get; set; }
        public string IsReference { get; set; }
        public Guid Reference_TripBookingDetails_Id { get; set; } = Guid.Empty;
        public decimal AmendAmount { get; set; }

        /// <summary>
        /// YesNo
        /// </summary>
        public string IsInsurance { get; set; } = YesNo.No;
        public string MInsuranceHeaders_Id { get; set; } = string.Empty;
        public decimal InsuranceAmount { get; set; } = 0m;

        public PassengerResultDetailModel ReferencePassengerDetailModel { get; set; }
    }
}

