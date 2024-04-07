namespace Stream.Mutations.Project.CreateProject;

public class CreateProjectMutation : Mutation
{
    public required Guid ProjectId { get; set; }
    public required string Name { get; set; }
}