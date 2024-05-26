using ProtoBuf;

namespace Opal.Deltas;

[ProtoContract]
public class Update : IDelta
{
    [ProtoMember(1)] public required DateTime Timestamp { get; init; }
    [ProtoMember(2)] public required int TypeId { get; init; }
    [ProtoMember(3)] public required Guid RowId { get; init; }
    [ProtoMember(4)] public required string Json { get; init; }
}