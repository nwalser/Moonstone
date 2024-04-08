using DistributedSessions.Mutations;
using ProtoBuf;

namespace DistributedSessions;

[ProtoContract]
[ProtoInclude(500, typeof(ChangeProjectNameMutation))]
[ProtoInclude(501, typeof(CreateProjectMutation))]
[ProtoInclude(502, typeof(DeleteProjectMutation))]

public abstract class Mutation
{
    [ProtoMember(1)]
    public required Guid MutationId { get; set; }
    
    [ProtoMember(2)]
    public required DateTime Occurence { get; set; }
}