namespace Moonstone.Database.Test.Domain;

public class Project : Document
{
    public required Guid Id { get; init; }
    public DateTime LastWrite { get; set; } = DateTime.MinValue;
    
    public required string Name { get; set; }
}