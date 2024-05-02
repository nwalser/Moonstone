using MessagePack;

namespace Implementation.Mutations;

[MessagePackObject]
public record DeleteTask : IMutation
{
    [Key(0)] public Guid Id { get; init; }
}