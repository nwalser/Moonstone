using MessagePack;

namespace Moonstone.Workspace.Data;

public interface IMutation
{
    public byte[] Serialize()
    {
        return MessagePackSerializer.Typeless.Serialize(this);
    }

    public static IMutation Deserialize(byte[] bytes)
    {
        return MessagePackSerializer.Typeless.Deserialize(bytes) as IMutation ?? throw new InvalidOperationException();
    }
}