using Amber.Domain.Documents.Todo.Mutations;
using Moonstone;

namespace Amber.Domain.Documents.Todo;

public class TodoHandler : IHandler
{
    public int DocumentTypeId { get; } = 0;
    public Type DocumentType { get; } = typeof(Todo);

    public Dictionary<int, Type> MutationTypes { get; } = new()
    {
        {0, typeof(ChangeName)},
        {1, typeof(ChangeEstimatedEffort)},
        {2, typeof(ChangeCompletion)},
    };
    
    public object CreateNew()
    {
        return new Todo(string.Empty);
    }

    public void ApplyMutation(object aggregate, object mutation)
    {
        var todo = (Todo)aggregate;
        
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