using Moonstone.Database;

namespace Sapphire.Data.Project.Entities;

public class Project : Document
{
    public required string Name { get; set; }
    
    public DateOnly? Deadline { get; set; }
}