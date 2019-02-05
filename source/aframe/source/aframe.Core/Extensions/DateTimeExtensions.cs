using System;

namespace aframe
{
    public static class DateTimeExtensions
    {
        public static int ToInt(
            this DateTime date) => int.Parse(date.ToString("yyyyMMdd"));

        public static decimal ToDecimal(
            this DateTime date) => decimal.Parse(date.ToString("yyyyMMdd"));

        public static DateTime GetFirstDayOfMonth(
            this DateTime date) => new DateTime(date.Year, date.Month, 1);

        public static DateTime GetEndDayOfMonth(
            this DateTime date) => new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

        public static DateTime GetDayOfSameMonth(
            this DateTime date,
            int day)
        {
            if (day > DateTime.DaysInMonth(date.Year, date.Month))
            {
                day = DateTime.DaysInMonth(date.Year, date.Month);
            }

            if (day < 1)
            {
                day = 1;
            }

            return new DateTime(date.Year, date.Month, day);
        }

        public static DateTime GetDateOnly(
            this DateTime date) => new DateTime(date.Year, date.Month, date.Day);

        public static DateTime GetDbMinValue(
            this DateTime date) => DateTimeConverter.DbMinValue;

        public static DateTime SetDbMinValue(
            this DateTime date) => date = DateTimeConverter.DbMinValue;
    }

    public static class DateTimeConverter
    {
        public static readonly DateTime DbMinValue = DateTime.Parse("1800-1-1");

        public static DateTime FromInt(
            int dateTime) => DateTime.Parse(dateTime.ToString("0000/00/00"));

        public static DateTime FromInt(
            decimal dateTime) => DateTime.Parse(dateTime.ToString("0000/00/00"));
    }
}
