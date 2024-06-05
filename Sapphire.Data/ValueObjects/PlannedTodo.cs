namespace Sapphire.Data.ValueObjects;

public class PlannedTodo
{
    public Guid TodoId { get; init; }
    
    public DateOnly? PlannedStart { get; init; }
    public DateOnly? PlannedEnd { get; init; }
}