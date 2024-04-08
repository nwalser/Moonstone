using System.Collections.Concurrent;
using DistributedSessions.Mutations;
using Microsoft.Extensions.Hosting;
using ProtoBuf;

namespace DistributedSessions;

public class MutationWriter
{
    private static readonly int MaxMutationsPerFile = 10_000;
    
    private int? _fileCounter;
    private int? _mutations;
    
    private readonly string _workspaceFolder;
    private readonly Guid _sessionId;
    
    private ConcurrentQueue<Mutation> _writeMutation;
    
    private string CurrentSessionPath => Path.Join(_workspaceFolder, _sessionId.ToString());
    private string CurrentFilePath => Path.Join(_workspaceFolder, _sessionId.ToString(), $"{_fileCounter}.bin");
    
    
    public MutationWriter(string workspaceFolder, Guid sessionId, ConcurrentQueue<Mutation> writeMutation)
    {
        _workspaceFolder = workspaceFolder;
        _sessionId = sessionId;
        _writeMutation = writeMutation;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        Directory.CreateDirectory(CurrentSessionPath);
        
        _fileCounter = Directory
            .EnumerateFiles(CurrentSessionPath, "*.bin")
            .Select(Path.GetFileNameWithoutExtension)
            .Select(p => Convert.ToInt32(p))
            .Max(i => (int?)i);

        // if no file is in session
        if (_fileCounter is null)
        {
            _fileCounter = 0;
            _mutations = 0;
            return;
        }

        // read mutations from latest file
        await using (var stream = File.OpenRead(CurrentFilePath))
        {
            var mutations =  Serializer.DeserializeItems<Mutation>(stream, PrefixStyle.Base128, 0).ToList();        
            _mutations = mutations.Count;
        }

        while (!ct.IsCancellationRequested)
        {
            await StoreMutations(ct);
            await Task.Delay(100, ct);
        }
    }
    
    private async Task StoreMutations(CancellationToken ct)
    {
        while (_writeMutation.TryDequeue(out var mutation) && !ct.IsCancellationRequested)
        {
            // mutation maximum reached
            if (_mutations >= MaxMutationsPerFile)
            {
                _fileCounter++;
                _mutations = 0;
            }

            try
            {
                await using var fileStream = File.Open(CurrentFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                Serializer.SerializeWithLengthPrefix(fileStream, mutation, PrefixStyle.Base128, 0);
                await fileStream.FlushAsync(ct);
                fileStream.Close();
                
                _mutations++;
            }
            catch (IOException ex)
            {
                // retry later
                _writeMutation.Enqueue(mutation);
                Console.WriteLine(ex);
            }
            
            // todo implement proper error handling without unlimited retries
        }
    }
}