using MsgPack.Serialization;
using Newtonsoft.Json;
using Stream.Mutations;

namespace Stream.FileStore;

public class MutationSerializer
{
    private static readonly JsonSerializerSettings SerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };

    private static readonly MessagePackSerializer<Mutation> Serializer = MessagePackSerializer.Get<Mutation>();
    
    public static byte[] Serialize(Mutation mutation)
    {
        return Serializer.PackSingleObject(mutation);
    }
    
    public static Mutation Deserialize(byte[] bytes)
    {
        return Serializer.UnpackSingleObject(bytes);
    }
    
    public static List<Mutation> DeserializeList(string json)
    {
        return JsonConvert.DeserializeObject<List<Mutation>>(json, SerializerSettings)!;
    }
}