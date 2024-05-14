namespace Amber.Doc;

public record MutationDefinition<TDocument>
{
    public required int Id { get; init; }
    public required Type Type { get; init; }
    public required Action<TDocument, object> Apply { get; init; }
}