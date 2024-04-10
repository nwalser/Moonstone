namespace DistributedSessions.Mutations;

public class ChangeProjectNameMutation : Mutation
{
    public required Guid ProjectId { get; set; }
    public required string Name { get; set; }
}