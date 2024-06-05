namespace Sapphire.Data.Entities.WorkingHours;

public class WeeklyWorkDay
{
    public required Guid WorkerId { get; set; }
    
    public required DayOfWeek DayOfWeek { get; set; }
    public required TimeSpan WorkingHours { get; set; }

    public required DateOnly ActiveFrom { get; set; }
    public required DateOnly ActiveTo { get; set; }
}