namespace Sapphire.Data.Extensions;

public static class DateTimeExtensions
{
    public static DateOnly ToDateOnly(this DateTime datetime) 
        => DateOnly.FromDateTime(datetime);
}