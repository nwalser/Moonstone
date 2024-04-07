namespace Stream.Mutations.Project.ChangeProjectName;

public class ChangeProjectNameMutationHandler : IMutationHandler<ChangeProjectNameMutation>
{
    public void Handle(ChangeProjectNameMutation mutation, Projection projection)
    {
        var project = projection.Projects.FirstOrDefault(p => p.Id == mutation.Id);

        if (project is null)
            throw new Exception("Project does not exist");
        
        project.ChangeName(mutation.Name);
    }
}