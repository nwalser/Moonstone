namespace Moonstone.Database;

public record DatabaseMetadata
{
    public required string Type { get; init; }
    public required DateTime Created { get; init; }
}