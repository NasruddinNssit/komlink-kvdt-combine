using System.Collections.Generic;

namespace kvdt_kiosk.Models
{
    public static class PassengerInfo
    {
        public static string PassengerName { get; set; } = null;
        public static string PassengerType { get; set; } = null;
        public static string ICNumber { get; set; }
        public static string DynamicInfo { get; set; }
        public static string JourneyButtonText { get; set; }
        public static int CurrentScanNumberForChild { get; set; }
        public static int CurrentScanNumberForAdult { get; set; }
        public static int CurrentScanNumberForSenior { get; set; }
        public static string ScanFor { get; set; }
        public static bool IsBtnScanScanning { get; set; }
        public static bool IsPaxSelected { get; set; } = false;
        public static bool IsMyKadScanSelected { get; set; } = false;
        public static int Age { get; set; }
        public static int MinChildAge { get; set; }
        public static int MaxChildAge { get; set; }
        public static int MinSeniorAge { get; set; }
        public static int MaxSeniorAge { get; set; }
        public static IList<string> IcScanned { get; set; } = new List<string>();
    }
}
