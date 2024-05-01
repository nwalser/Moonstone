using MessagePack;
using MessagePack.Formatters;

namespace Framework;

[MessagePackObject]
public class MutationLogEntry
{
    [Key(0)] 
    public required Guid Id { get; init; }
    
    [Key(1)] 
    [MessagePackFormatter(typeof(TypelessFormatter))]
    public required IMutation Mutation { get; init; }
}