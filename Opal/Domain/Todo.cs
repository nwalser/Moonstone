namespace Opal.Domain;

public record Todo
{
    public required Guid Id { get; init; }
    public required DateTime LastWrite { get; init; }

    public required string Name { get; init; }
    public required Guid ProjectId { get; init; }
}