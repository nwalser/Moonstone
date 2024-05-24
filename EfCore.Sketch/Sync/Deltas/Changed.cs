namespace EfCore.Sketch.Sync.Deltas;

public record Changed : IDelta
{
    public required long Ticks { get; init; }
    
    public required string Table { get; init; }
    public required string Row { get; init; }
    public required string Column { get; init; }
    
    public required string Value { get; init; }
}