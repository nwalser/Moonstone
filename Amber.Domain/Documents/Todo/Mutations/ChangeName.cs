namespace Amber.Domain.Documents.Todo.Mutations;

public record ChangeName
{
    public required string Name { get; init; }
}