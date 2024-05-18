namespace Moonstone;

public record DocumentIdentifier
{
    public required Type Type { get; init; }
    public required Guid Id { get; init; }
}