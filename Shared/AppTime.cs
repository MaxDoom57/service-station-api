using System;

namespace System
{
    public static class AppTime
    {
        public static DateTime Now
        {
            get
            {
                try
                {
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Sri Lanka Standard Time"));
                }
                catch (TimeZoneNotFoundException)
                {
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Colombo"));
                }
            }
        }
    }
}

