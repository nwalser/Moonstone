using System.Text.Json.Serialization;

namespace Moonstone.Database;

public class Document
{
    [JsonInclude]
    public Guid Id { get; internal set; }
    
    [JsonInclude]
    public DateTime LastWrite { get; internal set; } = DateTime.UtcNow;
    
    [JsonConstructor]
    public Document(Guid id)
    {
        Id = id;
    }

    public Document()
    {
        Id = Guid.NewGuid();
    }
}