namespace EfCore.Sketch.Sync.Deltas;

public class Deleted
{
    public required long Ticks { get; init; }

    public required Type Type { get; init; }
    public required Guid Id { get; init; }
}