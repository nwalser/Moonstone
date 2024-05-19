namespace Moonstone;

public record DocumentIdentity
{
    public required Type Type { get; init; }
    public required int TypeId { get; init; }
    public required Guid Id { get; init; }
}