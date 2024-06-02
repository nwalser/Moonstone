using Moonstone.Database;

namespace Sapphire.Data.Entities;

public class Project : Document
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public DateOnly? Deadline { get; set; }
}