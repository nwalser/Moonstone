using Moonstone.Database;

namespace Sapphire.Data.Entities;

public class TodoAggregate : Document
{
    public required string Name { get; set; }
    
    public required Guid ProjectId { get; init; }

    public Guid? ParentId { get; set; } = Guid.Empty;
    public uint Order { get; set; } = 0;
    
    public TimeSpan InitialEstimatedEffort { get; set; } = TimeSpan.Zero;
    public TimeSpan CurrentEstimatedEffort { get; set; } = TimeSpan.Zero;

    public bool Splittable { get; set; } = false;
    public int Priority { get; set; } = 0;
}