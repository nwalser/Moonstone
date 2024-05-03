using ProtoBuf;

namespace Opal.Mutations;

[ProtoContract]
public record DeleteTask : MutationBase
{
    [ProtoMember(1)] public required Guid Id { get; init; }
}