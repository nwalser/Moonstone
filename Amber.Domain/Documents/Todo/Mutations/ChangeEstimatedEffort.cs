namespace Amber.Domain.Documents.Todo.Mutations;

public record ChangeEstimatedEffort
{
    public required TimeSpan EstimatedEffort { get; init; }
}