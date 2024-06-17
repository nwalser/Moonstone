using Moonstone.Database;

namespace Sapphire.Data.Entities;

public class AllocationAggregate : Document
{
    public required Guid TodoId { get; init; }
    public required Guid WorkerId { get; init; }
    
    public required DateOnly Date { get; init; }
    public TimeSpan AllocatedTime { get; set; } = TimeSpan.Zero;

    public void AddEffort(TimeSpan duration)
    {
        AllocatedTime += duration;
    }

    public void Clear()
    {
        AllocatedTime = TimeSpan.Zero;
    }

    public void Delete(Database db)
    {
        db.Remove(this);
    }
}