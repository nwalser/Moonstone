using Moonstone.Database;

namespace Sapphire.Data.Entities.WorkingHours;

public class OfficeDayAggregate : Document
{
    public Guid WorkerId { get; set; }
    public DateOnly Date { get; set; }
    public TimeSpan WorkingHours { get; set; }
}