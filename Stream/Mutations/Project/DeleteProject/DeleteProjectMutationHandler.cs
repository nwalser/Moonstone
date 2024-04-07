namespace Stream.Mutations.Project.DeleteProject;

public class DeleteProjectMutationHandler : IMutationHandler<DeleteProjectMutation>
{
    public void Handle(DeleteProjectMutation mutation, Projection projection)
    {
        var project = projection.Projects.FirstOrDefault(p => p.Id == mutation.Id);
        
        if (project is null)
            throw new Exception("This project does not exists to delete");
        
        projection.Projects.Remove(project);
    }
}