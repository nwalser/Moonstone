using Amber.Domain.Documents.Project.Mutations;
using Moonstone;

namespace Amber.Domain.Documents.Project;

public class ProjectHandler : Handler<Project>
{
    public override int DocumentTypeId => 0;
    public override Type DocumentType => typeof(Project);

    public override Dictionary<int, Type> MutationTypes { get; } = new()
    {
        { 0, typeof(ChangeProjectName) },
        { 1, typeof(IncreaseCounter) },
    };
    
    public override Project CreateNew()
    {
        return new Project()
        {
            Name = "Your Project Name",
        };
    }

    public override void ApplyMutation(Project project, object mutation)
    {

    }
}