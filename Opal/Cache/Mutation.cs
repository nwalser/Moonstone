using Opal.Stream;
using ProtoBuf;

namespace Opal.Cache;

public class Mutation
{
    /// <summary>
    /// id field that is created in sequence with comb guid provider
    /// </summary>
    public required Guid Id { get; init; }
    public required bool CacheInvalidated { get; set; }
    
    public required byte[] Data { get; init; }

    public static Mutation FromMutationEnvelope<TMutation>(MutationEnvelope<TMutation> mutationEnvelope)
    {
        var stream = new MemoryStream();
        Serializer.Serialize(stream, mutationEnvelope.Mutation);

        return new Mutation()
        {
            Id = mutationEnvelope.Id,
            Data = stream.ToArray(),
            CacheInvalidated = false,
        };
    }
}