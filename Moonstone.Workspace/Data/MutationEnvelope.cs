using MessagePack;
using MessagePack.Formatters;

namespace Moonstone.Workspace.Data;

[MessagePackObject]
public class MutationEnvelope
{
    [Key(0)]
    public required Guid Id { get; init; }
    
    [Key(1)]
    public required IMutation Mutation { get; init; }


    
    public byte[] Serialize()
    {
        return MessagePackSerializer.Typeless.Serialize(this);
    }

    public static MutationEnvelope Deserialize(byte[] bytes)
    {
        return MessagePackSerializer.Typeless.Deserialize(bytes) as MutationEnvelope ?? throw new InvalidOperationException();
    }
}