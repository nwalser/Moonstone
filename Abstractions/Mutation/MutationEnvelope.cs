namespace Abstractions.Mutation;

public record MutationEnvelope<TMutation>
{
    public required Guid Id { get; init; }
    public required TMutation Mutation { get; init; }
}