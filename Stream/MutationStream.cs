using Stream.Mutations;

namespace Stream;

public class MutationStream
{
    public required List<Mutation> Mutations { get; set; }


    public bool MutationExists(Guid mutationId)
    {
        return Mutations.Any(m => m.MutationId == mutationId);
    }
    
    public void IngestMutation(Mutation mutation)
    {
        Mutations.Add(mutation);
    }
}