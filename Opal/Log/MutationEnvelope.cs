using ProtoBuf;

namespace Opal.Log;

[ProtoContract]
public class MutationEnvelope<TMutation>
{
    [ProtoMember(1)] public required Guid Id { get; init; }
    [ProtoMember(2)] public required TMutation Mutation { get; init; }
}