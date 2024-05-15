using System.Text.Json;
using Amber.Documents;
using DirectoryNotFoundException = System.IO.DirectoryNotFoundException;

namespace Amber.Mutation;

public class DocumentFileStore<TDocument>
{
    public required string Session { get; init; }
    public required IHandler<TDocument> Handler { get; init; }
    public required string Folder { get; set; }
    
    
    public static DocumentFileStore<TDocument> Init(string folder, string session, IHandler<TDocument> handler)
    {
        if (!Directory.Exists(folder)) throw new DocumentDoesNotExistException();

        return new DocumentFileStore<TDocument>()
        {
            Folder = folder,
            Session = session,
            Handler = handler,
        };
    }

    public static DocumentFileStore<TDocument> Create(string folder, string session, IHandler<TDocument> handler)
    {
        if (Directory.Exists(folder)) throw new DocumentAlreadyExistsException();

        var sessionPath = Path.Join(folder, $"{session}.txt");
        Directory.CreateDirectory(folder);
        
        using var fs = File.Create(sessionPath);
        fs.Close();

        return new DocumentFileStore<TDocument>()
        {
            Folder = folder,
            Session = session,
            Handler = handler,
        };
    }
    
    
    public async Task Append(string folder, object mutation, CancellationToken ct = default)
    {
        if (!Directory.Exists(folder)) throw new DirectoryNotFoundException();
        
        var sessionPath = Path.Join(folder, $"{Session}.txt");
        
        var occurenceBase64 = Convert.ToBase64String(BitConverter.GetBytes(DateTime.UtcNow.Ticks));
        
        var typeId = Handler.MutationTypes.Single(t => t.Value == mutation.GetType()).Key;
        
        var mutationJson = JsonSerializer.Serialize(mutation);
        var mutationBytes = System.Text.Encoding.UTF8.GetBytes(mutationJson);
        var mutationBase64 = Convert.ToBase64String(mutationBytes);

        var line = string.Join(",", typeId, occurenceBase64, mutationBase64);
        
        await File.AppendAllLinesAsync(sessionPath, [line], ct);
    }

    public async Task<TDocument> Read(string folder, CancellationToken ct = default)
    {
        var mutationLogs = Directory.EnumerateFiles(folder);
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