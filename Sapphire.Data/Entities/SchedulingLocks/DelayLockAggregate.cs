using Moonstone.Database;

namespace Sapphire.Data.Entities.SchedulingLocks;

public class DelayLockAggregate : Document, ILock
{
    public required Guid TodoId { get; set; }
    public required Guid TodoLockerId { get; set; }
    public required int DelayInDays { get; set; }
}