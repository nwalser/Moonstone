namespace Amber.Documents;

public record ChangeProjectName : IProjectMutation
{
    public required string Name { get; init; }
}