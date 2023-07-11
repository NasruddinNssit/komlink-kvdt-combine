using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Constants
{
    public static class DateTimeFormat
    {
        //public const string DisplayDayDateTimeFormat = "ddd, d MMM yyyy / HH:mm";
        //public const string DisplayDateFormat = "d MMM yyyy";
        //public const string DisplayReceiptDateTimeFormat = "d MMM yyyy / HH:mm:ss";
        //public const string DisplayDayDateFormat = "ddd, d MMM";
        public const string DisplayTimeFormat = "HH:mm";
        //public const string HTMLFormat = "dd MMM yyyy    hh:mm:ss tt";
        //public const string JavascriptFormat = "MMM dd, yyyy HH:mm:ss";
        //public const string LightpickDateJsFormat = "D MMM YYYY";
        //public const string ScheduleTimeFormat = "hh:mm tt";
        public const string SQLFormat = "yyyy-MM-dd HH:mm:ss";
        public const string Iso8601DateFormat = "yyyy-MM-dd";
        public const string Iso8601DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
        public const string DisplayDateOnlyFormat = "dd MMM yyyy";
        public const string LightpickDateJsFormat = "D MMM YYYY";
    }

    public static class YesNo
    {
        public const string Yes = "1";
        public const string No = "0";
    }

    public static class Gender
    {
        public const string Female = "F";
        public const string Male = "M";
    }

    public static class SeatStatus
    {
        public const string Blocked = "0";
        public const string Available = "1";
        public const string Reserved = "2";
        public const string SoldMale = "3";
        public const string SoldFemale = "4";
        public const string NotForSale = "5";
        public const string Sold = "6";
        public const string Refund = "8";
        public const string Cancelled = "9";
    }

    public static class FinancePaymentMethod
    {
        public const string Maybank = "Maybank";
        public const string PublicBank = "PublicBank";
        public const string IPay88 = "IPay88";

        public const string Boost = "Boost";
        public const string GrabPay = "GrabPay";
        public const string TngEWallet = "TngEWallet";

        public const string BillPlz = "BillPlz";
        public const string eGHL = "eGHL";
        public const string PayNet = "PayNet";

        public const string Cash = "Cash";
        public const string EWallet = "EWallet";
        public const string CreditCard = "CreditCard";
        public const string DebitCard = "DebitCard";
        public const string Ledger = "Ledger";
        public const string DutyPass = "DutyPass";
        public const string FreePass = "FreePass";

        public const string CreditCardAmex = "CreditCardAmex";
        public const string CreditCardVisa = "CreditCardVisa";
        public const string CreditCardMaster = "CreditCardMaster";
        public const string CreditCardOther = "CreditCardOther";
        public const string DebitCardVisa = "DebitCardVisa";
        public const string DebitCardAmex = "DebitCardAmex";
        public const string DebitCardMaster = "DebitCardMaster";
        public const string DebitCardOther = "DebitCardOther";
        //public const string NoPaymentRequired = "NoPaymentRequired";

        public const string MergeTrip = "MergeTrip";
    }
}
