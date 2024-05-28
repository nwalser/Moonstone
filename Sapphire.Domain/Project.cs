using Moonstone.Database;

namespace Sapphire.Domain;

public class Project : Document
{
    public required string Name { get; set; }
    
    public DateOnly? Deadline { get; set; }
}