using Sapphire.Data.Entities.SchedulingLocks;

namespace Sapphire.Data.ValueObjects;

public class ParentLock : ILock
{
    public Guid TodoId { get; set; }
    public Guid TodoLockerId { get; set; }

    public ParentLock(Guid todoId, Guid todoLockerId)
    {
        TodoId = todoId;
        TodoLockerId = todoLockerId;
    }
}