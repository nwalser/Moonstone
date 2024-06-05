namespace Sapphire.Data.Extensions;

public static class DateOnlyExtensions
{
    public const DayOfWeek WeekStart = DayOfWeek.Monday;
    
    public static DateOnly StartOfWeek(this DateOnly dt)
    {
        var diff = (7 + (dt.DayOfWeek - WeekStart)) % 7;
        return dt.AddDays(-1 * diff);
    }
}