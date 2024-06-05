using System.Globalization;

namespace Sapphire.App.Services;

public class TimeZoneService
{
    public TimeZoneService()
    {
        TimeZoneInfo = TimeZoneInfo.Local;
    }

    private TimeZoneInfo TimeZoneInfo { get; }

    public DateTime ToLocalTime(DateTime utcTime)
    {
        var userTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo);
        return userTime;
    }

    public DateTime ToUtcTime(DateTime localTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(localTime, TimeZoneInfo);
    }

    public string ToLocalTimeString(DateTime utcTime)
    {
        var userTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo);
        return userTime.ToString(CultureInfo.CurrentCulture);
    }
}