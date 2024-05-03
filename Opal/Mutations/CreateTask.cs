using ProtoBuf;

namespace Opal.Mutations;

[ProtoContract]
public record CreateTask : MutationBase
{
    [ProtoMember(1)] public required Guid Id { get; init; }
    [ProtoMember(2)] public required string Name { get; init; }
}