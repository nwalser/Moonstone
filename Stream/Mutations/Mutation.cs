namespace Stream.Mutations;

public abstract class Mutation
{
    public Guid MutationId { get; set; }
    public DateTime Occurence { get; set; }
}