namespace Abstractions;

public interface IBinarySerializer<TType>
{
    public byte[] Serialize(TType entry);
    public TType Deserialize(byte[] bytes);
}