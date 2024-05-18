using Moonstone;

namespace Amber.Domain.Documents.Project;

public class ProjectHandler : Handler<ProjectAggregate>
{
    public override int DocumentTypeId { get; }
    public override Type DocumentType { get; }
    public override Dictionary<int, Type> MutationTypes { get; }
    public override ProjectAggregate CreateNew()
    {
        throw new NotImplementedException();
    }

    public override void ApplyMutation(ProjectAggregate project, object mutation)
    {
        throw new NotImplementedException();
    }
}

public record AddTodo(Guid TodoId);