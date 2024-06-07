using Moonstone.Database;
using Sapphire.Data.Entities.SchedulingLocks;
using Sapphire.Data.Extensions;
using Sapphire.Data.ValueObjects;

namespace Sapphire.Data.Entities;

public class TodoAggregate : Document
{
    public required string Name { get; set; }
    
    public required Guid ProjectId { get; init; }
    
    public Guid? ParentId { get; set; } = Guid.Empty;
    public uint Order { get; set; } = 0;
    
    public TodoState State { get; set; } = TodoState.Active;

    public TimeSpan CurrentEstimatedEffort { get; set; } = TimeSpan.Zero;
    public TimeSpan? InitialGroupEstimatedEffort { get; set; } = TimeSpan.Zero;

    public Guid[] PossibleWorkerIds { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    
    public bool Splittable { get; set; } = false;


    public bool HasChildren(ProjectDatabase db)
    {
        return GetChildTodos(db).Any();
    }
    
    public IEnumerable<TodoAggregate> GetChildTodos(ProjectDatabase db, TodoAggregate? todo = default)
    {
        return db.Enumerate<TodoAggregate>()
            .Where(t => t.ParentId == (todo ?? this).Id);
    }

    public IEnumerable<TodoAggregate> GetDescendantTodos(ProjectDatabase db, TodoAggregate? todo = default)
    {
        var children = GetChildTodos(db, todo ?? this);
        
        foreach (var child in children)
        {
            yield return child;
            
            foreach (var grandChild in GetDescendantTodos(db, child))
                yield return grandChild;
        }
    }


    private IEnumerable<ILock> GetParentLocks(ProjectDatabase db, DateOnly date)
    {
        var parentTodo = GetParentTodo(db);

        if (parentTodo is null)
            yield break;
            
        var parentLocks = parentTodo.GetActiveLocks(db, date);

        foreach (var parentLock in parentLocks)
            yield return parentLock;
    }

    private IEnumerable<ILock> GetMinDateLocks(ProjectDatabase db, DateOnly date)
    {
        var activeMinDateLocks = db.Enumerate<MinDateLockAggregate>()
            .Where(l => l.TodoId == Id)
            .Where(m => m.MinDate > date);

        foreach (var minDateLock in activeMinDateLocks)
            yield return minDateLock;
    }
    
    private IEnumerable<ILock> GetDelayLocks(ProjectDatabase db, DateOnly date)
    {
        var delayLocks = db.Enumerate<DelayLockAggregate>()
            .Where(l => l.TodoId == Id);

        foreach (var delayLock in delayLocks)
        {
            var dependeeTodo = db.Enumerate<TodoAggregate>()
                .Single(t => t.Id == delayLock.TodoLockerId);

            if (dependeeTodo.State == TodoState.Completed)
                continue;

            var plannedTodo = dependeeTodo.GetPlannedTodo(db);

            if (plannedTodo is not null && plannedTodo.PlannedCompletion <= date)
                continue;

            yield return delayLock;
        }
    }
    
    public IEnumerable<ILock> GetActiveLocks(ProjectDatabase db, DateOnly date)
    {
        foreach (var @lock in GetParentLocks(db, date))
            yield return @lock;
        
        foreach (var @lock in GetMinDateLocks(db, date))
            yield return @lock;

        foreach (var @lock in GetDelayLocks(db, date))
            yield return @lock;
    }
    
    
    public TodoAggregate? GetParentTodo(ProjectDatabase db)
    {
        return db.Enumerate<TodoAggregate>()
            .SingleOrDefault(t => t.Id == ParentId);
    }
    
    public PlannedTodo? GetPlannedTodo(ProjectDatabase db)
    {
        return db.PlannedTodos.SingleOrDefault(t => t.TodoId == Id);
    }

    public ProjectAggregate GetProject(ProjectDatabase db)
    {
        return db.Enumerate<ProjectAggregate>()
            .Single(p => p.Id == ProjectId);
    }
    
    
    private TimeSpan GetChildrenEstimatedEffort(ProjectDatabase db)
    {
        var estimatedEfforts = GetChildTodos(db)
            .Select(c => c.GetGroupEstimatedEffort(db));

        return TimeSpanExtensions.Sum(estimatedEfforts);
    }

    private TimeSpan GetChildrenWorkedEffort(ProjectDatabase db)
    {
        var childrenWorkedEffort = GetChildTodos(db)
            .Select(c => c.GetGroupWorkedEffort(db));

        return TimeSpanExtensions.Sum(childrenWorkedEffort);
    }
    
    
    public TimeSpan GetEstimatedEffort(ProjectDatabase db)
    {
        return CurrentEstimatedEffort;
    }

    public TimeSpan GetWorkedEffort(ProjectDatabase db)
    {
        var workedEfforts = db.Enumerate<AllocationAggregate>()
            .Where(a => a.TodoId == Id)
            .Select(a => a.AllocatedTime);

        return TimeSpanExtensions.Sum(workedEfforts);
    }
    
    public TimeSpan GetPlannedEffort(ProjectDatabase db)
    {
        var planned = db.PlannedAllocations
            .Where(p => p.TodoId == Id)
            .Select(p => p.PlannedTime);
        
        return TimeSpanExtensions.Sum(planned);
    }

    public TimeSpan GetRemainingUnplannedEffort(ProjectDatabase db)
    {
        return GetEstimatedEffort(db) - GetWorkedEffort(db) - GetPlannedEffort(db);
    }
    
    
    public TimeSpan GetGroupEstimatedEffort(ProjectDatabase db)
    {
        return GetChildrenEstimatedEffort(db) + GetEstimatedEffort(db);
    }

    public TimeSpan GetGroupWorkedEffort(ProjectDatabase db)
    {
        return GetChildrenWorkedEffort(db) + GetWorkedEffort(db);
    }
}

public enum TodoState
{
    Draft,
    Active,
    Completed
}