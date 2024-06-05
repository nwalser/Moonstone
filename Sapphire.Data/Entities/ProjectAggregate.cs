using Moonstone.Database;

namespace Sapphire.Data.Entities;

public class ProjectAggregate : Document
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public DateOnly Start { get; set; }
    public DateOnly? Deadline { get; set; }

    public string[] PossibleTags { get; set; } = [];
}