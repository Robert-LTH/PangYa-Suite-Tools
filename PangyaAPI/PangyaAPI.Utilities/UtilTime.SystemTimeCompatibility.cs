using System;
using System.Runtime.InteropServices;

namespace PangyaAPI.Utilities
{
    public partial class UtilTime
    {
        public static DateTime ToDateTime(SystemTime value)
        {
            ArgumentNullException.ThrowIfNull(value);

            try
            {
                return value.ConvertTime();
            }
            catch (ArgumentOutOfRangeException)
            {
                return DateTime.MinValue;
            }
        }

        public static long GetTimeDiff(SystemTime first, SystemTime second) =>
            GetTimeDiff(first.ConvertTime(), second.ConvertTime());

        public static long GetHourDiff(SystemTime first, SystemTime second) =>
            GetHourDiff((SYSTEMTIME)first, (SYSTEMTIME)second);

        public static bool IsSameDay(SystemTime first, SystemTime second) =>
            IsSameDay(first.ConvertTime(), second.ConvertTime());

        public static bool IsSameDay(SystemTime value) => IsSameDay(value.ConvertTime(), DateTime.Now);

        public static bool IsEmpty(SystemTime value) => value.IsEmpty || value.ConvertTime() == DateTime.MinValue;

        public static long GetLocalDateDiff(SystemTime value) => GetLocalDateDiff((SYSTEMTIME)value);

        public static long GetLocalDateDiffDESC(SystemTime value) =>
            _GetLocalTimeDiffDESC(value.ConvertTime()).Ticks;

        public static string FormatDate(SystemTime value) => FormatDate(value.ConvertTime());

        public static string _formatDate(SystemTime value) => FormatDate(value);

        public static long SystemTimeToUnix(SystemTime value) => SystemTimeToUnix(value.ConvertTime());

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void GetLocalTime(ref SystemTime value);

        public static bool IsExpired(DateTime value) => DateTime.Now >= value;

        public static bool IsExpired(SystemTime value) => IsExpired(value.ConvertTime());

        public static long GetLocalTimeDiff(SystemTime value) => GetLocalTimeDiff(value.ConvertTime());

        public static long TzLocalTimeToUnixUTC(SystemTime value) =>
            new DateTimeOffset(value.ConvertTime()).ToUnixTimeSeconds();

        public static int GetDateDiff(SystemTime first, SystemTime second) =>
            GetDateDiff((SYSTEMTIME)first, (SYSTEMTIME)second);

        public static long GetTickCount() => Environment.TickCount64;
    }
}
