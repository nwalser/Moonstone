using Moonstone.Database;
using Sapphire.Data.Entities.SchedulingLocks;
using Sapphire.Data.ValueObjects;

namespace Sapphire.Data.Entities;

public class TodoAggregate : Document
{
    public required string Name { get; set; }
    
    public required Guid ProjectId { get; init; }
    
    public Guid? ParentId { get; set; } = Guid.Empty;
    public uint Order { get; set; } = 0;
    
    public TodoState State { get; set; } = TodoState.Active;

    public TimeSpan InitialEstimatedEffort { get; set; } = TimeSpan.Zero;
    public TimeSpan CurrentEstimatedEffort { get; set; } = TimeSpan.Zero;

    public Guid[] PossibleWorkerIds { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    
    public bool Splittable { get; set; } = false;


    private IEnumerable<TodoAggregate> GetChildTodos(ProjectDatabase db, TodoAggregate? todo = default)
    {
        return db.Enumerate<TodoAggregate>()
            .Where(t => t.ParentId == (todo ?? this).Id);
    }

    public IEnumerable<TodoAggregate> GetAllChildTodos(ProjectDatabase db, TodoAggregate? todo = default)
    {
        var children = GetChildTodos(db, todo ?? this);
        
        foreach (var child in children)
        {
            yield return child;
            
            foreach (var grandChild in GetAllChildTodos(db, child))
                yield return grandChild;
        }
    }
    
    
    public IEnumerable<ILock> GetActiveLocks(ProjectDatabase db, DateOnly date)
    {
        // ParentLock
        {
            var parentTodo = GetParentTodo(db);

            if (parentTodo is null)
                yield break;
        
            if (parentTodo.State == TodoState.Completed)
                yield break;

            var plannedTodo = parentTodo.GetPlannedTodo(db);
        
            if(plannedTodo is not null && plannedTodo.PlannedEnd <= date)
                yield break;

            yield return new ParentLock(Id, parentTodo.Id);
        }
        
        // MinDateLocks
        {
            var activeMinDateLocks = db.Enumerate<MinDateLockAggregate>()
                .Where(l => l.TodoId == Id)
                .Where(m => m.MinDate > date);

            foreach (var minDateLock in activeMinDateLocks)
                yield return minDateLock;
        }

        // DelayLock
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

                if (plannedTodo is not null && plannedTodo.PlannedEnd <= date)
                    continue;

                yield return delayLock;
            }
        }
    }

    public TodoAggregate? GetParentTodo(ProjectDatabase db)
    {
        return db.Enumerate<TodoAggregate>()
            .SingleOrDefault(t => t.ParentId == ParentId);
    }
    
    public PlannedTodo? GetPlannedTodo(ProjectDatabase db)
    {
        return db.PlannedTodos.SingleOrDefault(t => t.TodoId == Id);
    } 
}

public enum TodoState
{
    Draft,
    Active,
    Completed
}