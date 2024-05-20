using System.Text.Json;

namespace Moonstone;

public static class Store
{
    public static void AppendMutation<TDocument>(string folder, string session, object mutation, IHandler<TDocument> handler)
    {
        var filePath = Path.Join(folder, $"{session}.txt");

        if (!Directory.Exists(folder)) throw new DirectoryNotFoundException();

        var stream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
        using var sw = new StreamWriter(stream);
        SerializeEntry(sw, DateTime.UtcNow.Ticks, mutation, handler);
    }
    
    public static TDocument Read<TDocument>(string folder, IHandler<TDocument> handler, CancellationToken ct = default)
    {
        var mutationLogs = Directory.EnumerateFiles(folder, "*.txt");
        var mutations = new List<(long occurence, object mutation)>();
        
        foreach (var mutationLog in mutationLogs)
        {
            using var stream = File.Open(mutationLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(stream);

            while (!sr.EndOfStream || ct.IsCancellationRequested)
            {
                var mutation = DeserializeEntry(sr, handler);
                mutations.Add(mutation);
            }
        }

        var document = handler.CreateNew();

        var orderedMutations = mutations
            .OrderBy(m => m.occurence)
            .Select(m => m.mutation);
        
        foreach (var orderedMutation in orderedMutations)
            handler.ApplyMutation(document, orderedMutation);

        return document;
    }
    
    private static void SerializeEntry<TDocument>(TextWriter sw, long occurence, object mutation, IHandler<TDocument> handler)
    {
        {
            var typeId = handler.MutationTypes.Single(t => t.Value == mutation.GetType()).Key.ToString();
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

    private static (long occurence, object mutation) DeserializeEntry<TDocument>(TextReader sr, IHandler<TDocument> handler)
    {
        var line = sr.ReadLine() ?? throw new InvalidOperationException();
        var segments = line.Split(',');

        var typeId = int.Parse(segments[0]);
        var occurence = BitConverter.ToInt64(Convert.FromBase64String(segments[1]), 0);
        var json = Convert.FromBase64String(segments[2]);
                
        var type = handler.MutationTypes[typeId];
        var mutation = JsonSerializer.Deserialize(json, type) ?? throw new InvalidOperationException();

        return (occurence, mutation);
    }
}