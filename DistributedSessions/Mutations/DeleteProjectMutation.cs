using ProtoBuf;

namespace DistributedSessions.Mutations;

public class DeleteProjectMutation : Mutation
{
    [ProtoMember(3)]
    public required Guid Id { get; set; }
}