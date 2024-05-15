namespace Amber;

public class MutationEnvelope
{
    public required DateTime Occurence { get; init; }
    public required object Mutation { get; init; }
}