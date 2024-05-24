namespace EfCore.Sketch.Sync.Deltas;

public record Changed
{
    public required long Ticks { get; init; }
    
    public required Type Type { get; init; }
    public required Guid Id { get; init; }
    
    public required string Field { get; init; }
    public required object? Value { get; init; }
}