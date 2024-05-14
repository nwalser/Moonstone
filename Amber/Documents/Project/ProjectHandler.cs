namespace Amber.Documents;

public class ProjectHandler
{
    public Dictionary<int, Type> MutationTypes { get; } = new()
    {
        { 0, typeof(ChangeProjectName) },
        { 1, typeof(IncreaseCounter) },
    };
    
    public Project CreateNew()
    {
        return new Project()
        {
            Name = "Your Project Name",
            Counter = 0,
        };
    }

    public void ApplyMutation(Project project, IProjectMutation mutation)
    {
        throw new NotImplementedException();
    }
}