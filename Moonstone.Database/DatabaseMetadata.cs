namespace Moonstone.Database;

public record DatabaseMetadata
{
    public required string Type { get; set; }
    public required DateTime Created { get; set; }
}