namespace Abstractions;

public class MutationEnvelopeSerializer<TMutation> : ITextSerializer<MutationEnvelope<TMutation>>
{
    private readonly IBinarySerializer<TMutation> _mutationSerializer;
    private readonly ITextSerializer<byte[]> _byteSerializer;

    public MutationEnvelopeSerializer(IBinarySerializer<TMutation> mutationSerializer, ITextSerializer<byte[]> byteSerializer)
    {
        _mutationSerializer = mutationSerializer;
        _byteSerializer = byteSerializer;
    }

    public string Serialize(MutationEnvelope<TMutation> entry)
    {
        var guidText = entry.Id.ToString();
        var mutationBytes = _mutationSerializer.Serialize(entry.Mutation);
        var mutationText = _byteSerializer.Serialize(mutationBytes);
        
        return $"{guidText}, {mutationText}";
    }

    public MutationEnvelope<TMutation> Deserialize(string text)
    {
        var splitted = text.Split(",");
        var guidText = splitted[0];
        var mutationText = splitted[1];
        var mutationBytes = _byteSerializer.Deserialize(mutationText);
        
        var mutation = _mutationSerializer.Deserialize(mutationBytes);
        
        return new MutationEnvelope<TMutation>()
        {
            Id = Guid.Parse(guidText),
            Mutation = mutation,
        };
    }
}