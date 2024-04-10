using ProtoBuf;

namespace DistributedSessions.Mutations;

public class DeleteProjectMutation : Mutation
{
    public required Guid ProjectId { get; set; }
}