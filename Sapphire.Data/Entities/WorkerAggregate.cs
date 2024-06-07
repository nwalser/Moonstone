﻿using Moonstone.Database;
using Sapphire.Data.Entities.WorkingHours;
using Sapphire.Data.Extensions;


namespace Sapphire.Data.Entities;

public class WorkerAggregate : Document
{
    public required string Name { get; set; }
    public TimeSpan ConstantBaseLoad { get; set; } = TimeSpan.Zero;

    
    public TimeSpan RegularHours(ProjectDatabase db, DateOnly date)
    {
        // OfficeDay
        {
            var officeDays = db.Enumerate<OfficeDayAggregate>()
                .Where(o => o.Date == date)
                .Where(o => o.WorkerId == Id)
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
            var weeklyWorkDays = db.Enumerate<WeeklyWorkDay>()
                .Where(w => w.WorkerId == Id)
                .Where(w => w.DayOfWeek == date.DayOfWeek)
                .Where(w => w.ActiveFrom <= date)
                .Where(w => w.ActiveTo >= date)
                .ToList();
            
            if(weeklyWorkDays.Any())
                return TimeSpanExtensions.Sum(weeklyWorkDays.Select(t => t.WorkingHours));
        }

        return TimeSpan.Zero;
    }    

    public TimeSpan WorkedHours(ProjectDatabase db, DateOnly date)
    {
        var allocations = db.Enumerate<AllocationAggregate>()
            .Where(a => a.Date == date)
            .Where(a => a.WorkerId == Id)
            .ToList();
        
        return TimeSpanExtensions.Sum(allocations.Select(t => t.AllocatedTime));
    }    
    
    public TimeSpan PlannedHours(ProjectDatabase db, DateOnly date)
    {
        var plannedAllocations = db.PlannedAllocations
            .Where(a => a.Date == date)
            .Where(a => a.WorkerId == Id)
            .ToList();
        
        return TimeSpanExtensions.Sum(plannedAllocations.Select(t => t.PlannedTime));
    }

    public TimeSpan BaseLoad(ProjectDatabase db, DateOnly date)
    {
        return ConstantBaseLoad;
    }
    
    
    public TimeSpan AvailableHours(ProjectDatabase db, DateOnly date)
    {
        return RegularHours(db, date) - WorkedHours(db, date) - PlannedHours(db, date) - BaseLoad(db, date);
    }
}