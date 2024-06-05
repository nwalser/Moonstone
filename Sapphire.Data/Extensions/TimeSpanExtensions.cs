namespace Sapphire.Data.Extensions;

public static class TimeSpanExtensions
{
    public static TimeSpan Sum(IEnumerable<TimeSpan> timeSpans)
    {
        return new TimeSpan(timeSpans.Select(t => t.Ticks).Sum());
    }

    public static TimeSpan Min(IEnumerable<TimeSpan> timeSpans)
    {
        return new TimeSpan(timeSpans.Select(t => t.Ticks).Min());
    }
}