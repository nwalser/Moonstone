using ProtoBuf;

namespace Map.Sketch.Store;

[ProtoContract]
public record Mutation
{
    [ProtoMember(0)] public required long Tick { get; init; }
    [ProtoMember(1)] public required int Table { get; set; }
    [ProtoMember(1)] public required Guid Row { get; set; }
    [ProtoMember(2)] public required int Column { get; init; }
    [ProtoMember(3)] public required byte[] Value { get; init; }
}
