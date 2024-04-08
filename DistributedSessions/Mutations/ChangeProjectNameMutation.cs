namespace DistributedSessions.Mutations;

public class ChangeProjectNameMutation : Mutation
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
}