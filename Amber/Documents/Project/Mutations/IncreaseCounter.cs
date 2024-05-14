namespace Amber.Documents.Project.Mutations;

public record IncreaseCounter : IProjectMutation
{
    public required int Count { get; init; }
}