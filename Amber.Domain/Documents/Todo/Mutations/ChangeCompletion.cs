namespace Amber.Domain.Documents.Todo.Mutations;

public record ChangeCompletion
{
    public required bool Completed { get; init; }
}