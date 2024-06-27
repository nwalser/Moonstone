using Moonstone.Database;
using Sapphire.Data.Entities.SchedulingLocks;
using Sapphire.Data.Extensions;

namespace Sapphire.Data.Entities.Todo;

public partial class TodoAggregate : Document
{
    // todo: maybe switch to IQueryable for optimization
    public required string Name { get; set; }
    
    public required Guid ProjectId { get; init; }
    
    public Guid? ParentId { get; set; } = Guid.Empty;
    public int Order { get; set; } = 0;
    
    public TodoState State { get; set; } = TodoState.Active; // todo: implement state handling in simulation

    public TimeSpan CurrentEstimatedEffort { get; set; } = TimeSpan.Zero;
    public TimeSpan? InitialGroupEstimatedEffort { get; set; }
    
    
    public bool Splittable { get; set; } = false;


    public bool FilterMatches(ProjectDatabase db, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return true;

        if (Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            return true;

        if (GetTags(db).Any(t => t.Contains(filter, StringComparison.InvariantCultureIgnoreCase)))
            return true;

        if (GetPossibleWorkers(db).Any(w => w.FilterMatches(db, filter)))
            return true;

        if (State.ToString().Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            return true;

        return false;
    }

    public void Delete(ProjectDatabase db)
    {
        db.Remove(this);

        foreach (var allocation in GetAllocations(db))
            allocation.Delete(db);
        
        foreach (var delayLocks in GetDelayLocks(db))
            delayLocks.Delete(db);
        
        foreach (var minDateLocks in GetMinDateLocks(db))
            minDateLocks.Delete(db);
        
        foreach (var childTodo in GetChildTodos(db))
            childTodo.Delete(db);
    }

    public TodoWorkerMetadata GetMetadata(ProjectDatabase db, WorkerAggregate? worker)
    {
        var metadata = db.Enumerate<TodoWorkerMetadata>()
            .Where(m => m.WorkerId == worker?.Id)
            .FirstOrDefault(m => m.TodoId == Id);

        metadata ??= new TodoWorkerMetadata()
        {
            TodoId = Id,
            WorkerId = worker?.Id,
        };
        
        return metadata;
    }
    
    
    public IEnumerable<AllocationAggregate> GetAllocations(ProjectDatabase db)
    {
        return db.Enumerate<AllocationAggregate>()
            .Where(a => a.TodoId == Id);
    }
    
    public IEnumerable<WorkerAggregate> GetAssignedWorkers(ProjectDatabase db)
    {
        var workerAllocationIds = db.Enumerate<AllocationAggregate>()
            .Where(w => w.TodoId == Id)
            .Select(w => w.WorkerId);
        
        var workerPlannedAllocationIds = db.PlannedAllocations
            .Where(p => p.TodoId == Id)
            .Select(w => w.WorkerId);

        var workerIds = workerAllocationIds
            .Concat(workerPlannedAllocationIds)
            .Distinct();

        return db.Enumerate<WorkerAggregate>()
            .Where(w => workerIds.Contains(w.Id));
    }
    
    public IEnumerable<TodoAggregate> GetAncestorTodos(ProjectDatabase db)
    {
        var ancestor = GetParentTodo(db);
        
        if (ancestor is null)
            return [];

        return [ancestor, ..ancestor.GetAncestorTodos(db)];
    }
    
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


    private IEnumerable<ILock> GetActiveParentLocks(ProjectDatabase db, DateOnly date)
    {
        var parentTodo = GetParentTodo(db);

        if (parentTodo is null)
            yield break;
            
        var parentLocks = parentTodo.GetActiveLocks(db, date);

        foreach (var parentLock in parentLocks)
            yield return parentLock;
    }

    private IEnumerable<MinDateLockAggregate> GetMinDateLocks(ProjectDatabase db)
    {
        return db.Enumerate<MinDateLockAggregate>()
            .Where(l => l.TodoId == Id);
    }
    
    private IEnumerable<MinDateLockAggregate> GetActiveMinDateLocks(ProjectDatabase db, DateOnly date)
    {
        var activeMinDateLocks = GetMinDateLocks(db)
            .Where(m => m.MinDate > date);

        foreach (var minDateLock in activeMinDateLocks)
            yield return minDateLock;
    }

    private IEnumerable<DelayLockAggregate> GetDelayLocks(ProjectDatabase db)
    {
        return db.Enumerate<DelayLockAggregate>()
            .Where(l => l.TodoId == Id);
    }
    
    private IEnumerable<DelayLockAggregate> GetActiveDelayLocks(ProjectDatabase db, DateOnly date)
    {
        foreach (var delayLock in GetDelayLocks(db))
        {
            var dependeeTodo = db.Enumerate<TodoAggregate>()
                .Single(t => t.Id == delayLock.TodoLockerId);

            var estimatedCompletion = dependeeTodo.GetEstimatedCompletion(db);

            if (estimatedCompletion is null)
                yield return delayLock;

            if (estimatedCompletion > date.AddDays(delayLock.DelayInDays))
                yield return delayLock;
        }
    }

    public DateOnly? GetEstimatedCompletion(ProjectDatabase db)
    {
        if (CurrentEstimatedEffort <= TimeSpan.Zero)
            return DateOnly.MinValue;
        
        if (GetRemainingUnplannedEffort(db) > TimeSpan.Zero)
            return null;

        var plannedAllocations = db.PlannedAllocations.Select(a => a.Date);
        var allocations = db.Enumerate<AllocationAggregate>().Select(a => a.Date);

        return plannedAllocations.Concat(allocations).MaxBy(a => (DateOnly?)a);
    }
    
    public IEnumerable<ILock> GetActiveLocks(ProjectDatabase db, DateOnly date)
    {
        foreach (var @lock in GetActiveParentLocks(db, date))
            yield return @lock;
        
        foreach (var @lock in GetActiveMinDateLocks(db, date))
            yield return @lock;

        foreach (var @lock in GetActiveDelayLocks(db, date))
            yield return @lock;
    }
    
    
    public TodoAggregate? GetParentTodo(ProjectDatabase db)
    {
        return db.Enumerate<TodoAggregate>()
            .SingleOrDefault(t => t.Id == ParentId);
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