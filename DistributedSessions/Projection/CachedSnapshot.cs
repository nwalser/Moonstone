using System.ComponentModel.DataAnnotations;

namespace DistributedSessions.Projection;

public class CachedSnapshot
{
    [Key]
    public Guid Id { get; set; }
    
    public required TimeSpan TargetAge { get; set; }
    
    public required Guid LastMutationId { get; set; }
    public required DateTime LastMutationOccurence { get; set; }
}