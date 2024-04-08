using MsgPack.Serialization;

namespace DistributedSessions;

[MessagePackRuntimeType]
public abstract class Mutation
{
    public required Guid MutationId { get; set; }
    public required DateTime Occurence { get; set; }
}