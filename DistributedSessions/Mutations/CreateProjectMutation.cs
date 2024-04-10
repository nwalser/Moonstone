namespace DistributedSessions.Mutations;

public class CreateProjectMutation : Mutation
{    
    public required Guid ProjectId { get; set; }
    public required string Name { get; set; }
}