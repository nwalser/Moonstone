using System.Text;
using Newtonsoft.Json;

namespace Moonstone.Framework.Stream;

public class CachedMutation
{
    public required Guid MutationId { get; init; }
    public required byte[] Mutation { get; init; }
    
    
    private static readonly JsonSerializerSettings Settings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
    };
    
    public static CachedMutation FromMutation(Mutation mutation)
    {
        var json = JsonConvert.SerializeObject(mutation, Settings);
        var bytes = Encoding.UTF8.GetBytes(json);

        return new CachedMutation()
        {
            MutationId = mutation.Id,
            Mutation = bytes,
        };
    }
    
    public static Mutation ToMutation(CachedMutation cached)
    {
        var json = Encoding.UTF8.GetString(cached.Mutation);
        return JsonConvert.DeserializeObject<Mutation>(json, Settings)!;
    }
}