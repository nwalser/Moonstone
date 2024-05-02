using System.Text;
using Abstractions;
using Newtonsoft.Json;

namespace Implementation.Serializer;

public class ProjectionBinarySerializer : IBinarySerializer<Projection>
{
    public byte[] Serialize(Projection entry)
    {
        var json = JsonConvert.SerializeObject(entry);
        return Encoding.UTF8.GetBytes(json);
    }

    public Projection Deserialize(byte[] bytes)
    {
        var json = Encoding.UTF8.GetString(bytes);
        return JsonConvert.DeserializeObject<Projection>(json) ?? throw new InvalidOperationException();
    }
}