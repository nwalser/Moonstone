namespace Amber.Documents;

public record IncreaseCounter : IProjectMutation
{
    public required int Count { get; init; }
}