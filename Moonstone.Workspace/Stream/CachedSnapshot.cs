using System.ComponentModel.DataAnnotations;
using System.Text;
using Newtonsoft.Json;

namespace Moonstone.Workspace.Stream;

public class CachedSnapshot
{
    [Key]
    public Guid Id { get; set; }
    
    public required TimeSpan TargetAge { get; set; }
    
    public required Guid LastMutationId { get; set; }
    
    public required byte[] Snapshot { get; set; }
    
    
    private static readonly JsonSerializerSettings Settings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
    };
    
    public static CachedSnapshot ToCached<TModel>(Guid id, TimeSpan targetAge, Snapshot<TModel> snapshot) where TModel : new()
    {
        var json = JsonConvert.SerializeObject(snapshot, Settings);
        var bytes = Encoding.UTF8.GetBytes(json);
        
        return new CachedSnapshot()
        {
            Id = id,
            TargetAge = targetAge,
            Snapshot = bytes,
            LastMutationId = snapshot.LastMutationId,
        };
    }
    
    public static Snapshot<TModel> CopyFromCached<TModel>(CachedSnapshot cached) where TModel : new()
    {
        var json = Encoding.UTF8.GetString(cached.Snapshot);
        return JsonConvert.DeserializeObject<Snapshot<TModel>>(json, Settings)!;
    }
}