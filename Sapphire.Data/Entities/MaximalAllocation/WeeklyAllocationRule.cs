using Moonstone.Database;

namespace Sapphire.Data.Entities.MaximalAllocation;

public class WeeklyAllocationRule : Document
{
    public required Guid WorkerId { get; set; }
    public required Guid ProjectId { get; set; }

    public Dictionary<DayOfWeek, TimeSpan> MaximalAllocations { get; set; } = new();

    public DateOnly? ActiveFrom { get; set; }
    public DateOnly? ActiveTo { get; set; }
    
    public void Delete(ProjectDatabase db)
    {
        db.Remove(this);
    }
}