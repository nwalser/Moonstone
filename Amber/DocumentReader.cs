using System.Text.Json;

namespace Amber;

public class DocumentReader
{
    public required string Session { get; init; }
    public required IHandler Handler { get; init; }
    
    
    public void Create(string folder) // todo do not pass folder directly
    {
        if (Directory.Exists(folder)) throw new Exception(); // todo: better exception
        
        Directory.CreateDirectory(folder);
        File.Create(Path.Join(folder, ".keep"));
    }

    public void Delete(string folder)
    {
        if (!Directory.Exists(folder)) throw new Exception();
        Directory.Delete(folder, recursive: true);
    }
    
    public async Task Append(string folder, object mutation, CancellationToken ct = default)
    {
        var filePath = Path.Join(folder, $"{Session}.txt");

        if (!Directory.Exists(folder)) throw new DirectoryNotFoundException();
        
        var occurenceBase64 = Convert.ToBase64String(BitConverter.GetBytes(DateTime.UtcNow.Ticks));
        
        var typeId = Handler.GetMutationTypeId(mutation.GetType());
        
        var mutationJson = JsonSerializer.Serialize(mutation);
        var mutationBytes = System.Text.Encoding.UTF8.GetBytes(mutationJson);
        var mutationBase64 = Convert.ToBase64String(mutationBytes);

        var line = string.Join(",", typeId, occurenceBase64, mutationBase64);
        
        await File.AppendAllLinesAsync(filePath, [line], ct);
    }

    public async Task<object> Read(string folder, CancellationToken ct = default)
    {
        var mutationLogs = Directory.EnumerateFiles(folder, "*.txt");
        var mutations = new List<(DateTime occurence, object mutation)>();
        
        foreach (var mutationLog in mutationLogs)
        {
            await using var stream = File.Open(mutationLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(stream);

            while (!sr.EndOfStream || ct.IsCancellationRequested)
            {
                var line = await sr.ReadLineAsync(ct) ?? throw new InvalidOperationException();
                var segments = line.Split(',');

                var typeId = int.Parse(segments[0]);
                var occurence = new DateTime(BitConverter.ToInt64(Convert.FromBase64String(segments[1]), 0));
                var json = Convert.FromBase64String(segments[2]);
                
                var type = Handler.MutationTypes[typeId];
                var mutation = JsonSerializer.Deserialize(json, type) ?? throw new InvalidOperationException();
                
                mutations.Add((occurence, mutation));
            }
        }

        var document = Handler.CreateNew();

        var orderedMutations = mutations
            .OrderBy(m => m.occurence)
            .Select(m => m.mutation);
        
        foreach (var orderedMutation in orderedMutations)
            Handler.ApplyMutation(document, orderedMutation);

        return document;
    }
}