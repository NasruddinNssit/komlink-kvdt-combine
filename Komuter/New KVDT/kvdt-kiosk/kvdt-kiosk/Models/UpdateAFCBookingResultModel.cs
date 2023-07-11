using NssIT.Train.Kiosk.Common.Constants;
using NssIT.Train.Kiosk.Common.Helper;
using System;
using System.Collections.Generic;

namespace kvdt_kiosk.Models
{
    public class UpdateAFCBookingResultModel
    {

        public string Booking_Id { get; set; } = "";
        public string BookingNo { get; set; } = "";
        public string MCurrencies_Id { get; set; }

        public decimal TotalAmount { get; set; }
        public DateTime BookingExpiredDateTime { get; set; } = DateTimeHelper.GetMinDate();
        public double BookingRemainingInSecond { get; set; } = 0;
        public string Error { get; set; } = YesNo.No;
        public string ErrorMessage { get; set; } = "";
    }



    public class CheckoutBookingResultModel
    {
        public string Booking_Id { get; set; } = "";
        public string BookingNo { get; set; } = "";
        public DateTime BookingExpiredDateTime { get; set; } = DateTimeHelper.GetMinDate();
        public double BookingRemainingInSecond { get; set; } = 0;
        public string IsRequirePayment { get; set; } = YesNo.Yes;
        public string MCurrencies_Id { get; set; } = "";
        public decimal PayableAmount { get; set; } = 0;
        public string Error { get; set; } = YesNo.No;
        public string ErrorMessage { get; set; } = "";

        public IList<CheckoutPassengerDetailResultModel> PassengerDetailModels { get; set; } = new List<CheckoutPassengerDetailResultModel>();
    }

    public class CheckoutPassengerDetailResultModel
    {
        public Guid SeatLayoutModel_Id { get; set; } = Guid.Empty;

        public string SeatNo { get; set; }

        public string PassengerName { get; set; }

        public string PassengerIC { get; set; }

        public string Gender { get; set; }

        public string PNR { get; set; }

        public string PhoneNo { get; set; }

        public string Email { get; set; }

        public string TicketTypes_Id { get; set; }

        public string Penalties_Id { get; set; } = string.Empty;

        public string BookingType { get; set; }

        public string MCurrencies_Id { get; set; }

        public decimal BeforePromoDiscountTicketPrice { get; set; }

        public decimal PromoDiscountAmount { get; set; }

        public decimal TicketPrice { get; set; }

        public string PromoCode { get; set; } = string.Empty;

        public string PromoError { get; set; } = YesNo.No;

        public string PromoErrorMessage { get; set; } = string.Empty;

        public string IsInsurance { get; set; } = YesNo.No;

        public string MInsuranceHeaders_Id { get; set; } = string.Empty;

        public decimal InsuranceAmount { get; set; } = 0m;


        public string IsError { get; set; } = YesNo.No;

        public string ErrorMessage { get; set; } = string.Empty;

    }

}
