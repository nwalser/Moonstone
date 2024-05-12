using Opal.Mutations;
using Opal.Projection;
using Opal.Stream;

namespace Opal.Cache;

public class Snapshot
{
    public required Guid Id { get; set; }
    public required Guid LastMutationId { get; set; }
    
    public required int NumberOfAppliedMutations { get; set; }
    
    public required byte[] Projection { get; set; }


    public void NewId()
    {
        Id = Guid.NewGuid();
    }
    
    public void ApplyMutation(MutationEnvelope<MutationBase> mutation)
    {
        if (mutation.Id <= LastMutationId)
            throw new InvalidOperationException();
        
        LastMutationId = mutation.Id;
        NumberOfAppliedMutations++;
    }
    
    
    public static Snapshot Create(int numberOfAppliedMutations, byte[] projection)
    {
        return new Snapshot()
        {
            Id = Guid.NewGuid(),
            LastMutationId = Guid.Empty,
            NumberOfAppliedMutations = numberOfAppliedMutations,
            Projection = projection
        };
    }

    public static Snapshot Copy(Snapshot parent)
    {
        return new Snapshot()
        {
            Id = Guid.NewGuid(),
            LastMutationId = parent.LastMutationId,
            NumberOfAppliedMutations = parent.NumberOfAppliedMutations,
            Projection = parent.Projection.ToArray()
        };
    }
}