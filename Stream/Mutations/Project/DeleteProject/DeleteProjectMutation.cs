namespace Stream.Mutations.Project.DeleteProject;

public class DeleteProjectMutation : Mutation
{
    public required Guid Id { get; set; }
}