using Amber.Domain.Documents.Todo;
using Moonstone;

namespace Amber.Domain.Documents.Project;

public class ProjectAggregate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public List<DocumentLink<TodoAggregate>> Todos { get; init; } = [];
    
    
    public async Task<TodoAggregate> AddTodo(IWorkspace workspace)
    {
        var id = Guid.NewGuid();
        await workspace.Create<TodoAggregate>(id);
        Todos.Add(new DocumentLink<TodoAggregate>(id));
        return workspace.Load<TodoAggregate>(id);
    }
}