using System.Collections.Generic;

namespace kvdt_kiosk.Models
{

    public static class UserSession
    {
        public static string SessionId { get; set; }
        public static string KioskId { get; set; }
        public static string AFCService { get; set; }
        public static string FromStationId { get; set; }
        public static string FromStationName { get; set; }
        public static string ToStationId { get; set; }
        public static string ToStationName { get; set; }
        public static string JourneyType { get; set; }
        public static string JourneyTypeId { get; set; }
        public static string JourneyDuration { get; set; }
        public static int ChildSeat { get; set; } = 0;
        public static int SeniorSeat { get; set; } = 0;
        public static int OKUSeat { get; set; } = 0;
        public static bool IsVerifyAgeAFCRequiredForChild { get; set; } = false;
        public static bool IsVerifyAgeAFCRequiredForSenior { get; set; } = false;
        public static bool IsVerifyAgeAFCRequiredForOKU { get; set; } = false;
        public static int TempDataForChildSeat { get; set; } = 0;
        public static int TempDataForSeniorSeat { get; set; } = 0;
        public static int TempDataForOKUSeat { get; set; } = 0;
        public static string CurrencyCode { get; set; } = "MYR";
        public static int AgeMaxAFC { get; set; }
        public static bool IsCheckOut { get; set; } = false;
        public static bool IsParcelCheckOut { get; set; } = false;

        public static bool isParcelHaveClicked = false;

        public static decimal TotalTicketPrice { get; set; }
        public static List<UserAddon> UserAddons { get; set; }
        public static List<TicketOrderType> TicketOrderTypes { get; set; }

        public static string MCounters_Id = "";
        public static string CounterUserId = "";
        public static string HandheldUserId = "";
        public static string PurchaseStationId = "";

        public static UpdateAFCBookingResultModel UpdateAFCBookingResultModel { get; set; }

        public static CheckoutBookingResultModel CheckoutBookingResultModel { get; set; }

        public static string FinancePaymentMethod { get; set; }

        public static string PaymentGateWaySalesTransactionNo { get; set; }
        public static bool IsPrintingDone { get; set; } = false;
    }

    public class UserAddon
    {
        public string AddOnId { get; set; }
        public string AddOnName { get; set; }
        public int AddOnCount { get; set; }
        public decimal AddOnPrice { get; set; }
    }

    public class TicketOrderType
    {
        public bool IsConcession { get; set; }
        public bool IsSinglePassengerOnly { get; set; }
        public string TicketTypeId { get; set; }
        public string TicketTypeName { get; set; }
        public string TicketCount { get; set; }
        public decimal UnitPrice { get; set; }
        public bool VerifyMalaysianAFC { get; set; }
        public bool IsVerifyAgeAFCRequired { get; set; }
        public int AgeMinAFC { get; set; }
        public int AgeMaxAFC { get; set; }
        public bool IsVerifyPeopleSoftRequired { get; set; }
        public string IsPNRAFCRequired { get; set; }
        public int NoOfPax { get; set; }
        public decimal TotalPrice { get; set; }
    }


}
