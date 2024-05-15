using Amber.Documents.Project.Mutations;

namespace Amber.Documents.Project;

public class ProjectHandler : IHandler
{
    public int DocumentTypeId => 0;

    public Dictionary<int, Type> MutationTypes { get; } = new()
    {
        { 0, typeof(ChangeProjectName) },
        { 1, typeof(IncreaseCounter) },
    };
    
    public object CreateNew()
    {
        return new Project()
        {
            Name = "Your Project Name",
            Counter = 0,
        };
    }

    public void ApplyMutation(object entity, object mutation)
    {
        var project = (Project)entity;
        
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