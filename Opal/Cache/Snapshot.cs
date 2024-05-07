namespace Opal.Cache;

public class Snapshot
{
    public required Guid Id { get; init; }
    public required Guid LastMutationId { get; set; }
    
    public required int MinAge { get; set; }
    public required int MaxAge { get; set; }
    
    public required byte[] Projection { get; set; }

    
    public static Snapshot Create(int minAge, int maxAge, byte[] projection)
    {
        return new Snapshot()
        {
            Id = Guid.NewGuid(),
            LastMutationId = Guid.Empty,
            MinAge = minAge,
            MaxAge = maxAge,
            Projection = projection
        };
    }

    public static Snapshot Copy(int minAge, int maxAge, Snapshot parent)
    {
        return new Snapshot()
        {
            Id = Guid.NewGuid(),
            LastMutationId = parent.LastMutationId,
            MinAge = minAge,
            MaxAge = maxAge,
            Projection = parent.Projection.ToArray() // copy data
        };
    }
}