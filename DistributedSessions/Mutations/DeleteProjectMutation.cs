namespace DistributedSessions.Mutations;

public class DeleteProjectMutation : Mutation
{
    public required Guid Id { get; set; }
}