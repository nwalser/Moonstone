using ProtoBuf;

namespace Moonstone.FileFormat.Deltas;

[ProtoContract]
public class Delete : IDelta
{
    [ProtoMember(1)] public required DateTime Timestamp { get; init; }
    [ProtoMember(2)] public required int TypeId { get; init; }
    [ProtoMember(3)] public required Guid RowId { get; init; }
}