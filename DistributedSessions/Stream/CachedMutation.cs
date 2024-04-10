using System.ComponentModel.DataAnnotations;
using System.Text;
using DistributedSessions.Mutations;
using Newtonsoft.Json;

namespace DistributedSessions.Stream;

public class CachedMutation
{
    [Key]
    public required Guid MutationId { get; set; }
    public required DateTime Occurence { get; set; }
    public required byte[] Mutation { get; set; }

    
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
            Occurence = mutation.Occurence,
            Mutation = bytes,
        };
    }
    
    public static Mutation ToMutation(CachedMutation cached)
    {
        var json = Encoding.UTF8.GetString(cached.Mutation);
        return JsonConvert.DeserializeObject<Mutation>(json, Settings)!;
    }
}