namespace Sapphire.Data.ValueObjects;

public class PlannedAllocation
{
    public required Guid TodoId { get; init; }
    public required Guid WorkerId { get; init; }
    
    public required DateOnly Date { get; init; }
    public required TimeSpan PlannedTime { get; init; }
}