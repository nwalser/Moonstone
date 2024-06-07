using Moonstone.Database;

namespace Sapphire.Data.Entities.WorkingHours;

public class OfficeDayAggregate : Document
{
    public Guid WorkerId { get; set; }
    public DateOnly Date { get; set; }
    public TimeSpan WorkingHours { get; set; }
    
    
    public static List<OfficeDayAggregate> FromRange(Guid workerId, TimeSpan workingHours, DateOnly start, DateOnly end)
    {
        return Enumerable.Range(0, end.DayNumber - start.DayNumber + 1)
            .Select(start.AddDays)
            .Select(d => new OfficeDayAggregate()
            {
                WorkerId = workerId,
                Date = d,
                WorkingHours = workingHours,
            })
            .ToList();
    }
    
    public static IEnumerable<IGrouping<DateOnly, OfficeDayAggregate>> GetGrouped(IEnumerable<OfficeDayAggregate> leaveDays)
    {
        return leaveDays
            .DistinctBy(d => d.Date)
            .OrderBy(d => d.Date)
            .Select((d, i) => new { date = d, key = d.Date.AddDays(-i) })
            .GroupBy(d => d.key, tuple => tuple.date);
    }
}