using Abstractions;
using Newtonsoft.Json;

namespace Implementation.Serializer;

public class ProjectionTextSerializer : ITextSerializer<Projection>
{
    public string Serialize(Projection entry)
    {
        return JsonConvert.SerializeObject(entry);
    }

    public Projection Deserialize(string text)
    {
        return JsonConvert.DeserializeObject<Projection>(text) ?? throw new InvalidOperationException();
    }
}