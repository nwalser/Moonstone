using System.ComponentModel.DataAnnotations;
using System.Text;
using Newtonsoft.Json;

namespace DistributedSessions.Projection;

public class CachedSnapshot
{
    [Key]
    public Guid Id { get; set; }
    
    public required TimeSpan TargetAge { get; set; }
    
    public required Guid? LastMutationId { get; set; }
    public required DateTime? LastMutationOccurence { get; set; }
    
    public required byte[] Snapshot { get; set; }
    
    
    private static readonly JsonSerializerSettings Settings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
    };
    
    public static CachedSnapshot ToCached(Guid id, TimeSpan targetAge, Snapshot snapshot)
    {
        var json = JsonConvert.SerializeObject(snapshot, Settings);
        var bytes = Encoding.UTF8.GetBytes(json);
        
        return new CachedSnapshot()
        {
            Id = id,
            TargetAge = targetAge,
            Snapshot = bytes,
            LastMutationId = snapshot.LastMutation?.Id,
            LastMutationOccurence = snapshot.LastMutation?.Occurence
        };
    }
    
    public static Snapshot FromCached(CachedSnapshot cached)
    {
        var json = Encoding.UTF8.GetString(cached.Snapshot);
        return JsonConvert.DeserializeObject<Snapshot>(json, Settings)!;
    }
}