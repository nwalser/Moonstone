using Moonstone.Database;

namespace Sapphire.Data.Entities.WorkingHours;

public class LeaveDayAggregate : Document
{
    public Guid WorkerId { get; set; }
    public DateOnly Date { get; set; }
}