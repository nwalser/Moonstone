namespace Abstractions.Serializer;

public class ByteTextSerializer : ITextSerializer<byte[]>
{
    public string Serialize(byte[] entry)
    {
        return Convert.ToBase64String(entry);
    }

    public byte[] Deserialize(string text)
    {
        return Convert.FromBase64String(text);
    }
}