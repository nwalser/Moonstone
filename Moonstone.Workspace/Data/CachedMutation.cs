namespace Moonstone.Workspace.Data;

public class CachedMutation
{
    public required Guid MutationId { get; init; }
    public required byte[] Mutation { get; init; }
    
    
    public static CachedMutation FromMutationEnvelope(MutationEnvelope envelope)
    {
        return new CachedMutation()
        {
            MutationId = envelope.Id,
            Mutation = envelope.Mutation.Serialize(),
        };
    }
    
    public static MutationEnvelope ToMutationEnvelope(CachedMutation cached)
    {
        return new MutationEnvelope()
        {
            Id = cached.MutationId,
            Mutation = IMutation.Deserialize(cached.Mutation)
        };
    }
}