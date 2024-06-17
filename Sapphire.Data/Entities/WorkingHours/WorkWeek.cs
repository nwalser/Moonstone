using Moonstone.Database;

namespace Sapphire.Data.Entities.WorkingHours;

public class WorkWeek : Document
{
    public required Guid WorkerId { get; set; }
    
    public Dictionary<DayOfWeek, TimeSpan> DailyWorkingHours { get; set; } = new();
    
    public DateOnly? ActiveFrom { get; set; }
    public DateOnly? ActiveTo { get; set; }

    public TimeSpan GetWorkingHours(DayOfWeek day)
    {
        if (DailyWorkingHours.TryGetValue(day, out var workingHours))
            return workingHours;
        
        return TimeSpan.Zero;
    }

    public void Delete(ProjectDatabase db)
    {
        db.Remove(this);
    }
}