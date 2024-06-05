namespace Sapphire.Data.Entities.WorkingHours;

public class LeaveDayAggregate
{
    public Guid WorkerId { get; set; }
    public DateOnly Date { get; set; }
}