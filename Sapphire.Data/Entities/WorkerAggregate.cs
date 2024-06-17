using Moonstone.Database;
using Sapphire.Data.Entities.WorkingHours;
using Sapphire.Data.Extensions;

namespace Sapphire.Data.Entities;

public class WorkerAggregate : Document
{
    public required string Name { get; set; }
    public string Abbreviation { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public TimeSpan ConstantBaseLoad { get; set; } = TimeSpan.Zero;


    public string GetDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(Abbreviation))
            return Abbreviation;

        return Name;
    }
    
    public bool FilterMatches(ProjectDatabase db, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return true;

        if (Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            return true;

        if (Abbreviation.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            return true;

        return false;
    }
    
    public void Delete(ProjectDatabase db)
    {
        db.Remove(this);

        foreach (var officeDay in GetOfficeDays(db))
            officeDay.Delete(db);
        
        foreach (var leaveDay in GetLeaveDays(db))
            leaveDay.Delete(db);
        
        foreach (var workWeek in GetWorkWeeks(db))
            workWeek.Delete(db);
    }

    private IEnumerable<OfficeDayAggregate> GetOfficeDays(ProjectDatabase db)
    {
        return db.Enumerate<OfficeDayAggregate>()
            .Where(o => o.WorkerId == Id);
    }
    
    private IEnumerable<LeaveDayAggregate> GetLeaveDays(ProjectDatabase db)
    {
        return db.Enumerate<LeaveDayAggregate>()
            .Where(o => o.WorkerId == Id);
    }
    
    private IEnumerable<WorkWeek> GetWorkWeeks(ProjectDatabase db)
    {
        return db.Enumerate<WorkWeek>()
            .Where(o => o.WorkerId == Id);
    }
    
    public TimeSpan GetRegularHours(ProjectDatabase db, DateOnly date)
    {
        // OfficeDay
        {
            var officeDays = GetOfficeDays(db)
                .Where(o => o.Date == date)
                .ToList();

            if (officeDays.Any())
                return TimeSpanExtensions.Sum(officeDays.Select(t => t.WorkingHours));
        }
        
        // LeaveDay
        {
            var leaveDay = db.Enumerate<LeaveDayAggregate>()
                .Where(l => l.Date == date)
                .FirstOrDefault(o => o.WorkerId == Id);

            if (leaveDay is not null)
                return TimeSpan.Zero;
        }
        
        // WeeklyWorkDay
        {
            var weeklyWorkDays = db.Enumerate<WorkWeek>()
                .Where(w => w.WorkerId == Id)
                .Where(w => w.ActiveFrom is null || w.ActiveFrom <= date)
                .Where(w => w.ActiveTo is null || w.ActiveTo >= date)
                .ToList();

            if (weeklyWorkDays.Any())
                return TimeSpanExtensions.Sum(weeklyWorkDays.Select(t => t.GetWorkingHours(date.DayOfWeek)));
        }

        return TimeSpan.Zero;
    }    

    public TimeSpan GetWorkedHours(ProjectDatabase db, DateOnly date)
    {
        var allocations = db.Enumerate<AllocationAggregate>()
            .Where(a => a.Date == date)
            .Where(a => a.WorkerId == Id)
            .ToList();
        
        return TimeSpanExtensions.Sum(allocations.Select(t => t.AllocatedTime));
    }    
    
    public TimeSpan GetPlannedHours(ProjectDatabase db, DateOnly date)
    {
        var plannedAllocations = db.PlannedAllocations
            .Where(a => a.Date == date)
            .Where(a => a.WorkerId == Id)
            .ToList();
        
        return TimeSpanExtensions.Sum(plannedAllocations.Select(t => t.PlannedTime));
    }

    public TimeSpan GetBaseLoad(ProjectDatabase db, DateOnly date)
    {
        return ConstantBaseLoad;
    }
    
    
    public TimeSpan GetAvailableHours(ProjectDatabase db, DateOnly date)
    {
        return GetRegularHours(db, date) - GetWorkedHours(db, date) - GetPlannedHours(db, date) - GetBaseLoad(db, date);
    }
}