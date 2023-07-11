using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Helper
{
    public static class DateTimeHelper
    {
        private static string _timeZoneId = "";
        private static string timeZoneId
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_timeZoneId))
                    _timeZoneId = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting().TimeZoneId;

                return _timeZoneId;
            }
        }

        public static string ToLocalDateOnlyFormat(this DateTime time)
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime myTime = TimeZoneInfo.ConvertTimeFromUtc(time, timeZone);
            return myTime.ToDateOnlyFormat();
        }

        public static string ToLocalDateTimeFormat(this DateTime time)
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime myTime = TimeZoneInfo.ConvertTimeFromUtc(time, timeZone);
            return myTime.ToDateTimeFormat();
        }

        public static DateTime ConvertToLocal(this DateTime time)
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime myTime = TimeZoneInfo.ConvertTimeFromUtc(time, timeZone);
            return myTime;
        }

        public static DateTime ConvertToUTC(this DateTime time)
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime myTime = TimeZoneInfo.ConvertTimeToUtc(time, timeZone);
            return myTime;
        }

        //public static string GetDate(DateTime date)
        //{
        //	return ToDayDateTimeFormat(date).ToString().Substring(0, 11);
        //}

        //public static string GetHour(DateTime date)
        //{
        //	return ToSQLFormat(date).ToString().Substring(11, 2);
        //}

        public static DateTime GetMinDate()
        {
            return System.Data.SqlTypes.SqlDateTime.MinValue.Value;
        }

        //public static string GetMinute(DateTime date)
        //{
        //	return ToSQLFormat(date.ConvertToLocal()).ToString().Substring(14, 2);
        //}

        //public static string GetMonth(DateTime date)
        //{
        //	return ToReportNameLocalFormat(date.ConvertToLocal()).Substring(4, 2);
        //}

        public static DateTime GetNow()
        {
            return DateTime.UtcNow;
            //return DateTime.Now;
        }

        //public static DateTime GetTimeFromUTC(DateTime time)
        //{
        //	TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
        //	DateTime myTime = TimeZoneInfo.ConvertTimeFromUtc(time, timeZone);
        //	return myTime;
        //}

        //public static string GetYear(DateTime date)
        //{
        //	return ToReportNameLocalFormat(date.ConvertToLocal()).Substring(0, 4);
        //}

        //public static string StartDate(DateTime date)
        //{
        //	return date.ConvertToLocal().ToString();
        //}

        public static string ToDateOnlyFormat(this DateTime timeForFormat)
        {
            return timeForFormat.ToString("dd MMM yyyy");
        }

        //public static string ToDayDateOnlyFormat(this DateTime timeForFormat)
        //{
        //	return timeForFormat.ToString(Constants.DateTimeFormat.DisplayDayDateFormat);
        //}

        //public static string ToDayDateTimeFormat(this DateTime timeForFormat)
        //{
        //	return timeForFormat.ToString(Constants.DateTimeFormat.DisplayDayDateTimeFormat);
        //}

        //public static string ToTimeOnlyFormat(this DateTime timeForFormat)
        //{
        //	return timeForFormat.ToString(Constants.DateTimeFormat.DisplayTimeFormat);
        //}

        //public static string ToHTMLFormat(this DateTime timeForFormat)
        //{
        //	return timeForFormat.ConvertToLocal().ToString(Constants.DateTimeFormat.HTMLFormat).Replace(" ", "&nbsp;");
        //}

        //public static string ToJavascriptFormat(this DateTime timeForConvert)
        //{
        //	return timeForConvert.ConvertToLocal().ToString(Constants.DateTimeFormat.JavascriptFormat);
        //}

        //public static string ToJsonFormat(this DateTime timeForFormat)
        //{
        //	return "LocalTime=" + timeForFormat.ConvertToLocal().ToString(Constants.DateTimeFormat.DisplayDayDateTimeFormat);
        //}

        //public static string ToReceiptDateTimeFormat(this DateTime timeForFormat)
        //{
        //	return timeForFormat.ToString(Constants.DateTimeFormat.DisplayReceiptDateTimeFormat);
        //}

        //public static string ToReportNameLocalFormat(this DateTime timeForConvert)
        //{
        //	return DateTime.UtcNow.ConvertToLocal().ToString("yyyyMMdd_HHmmss");
        //}

        //public static string ToScheduleTimeFormat(this DateTime timeForFormat)
        //{
        //	return timeForFormat.ConvertToLocal().ToString(Constants.DateTimeFormat.ScheduleTimeFormat);
        //}

        public static DateTime ParseIso8601Date(string dateToConvert)
        {
            return DateTime.ParseExact(dateToConvert, Constants.DateTimeFormat.Iso8601DateFormat, CultureInfo.InvariantCulture);
        }

        public static DateTime ParseIso8601DateTime(string dateToConvert)
        {
            return DateTime.ParseExact(dateToConvert, Constants.DateTimeFormat.Iso8601DateTimeFormat, CultureInfo.InvariantCulture);
        }

        public static string ToSQLFormat(this DateTime timeForConvert)
        {
            return timeForConvert.ToString(Constants.DateTimeFormat.SQLFormat);
        }

        public static string ToIso8601Date(this DateTime timeForConvert)
        {
            return timeForConvert.ToString(Constants.DateTimeFormat.Iso8601DateFormat);
        }

        public static string ToIso8601DateTime(this DateTime timeForConvert)
        {
            return timeForConvert.ToString(Constants.DateTimeFormat.Iso8601DateTimeFormat);
        }

        //public static string ToSQLUTC(this DateTime timeForConvert)
        //{
        //	return timeForConvert.ConvertToUTC().ToString(Constants.DateTimeFormat.SQLFormat);
        //}

        //public static string ToTimeFormat(this DateTime timeForFormat)
        //{
        //	return timeForFormat.ConvertToLocal().ToString(Constants.DateTimeFormat.DisplayTimeFormat);
        //}


        public static string ToDateTimeFormat(this DateTime timeForFormat)
        {

            return timeForFormat.ToString("dd MMM yyyy hh:mm:ss tt");
        }
    }
}
