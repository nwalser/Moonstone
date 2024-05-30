using Moonstone.Database;

namespace Sapphire.Data.ProjectData.Entities;

public class Project : Document
{
    public required string Name { get; set; }
    
    public DateOnly? Deadline { get; set; }
}