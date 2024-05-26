namespace Opal.Domain;

public record Todo : IDocument
{
    public required Guid Id { get; init; }
    public required DateTime LastWrite { get; set; }

    public required string Name { get; init; }
    public required Guid ProjectId { get; init; }
}