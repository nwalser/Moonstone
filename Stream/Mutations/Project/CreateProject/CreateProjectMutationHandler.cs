namespace Stream.Mutations.Project.CreateProject;

public class CreateProjectMutationHandler : IMutationHandler<CreateProjectMutation>
{
    public void Handle(CreateProjectMutation mutation, Projection projection)
    {
        if (projection.Projects.Any(p => p.Id == mutation.ProjectId))
            throw new Exception("A project with this id already exists");
        
        if (projection.Projects.Any(p => p.Name == mutation.Name))
            throw new Exception("A project with this name already exists");
        
        projection.Projects.Add(new Stream.Project()
        {
            Id = mutation.ProjectId,
            Name = mutation.Name,
        });
    }
}