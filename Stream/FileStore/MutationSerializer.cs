using Newtonsoft.Json;
using Stream.Mutations;

namespace Stream.FileStore;

public class MutationSerializer
{
    private static readonly JsonSerializerSettings SerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };

    public static string Serialize(Mutation mutation)
    {
        return JsonConvert.SerializeObject(mutation, SerializerSettings);
    }
    
    public static Mutation Deserialize(string json)
    {
        return JsonConvert.DeserializeObject<Mutation>(json, SerializerSettings)!;
    }
}