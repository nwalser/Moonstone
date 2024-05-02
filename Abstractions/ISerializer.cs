namespace Abstractions;

public interface ISerializer<TType>
{
    public byte[] Serialize(TType entry);
    public TType Deserialize(byte[] bytes);
}