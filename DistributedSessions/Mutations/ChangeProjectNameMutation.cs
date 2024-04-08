using ProtoBuf;

namespace DistributedSessions.Mutations;

public class ChangeProjectNameMutation : Mutation
{
    [ProtoMember(3)]
    public required Guid Id { get; set; }
    
    [ProtoMember(4)]
    public required string Name { get; set; }
}