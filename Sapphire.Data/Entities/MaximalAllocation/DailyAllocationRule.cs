using Moonstone.Database;

namespace Sapphire.Data.Entities.MaximalAllocation;

public class DailyAllocationRule : Document
{
    public required Guid WorkerId { get; set; }
    public required Guid ProjectId { get; set; }
    public required TimeSpan MaximalAllocation { get; set; }
    
    public DateOnly ActiveFrom { get; set; }
    public DateOnly ActiveTo { get; set; }
}