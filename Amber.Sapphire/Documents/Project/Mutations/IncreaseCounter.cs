namespace Amber.Sapphire.Documents.Project.Mutations;

public record IncreaseCounter
{
    public required int Count { get; init; }
}