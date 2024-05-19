using System.Text.Json;

namespace Moonstone;

public class Reader<TDocument>
{
    private readonly string _path;
    private readonly string _session;
    private readonly IHandler<TDocument> _handler;
    
    public Reader(string path, string session, IHandler<TDocument> handler)
    {
        _path = path;
        _session = session;
        _handler = handler;
    }

    private string GetDocumentPath(DocumentIdentity identity)
    {
        return Path.Join(_path, identity.TypeId.ToString(), identity.Id.ToString());
    }
    
    public void AppendMutation(DocumentIdentity identity, object mutation)
    {
        var folder = GetDocumentPath(identity);
        var filePath = Path.Join(folder, $"{_session}.txt");

        if (!Directory.Exists(folder)) throw new DirectoryNotFoundException();

        var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
        using var sw = new StreamWriter(stream);
        SerializeEntry(sw, DateTime.UtcNow.Ticks, mutation);
    }

    public TDocument Read(DocumentIdentity identity, CancellationToken ct = default)
    {
        var folder = GetDocumentPath(identity);

        var mutationLogs = Directory.EnumerateFiles(folder, "*.txt");
        var mutations = new List<(long occurence, object mutation)>();
        
        foreach (var mutationLog in mutationLogs)
        {
            using var stream = File.Open(mutationLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(stream);

            while (!sr.EndOfStream || ct.IsCancellationRequested)
            {
                var mutation = DeserializeEntry(sr);
                mutations.Add(mutation);
            }
        }

        var document = _handler.CreateNew();

        var orderedMutations = mutations
            .OrderBy(m => m.occurence)
            .Select(m => m.mutation);
        
        foreach (var orderedMutation in orderedMutations)
            _handler.ApplyMutation(document, orderedMutation);

        return document;
    }
    
    private void SerializeEntry(StreamWriter sw, long occurence, object mutation)
    {
        {
            var typeId = _handler.MutationTypes.Single(t => t.Value == mutation.GetType()).Key.ToString();
            sw.Write(typeId);
            sw.Write(",");
        }

        {
            var bytes = BitConverter.GetBytes(occurence);
            var base64 = Convert.ToBase64String(bytes);
            sw.Write(base64);
            sw.Write(",");
        }

        {
            var json = JsonSerializer.Serialize(mutation);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var base64 = Convert.ToBase64String(bytes);
            sw.Write(base64);
            sw.WriteLine();
        }
    }

    private (long occurence, object mutation) DeserializeEntry(TextReader sr)
    {
        var line = sr.ReadLine() ?? throw new InvalidOperationException();
        var segments = line.Split(',');

        var occurence = BitConverter.ToInt64(Convert.FromBase64String(segments[1]), 0);
                
        var typeId = int.Parse(segments[0]);
        var json = Convert.FromBase64String(segments[2]);
        var type = _handler.MutationTypes[typeId];
        var mutation = JsonSerializer.Deserialize(json, type) ?? throw new InvalidOperationException();

        return (occurence, mutation);
    }
}