namespace Mnemo.Shared.Extensions
{
    public static class DateTimeExtension
    {
        public static int DaysUntilNext(this DateTime dateTime, DayOfWeek targetDay, bool includeTodayIfMatch = false)
        {
            int diff = (targetDay - dateTime.DayOfWeek + 7) % 7;
            if (diff == 0 && !includeTodayIfMatch) diff = 7;
            return diff;
        }
    }
}
