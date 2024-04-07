using MsgPack.Serialization;

namespace Stream.Mutations;

[MessagePackRuntimeType]
public abstract class Mutation
{
    public Guid MutationId { get; set; }
    public DateTime Occurence { get; set; }
}