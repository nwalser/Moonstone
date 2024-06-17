using Moonstone.Database;

namespace Sapphire.Data.Entities.WorkingHours;

public class LeaveDayAggregate : Document
{
    public Guid WorkerId { get; set; }
    public DateOnly Date { get; set; }

    
    public static List<LeaveDayAggregate> FromRange(Guid workerId, DateOnly start, DateOnly end)
    {
        return Enumerable.Range(0, end.DayNumber - start.DayNumber + 1)
            .Select(start.AddDays)
            .Select(d => new LeaveDayAggregate()
            {
                WorkerId = workerId,
                Date = d
            })
            .ToList();
    }
    
    public static IEnumerable<IGrouping<DateOnly, LeaveDayAggregate>> GetGrouped(IEnumerable<LeaveDayAggregate> leaveDays)
    {
        return leaveDays
            .DistinctBy(d => d.Date)
            .OrderBy(d => d.Date)
            .Select((d, i) => new { date = d, key = d.Date.AddDays(-i) })
            .GroupBy(d => d.key, tuple => tuple.date);
    }
    
    public void Delete(ProjectDatabase db)
    {
        db.Remove(this);
    }
}