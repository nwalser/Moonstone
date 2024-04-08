using ProtoBuf;

namespace DistributedSessions.Mutations;

public class CreateProjectMutation : Mutation
{    
    [ProtoMember(3)]
    public required Guid ProjectId { get; set; }
    
    [ProtoMember(4)]
    public required string Name { get; set; }
}