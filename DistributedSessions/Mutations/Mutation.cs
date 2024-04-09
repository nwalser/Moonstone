using ProtoBuf;

namespace DistributedSessions.Mutations;

[ProtoContract]
[ProtoInclude(500, typeof(ChangeProjectNameMutation))]
[ProtoInclude(501, typeof(CreateProjectMutation))]
[ProtoInclude(502, typeof(DeleteProjectMutation))]

public abstract class Mutation
{
    [ProtoMember(1)]
    public required Guid MutationId { get; set; }
    
    [ProtoMember(2)]
    public required MutationOccurence Occurence { get; set; }
}