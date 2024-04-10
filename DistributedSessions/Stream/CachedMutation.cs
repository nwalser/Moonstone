using System.ComponentModel.DataAnnotations;

namespace DistributedSessions.Stream;

public class CachedMutation
{
    [Key]
    public required Guid MutationId { get; set; }
    public required DateTime Occurence { get; set; }
    public required byte[] Mutation { get; set; }
}