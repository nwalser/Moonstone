using Moonstone.Database;

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


    public List<TodoAggregate> GetChildren(ProjectDatabase database, TodoAggregate? todo = default)
    {
        return database.Enumerate<TodoAggregate>().Where(t => t.ParentId == (todo ?? this).Id).ToList();
    }

    public IEnumerable<TodoAggregate> GetAllChildren(ProjectDatabase database, TodoAggregate? todo = default)
    {
        var children = GetChildren(database, todo ?? this);
        
        foreach (var child in children)
        {
            yield return child;
            
            foreach (var grandChild in GetAllChildren(database, child))
                yield return grandChild;
        }
    }
}

public enum TodoState
{
    Draft,
    Active,
    Completed
}