using Abstractions;
using Abstractions.Serializer;
using Implementation.Mutations;
using MessagePack;

namespace Implementation.Serializer;

public class MutationBinarySerializer : IBinarySerializer<IMutation>
{
    public byte[] Serialize(IMutation entry)
    {
        return MessagePackSerializer.Serialize(entry);
    }

    public IMutation Deserialize(byte[] bytes)
    {
        return MessagePackSerializer.Deserialize<IMutation>(bytes);
    }
}