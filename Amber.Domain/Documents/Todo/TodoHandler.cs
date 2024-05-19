using Moonstone;

namespace Amber.Domain.Documents.Todo;

public class TodoHandler : IHandler<TodoAggregate>
{
    public Dictionary<int, Type> MutationTypes { get; } = new()
    {
        {0, typeof(ChangeName)},
        {1, typeof(ChangeEstimatedEffort)},
        {2, typeof(ChangeCompletion)},
    };

    public TodoAggregate CreateNew()
    {
        return new TodoAggregate(string.Empty);
    }

    public void ApplyMutation(TodoAggregate aggregate, object mutation)
    {
        switch (mutation)
        {
            case ChangeName m:
                aggregate.Name = m.Name;
                break;
            case ChangeEstimatedEffort m:
                aggregate.EstimatedEffort = m.EstimatedEffort;
                break;
            case ChangeCompletion m:
                aggregate.Completed = m.Completed;
                break;
        }
    }
}


public record ChangeCompletion(bool Completed);
public record ChangeEstimatedEffort(TimeSpan EstimatedEffort);
public record ChangeName(string Name);
