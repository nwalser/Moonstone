using MessagePack;

namespace Implementation.Mutations;

[MessagePackObject]
public record CreateTask : IMutation
{
    [Key(0)] public required Guid Id { get; set; }
    [Key(1)] public required string Name { get; set; }
}