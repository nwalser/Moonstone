using System.Data.Common;
using Moonstone.Database;
using Sapphire.Data.Entities.MaximalAllocation;
using Sapphire.Data.Extensions;

namespace Sapphire.Data.Entities;

public class ProjectAggregate : Document
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;

    public int Priority { get; set; } = 0;
    public DateOnly? Start { get; set; }
    public DateOnly? Deadline { get; set; }

    public List<string> PossibleTags { get; set; } = [];


    public IEnumerable<TodoAggregate> GetRootTodos(ProjectDatabase db)
    {
        return db.Enumerate<TodoAggregate>()
            .Where(t => t.ProjectId == Id)
            .Where(t => t.ParentId is null);
    }
    
    public IEnumerable<TodoAggregate> GetTodos(ProjectDatabase db)
    {
        return db.Enumerate<TodoAggregate>()
            .Where(p => p.ProjectId == Id);
    }

    public int GetNumberOfTodos(ProjectDatabase db)
    {
        return GetTodos(db).Count();
    }
    
    public void Delete(ProjectDatabase db)
    {
        // todo: delete all related entities
        db.Remove(this);
    }


    public TimeSpan GetAllocations(ProjectDatabase db, DateOnly day, Guid workerId)
    {
        var todoIds = GetTodos(db).Select(t => t.Id).ToList();

        var allocations = db.Enumerate<AllocationAggregate>()
            .Where(a => a.WorkerId == workerId)
            .Where(a => a.Date == day)
            .Where(a => todoIds.Contains(a.TodoId))
            .Select(a => a.AllocatedTime);

        return TimeSpanExtensions.Sum(allocations);
    }
    
    public TimeSpan GetPlannedAllocations(ProjectDatabase db, DateOnly day, Guid workerId)
    {
        var todoIds = GetTodos(db).Select(t => t.Id).ToList();

        var plannedAllocations = db.PlannedAllocations
            .Where(p => p.WorkerId == workerId)
            .Where(p => p.Date == day)
            .Where(p => todoIds.Contains(p.TodoId))
            .Select(p => p.PlannedTime);

        return TimeSpanExtensions.Sum(plannedAllocations);
    }

    public TimeSpan GetMaximalAllocatable(ProjectDatabase db, DateOnly day, Guid workerId)
    {
        if (Start > day)
            return TimeSpan.Zero;
        
        var weeklyAllocations = GetWeeklyAllocations(db, workerId)
            .Where(w => w.ActiveFrom <= day)
            .Where(w => w.ActiveTo >= day)
            .Where(w => w.DayOfWeek == day.DayOfWeek)
            .Select(w => w.MaximalAllocation);
        
        var dailyAllocations = GetDailyAllocations(db, workerId)
            .Where(w => w.ActiveFrom <= day)
            .Where(w => w.ActiveTo >= day)
            .Select(w => w.MaximalAllocation);

        List<TimeSpan> allocationSpans = [..weeklyAllocations, ..dailyAllocations];

        if (allocationSpans.Count == 0)
            return TimeSpan.Zero;
        
        return TimeSpanExtensions.Min(allocationSpans);
    }
    
    public TimeSpan GetRemainingAllocatable(ProjectDatabase db, DateOnly day, Guid workerId)
    {
        var alreadyAllocated = GetPlannedAllocations(db, day, workerId) + GetAllocations(db, day, workerId);
        var maximalAllocatable = GetMaximalAllocatable(db, day, workerId);
        
        return maximalAllocatable - alreadyAllocated;
    }

    public IEnumerable<WeeklyAllocationRule> GetWeeklyAllocations(ProjectDatabase db, Guid workerId)
    {
        return db.Enumerate<WeeklyAllocationRule>()
            .Where(w => w.ProjectId == Id)
            .Where(w => w.WorkerId == workerId);
    }
    
    public IEnumerable<DailyAllocationRule> GetDailyAllocations(ProjectDatabase db, Guid workerId)
    {
        return db.Enumerate<DailyAllocationRule>()
            .Where(w => w.ProjectId == Id)
            .Where(w => w.WorkerId == workerId);
    }
    
    public IEnumerable<WorkerInProject> GetWorkersInProject(ProjectDatabase db)
    {
        return db.Enumerate<WorkerInProject>()
            .Where(w => w.ProjectId == Id);
    }
}