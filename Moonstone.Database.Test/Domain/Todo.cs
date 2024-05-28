namespace Moonstone.Database.Test.Domain;

public class Todo : Document
{
    public required Guid Id { get; init; }
    public required DateTime LastWrite { get; set; }

    public required string Name { get; init; }
    public required Guid ProjectId { get; init; }
}