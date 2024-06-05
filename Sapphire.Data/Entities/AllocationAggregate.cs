using Moonstone.Database;

namespace Sapphire.Data.Entities;

public class AllocationAggregate : Document
{
    public required Guid TodoId { get; init; }
    public required Guid WorkerId { get; init; }
    
    public required DateOnly Date { get; init; }
    public required TimeSpan AllocatedTime { get; init; }
}