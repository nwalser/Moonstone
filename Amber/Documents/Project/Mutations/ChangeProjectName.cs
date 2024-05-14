namespace Amber.Documents.Project.Mutations;

public record ChangeProjectName : IProjectMutation
{
    public required string Name { get; init; }
}