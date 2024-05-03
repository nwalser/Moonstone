using ProtoBuf;

namespace Opal.Mutations;

[ProtoContract]
[ProtoInclude(1, typeof(CreateTask))]
[ProtoInclude(2, typeof(DeleteTask))]
public record MutationBase { }