namespace Client1;

public class CachedMutation
{
    public required Guid MutationId { get; set; }
    public required DateTime Occurence { get; set; }
    public required string MutationJson { get; set; }
}