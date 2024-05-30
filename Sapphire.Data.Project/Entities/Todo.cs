using Moonstone.Database;

namespace Sapphire.Data.Project.Entities;

public class Todo : Document
{
    public required string Name { get; set; }
    
    public TimeSpan InitialEstimatedEffort { get; set; } = TimeSpan.Zero;
    public TimeSpan CurrentEstimatedEffort { get; set; } = TimeSpan.Zero;

    public bool Splittable { get; set; } = false;
    public int Priority { get; set; } = 0;
}