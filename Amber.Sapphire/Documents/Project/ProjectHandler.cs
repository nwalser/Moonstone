using Amber.Sapphire.Documents.Project.Mutations;

namespace Amber.Sapphire.Documents.Project;

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
            Counter = 0,
        };
    }

    public override void ApplyMutation(Project project, object mutation)
    {
        switch (mutation)
        {
            case ChangeProjectName changeProjectName: 
                project.ChangeProjectName(changeProjectName.Name);
                break;
            case IncreaseCounter increaseCounter:
                project.IncreaseCounter(increaseCounter.Count);
                break;
        }
    }
}