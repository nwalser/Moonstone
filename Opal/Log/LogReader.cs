using ProtoBuf;

namespace Opal.Log;

public class LogReader : IDisposable
{
    private const PrefixStyle PrefixStyle = ProtoBuf.PrefixStyle.Base128;
    private const int FieldNumber = 0;
    
    private readonly FileStream _stream;
    
    public bool EndOfStream => _stream.Position >= _stream.Length;

    
    public LogReader(FileStream stream)
    {
        _stream = stream;
    }

    public static LogReader Open(string file)
    {
        var stream = File.Open(file, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
        return new LogReader(stream);
    }

    public int NumberOfEntries()
    {
        var counter = 0;
        
        while (!EndOfStream)
        {
            counter++;
            SkipNext();
        }

        return counter;
    }
    
    public void Skip(int numberOfEntries)
    {
        for (var i = 0; i < numberOfEntries; i++)
        {
            SkipNext();
        }
    }
    
    public void SkipNext()
    {
        if (Serializer.TryReadLengthPrefix(_stream, PrefixStyle, out var length))
        {
            _stream.Seek(length, SeekOrigin.Current);
        }
        else
        {
            throw new InvalidOperationException("Could not determine length prefix");
        }
    }
    
    public TEntry ReadNext<TEntry>()
    {
        return Serializer.DeserializeWithLengthPrefix<TEntry>(_stream, PrefixStyle, FieldNumber);
    }

    public void Dispose()
    {
        _stream.Dispose();
    }
}