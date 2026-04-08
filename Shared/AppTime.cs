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
                    var slTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Sri Lanka Standard Time"));
                    return DateTime.SpecifyKind(slTime, DateTimeKind.Local);
                }
                catch (TimeZoneNotFoundException)
                {
                    var slTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Colombo"));
                    return DateTime.SpecifyKind(slTime, DateTimeKind.Local);
                }
            }
        }
    }
}

