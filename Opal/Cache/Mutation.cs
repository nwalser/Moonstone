using Opal.Log;
using ProtoBuf;

namespace Opal.Cache;

public class Mutation
{
    public required Guid Id { get; init; }
    public required bool Projected { get; set; }
    
    public required byte[] Data { get; init; }

    public static Mutation FromMutationEnvelope<TMutation>(MutationEnvelope<TMutation> mutationEnvelope)
    {
        var stream = new MemoryStream();
        Serializer.Serialize(stream, mutationEnvelope.Mutation);

        return new Mutation()
        {
            Id = mutationEnvelope.Id,
            Data = stream.ToArray(),
            Projected = false,
        };
    }
}