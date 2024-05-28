namespace Moonstone.FileFormat.Domain;

public class Todo : IDocument
{
    public required Guid Id { get; init; }
    public required DateTime LastWrite { get; set; }

    public required string Name { get; init; }
    public required Guid ProjectId { get; init; }
}