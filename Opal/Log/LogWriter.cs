using ProtoBuf;

namespace Opal.Log;

public class LogWriter : IDisposable
{
    private const PrefixStyle PrefixStyle = ProtoBuf.PrefixStyle.Base128;
    private const int FieldNumber = 0;
    
    private readonly FileStream _stream;

    public LogWriter(FileStream stream)
    {
        _stream = stream;
    }

    public static LogWriter Open(string file)
    {
        var stream = File.Open(file, FileMode.Append, FileAccess.Write, FileShare.Read);
        return new LogWriter(stream);
    }

    public void Append<TEntry>(TEntry entry)
    {
        Serializer.SerializeWithLengthPrefix(_stream, entry, PrefixStyle, FieldNumber);
    }

    public void Dispose()
    {
        _stream.Dispose();
    }
}