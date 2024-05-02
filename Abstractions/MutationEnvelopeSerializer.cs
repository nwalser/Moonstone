namespace Abstractions;

public class MutationEnvelopeSerializer<TMutation> : ITextSerializer<MutationEnvelope<TMutation>>
{
    private readonly ITextSerializer<TMutation> _mutationSerializer;

    public MutationEnvelopeSerializer(ITextSerializer<TMutation> mutationSerializer)
    {
        _mutationSerializer = mutationSerializer;
    }

    public string Serialize(MutationEnvelope<TMutation> entry)
    {
        var guidText = entry.Id.ToString();
        var mutationText = _mutationSerializer.Serialize(entry.Mutation);

        return $"{guidText}, {mutationText}";
    }

    public MutationEnvelope<TMutation> Deserialize(string text)
    {
        var splitted = text.Split(",");
        var guidText = splitted[0];
        var mutationText = splitted[1];
        
        var mutation = _mutationSerializer.Deserialize(mutationText);

        return new MutationEnvelope<TMutation>()
        {
            Id = Guid.Parse(guidText),
            Mutation = mutation,
        };
    }
}