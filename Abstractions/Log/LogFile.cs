using Abstractions.Serializer;

namespace Abstractions.Log;

public class LogFile<TEntry>
{
    private readonly ITextSerializer<TEntry> _serializer;

    public LogFile(ITextSerializer<TEntry> serializer)
    {
        _serializer = serializer;
    }

    public IEnumerable<TEntry> Read(string file, CancellationToken ct = default)
    {
        using var stream = File.Open(file, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
        using var sr = new StreamReader(stream);

        while (!sr.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = sr.ReadLine();

            if (line is null) 
                break;

            yield return _serializer.Deserialize(line);
        }

        stream.Close();
    }
    
    public void Append(TEntry entry, string file, CancellationToken ct = default)
    {
        using var stream = File.Open(file, FileMode.Append, FileAccess.Write, FileShare.Read);
        using var sw = new StreamWriter(stream);

        var line = _serializer.Serialize(entry);
        sw.WriteLine(line);

        sw.Flush();
        sw.Close();
    }

    public int GetNumberOfEntries(string file, CancellationToken ct = default)
    {
        using var stream = File.Open(file, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
        using var sr = new StreamReader(stream);

        var numberOfEntries = 0;
        
        while (!sr.EndOfStream && !ct.IsCancellationRequested)
        {
            _ = sr.ReadLine();
            numberOfEntries++;
        }

        return numberOfEntries;
    }
}