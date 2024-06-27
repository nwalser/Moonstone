using System.Text.Json.Serialization;

namespace Sapphire.Data.Entities.Todo;

public partial class TodoAggregate
{
    [JsonInclude] private List<string>? Tags { get; set; }

    [JsonIgnore]
    public bool InheritsTags
    {
        get => Tags is null;
        set => Tags = value ? null : [];
    }

    public void RemoveTag(string tag) => Tags?.Remove(tag);
    public void AddTag(string tag) => Tags?.Add(tag);

    public void SetTags(List<string> tags)
    {
        if(!InheritsTags)
            Tags = tags;
    }
    
    public IEnumerable<string> GetTags(ProjectDatabase db)
    {
        if (Tags is not null)
            return Tags;
        
        return GetParentTodo(db)?.GetTags(db) ?? [];
    }
}