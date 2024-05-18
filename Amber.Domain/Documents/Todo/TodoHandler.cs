using Moonstone;

namespace Amber.Domain.Documents.Todo;

public class TodoHandler : IHandler
{
    public int DocumentTypeId { get; } = 0;
    public Type DocumentType { get; } = typeof(TodoAggregate);

    public Dictionary<int, Type> MutationTypes { get; } = new()
    {
        {0, typeof(ChangeName)},
        {1, typeof(ChangeEstimatedEffort)},
        {2, typeof(ChangeCompletion)},
    };
    
    public object CreateNew()
    {
        return new TodoAggregate(string.Empty);
    }

    public void ApplyMutation(object aggregate, object mutation)
    {
        var todo = (TodoAggregate)aggregate;
        
        switch (mutation)
        {
            case ChangeName m:
                todo.Name = m.Name;
                break;
            case ChangeEstimatedEffort m:
                todo.EstimatedEffort = m.EstimatedEffort;
                break;
            case ChangeCompletion m:
                todo.Completed = m.Completed;
                break;
        }
    }
}


public record ChangeCompletion(bool Completed);
public record ChangeEstimatedEffort(TimeSpan EstimatedEffort);
public record ChangeName(string Name);
