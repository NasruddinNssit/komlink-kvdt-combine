using System;

namespace kvdt_kiosk.Models
{

    public static class UserSession
    {
        public static string SessionId { get; set; }
        public static DateTime CurrentDateTime { get; set; }
        public static string FromStation { get; set; }
        public static string ToStation { get; set; }
        public static string JourneyType { get; set; }
        public static int AdultSeat { get; set; }
        public static int ChildSeat { get; set; }
        public static int SeniorSeat { get; set; }
        public static int TotalSeat { get; set; }
        public static decimal TotalAmount { get; set; }
        public static string MyKadNumber { get; set; }
    }

}
