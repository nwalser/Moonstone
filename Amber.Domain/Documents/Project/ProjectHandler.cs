using Moonstone;

namespace Amber.Domain.Documents.Project;

public class ProjectHandler : IHandler<ProjectAggregate>
{
    public Dictionary<int, Type> MutationTypes { get; } = new()
    {
        { 0, typeof(ChangeName) },
        { 1, typeof(AddTodo) },
        { 2, typeof(RemoveTodo) }
    };
    
    public ProjectAggregate CreateNew()
    {
        return new ProjectAggregate();
    }

    public void ApplyMutation(ProjectAggregate aggregate, object mutation)
    {
        switch (mutation)
        {
            case ChangeName changeName:
                aggregate.Name = changeName.Name;
                break;
            case AddTodo addTodo:
                aggregate.Todos.Add(addTodo.TodoId);
                break;
            case RemoveTodo removeTodo:
                aggregate.Todos.Remove(removeTodo.TodoId);
                break;
        }
    }

    public static Reader<ProjectAggregate> GetReader(string session)
    {
        return new Reader<ProjectAggregate>(session, new ProjectHandler());
    }
}

public record ChangeName(string Name);
public record AddTodo(Guid TodoId);
public record RemoveTodo(Guid TodoId);