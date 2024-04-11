using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Moonstone.Framework.Stream;

namespace Moonstone.Framework;

public class MutationWriter : BackgroundWorker<MutationWriter>
{
    private static readonly int MaxMutationsPerFile = 10_000;
    
    private readonly ConcurrentQueue<Mutation> _writeMutation;

    private readonly PathProvider _paths;

    
    private int _fileCounter;
    private int _mutations;
    
    
    public MutationWriter(ConcurrentQueue<Mutation> writeMutation, PathProvider paths, CancellationToken ct, ILogger<MutationWriter> logger) : base(ct, logger)
    {
        _writeMutation = writeMutation;
        _paths = paths;

        StartBackgroundWorker(ct);
    }

    protected override async Task Initialize(CancellationToken ct)
    {
        // create session if it does not exist
        Directory.CreateDirectory(_paths.GetSessionMutationsFolder());
        
        var fileCounter = Directory
            .EnumerateFiles(_paths.GetSessionMutationsFolder(), "*.nljson")
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
            var mutations = await JsonNewlineFile.Read<Mutation>(_paths.GetSessionMutationsFile(fileCounter.Value));
            
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

            var mutationsFile = _paths.GetSessionMutationsFile(_fileCounter);
            await JsonNewlineFile.Append(mutation, mutationsFile);
            _mutations++;

            // dequeue if processed successfully
            _writeMutation.TryDequeue(out _);
        }
    }
}