using System.ComponentModel.DataAnnotations;

namespace Stream.FileStore;

public class CachedMutation
{
    [Key]
    public required Guid MutationId { get; set; }
    public required DateTime Occurence { get; set; }
    public required string MutationJson { get; set; }
}