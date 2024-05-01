using MessagePack;
using Moonstone.Workspace.Data;

namespace Moonstone.Workspace.MutationStream;

public static class MutationLog
{
    public static IEnumerable<MutationEnvelope> Read(string file, CancellationToken ct = default)
    {
        using var stream = File.Open(file, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
        using var sr = new StreamReader(stream);

        while (!sr.EndOfStream && !ct.IsCancellationRequested)
        {
            var base64 = sr.ReadLine();

            if (base64 is null) 
                break;
            
            var bytes = Convert.FromBase64String(base64);
            var entry = MessagePackSerializer.Deserialize<MutationEnvelope>(bytes, cancellationToken: ct);

            yield return entry;
        }

        stream.Close();
    }
    
    public static void Append(MutationEnvelope entry, string file, CancellationToken ct = default)
    {
        using var stream = File.Open(file, FileMode.Append, FileAccess.Write, FileShare.Read);
        using var sw = new StreamWriter(stream);
        
        var bytes = MessagePackSerializer.Serialize(entry, cancellationToken: ct);
        var base64 = Convert.ToBase64String(bytes);
        sw.WriteLine(base64);

        sw.Flush();
        sw.Close();
    }

    public static int GetNumberOfEntries(string file, CancellationToken ct = default)
    {
        using var stream = File.Open(file, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
        using var sr = new StreamReader(stream);

        var numberOfEntries = 0;
        
        while (sr.EndOfStream && !ct.IsCancellationRequested)
        {
            _ = sr.ReadLine();
            numberOfEntries++;
        }

        return numberOfEntries;
    }
}