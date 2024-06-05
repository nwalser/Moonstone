using Moonstone.Database;

namespace Sapphire.Data.Entities.SchedulingLocks;

public class MinDateLockAggregate : Document, ILock
{
    public required Guid TodoId { get; set; }
    public required DateOnly MinDate { get; set; }
}