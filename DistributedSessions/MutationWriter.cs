using System.Collections.Concurrent;
using DistributedSessions.Mutations;
using Microsoft.Extensions.Logging;
using ProtoBuf;

namespace DistributedSessions;

public class MutationWriter : BackgroundWorker<MutationWriter>
{
    private static readonly int MaxMutationsPerFile = 10_000;
    
    private readonly ConcurrentQueue<Mutation> _writeMutation;
    
    private readonly string _workspace;
    private readonly Guid _sessionId;
    
    private int _fileCounter;
    private int _mutations;
    
    
    public MutationWriter(string workspace, Guid sessionId, ConcurrentQueue<Mutation> writeMutation, CancellationToken ct, ILogger<MutationWriter> logger) : base(ct, logger)
    {
        _workspace = workspace;
        _sessionId = sessionId;
        _writeMutation = writeMutation;
    }

    protected override async Task Initialize(CancellationToken ct)
    {
        // create session if it does not exist
        Directory.CreateDirectory(PathFactory.GetSessionMutationsFolder(_workspace, _sessionId));
        
        var fileCounter = Directory
            .EnumerateFiles(PathFactory.GetSessionMutationsFolder(_workspace, _sessionId), "*.nljson")
            .Select(Path.GetFileNameWithoutExtension)
            .Select(p => int.TryParse(p, out var value) ? value : default(int?))
            .Max(i => i);

        if (fileCounter is null)
        {
            // if no file is in session
            _fileCounter = 0;
            _mutations = 0;
        }
        else
        {
            // read mutations from latest file
            await using var stream = File.OpenRead(PathFactory.GetSessionMutationsFile(_workspace, _sessionId, fileCounter.Value));
            var mutations =  Serializer.DeserializeItems<Mutation>(stream, PrefixStyle.Base128, 0).ToList();
            
            
            _fileCounter = fileCounter.Value;
            _mutations = mutations.Count;
        }
    }
    
    protected override async Task Stop(CancellationToken ct)
    {
        await ProcessWork(ct);
    }
    
    protected override async Task ProcessWork(CancellationToken ct)
    {
        while (_writeMutation.TryPeek(out var mutation) && !ct.IsCancellationRequested)
        {
            // mutation maximum per file reached
            if (_mutations >= MaxMutationsPerFile)
            {
                _fileCounter++;
                _mutations = 0;
            }

            var mutationsFile = PathFactory.GetSessionMutationsFile(_workspace, _sessionId, _fileCounter);
            await NlJson.Append(mutation, mutationsFile);
            _mutations++;

            // dequeue if processed successfully
            _writeMutation.TryDequeue(out _);
        }
    }
}