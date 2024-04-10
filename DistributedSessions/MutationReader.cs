using System.Collections.Concurrent;
using DistributedSessions.Mutations;
using Microsoft.Extensions.Logging;

namespace DistributedSessions;

public class MutationReader : BackgroundWorker<MutationReader>
{
    private readonly ConcurrentQueue<Mutation> _mutationRead;
    private readonly FileSystemWatcher _fileSystemWatcher;
    private readonly PathProvider _paths;
    
    private readonly ConcurrentQueue<string> _updatedPaths = new();
    
    public MutationReader(ConcurrentQueue<Mutation> mutationRead, PathProvider paths, CancellationToken ct, ILogger<MutationReader> logger) : base(ct, logger)
    {
        _mutationRead = mutationRead;
        _paths = paths;

        _fileSystemWatcher = new FileSystemWatcher(paths.Workspace)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = false,
            NotifyFilter = NotifyFilters.LastWrite
        };
    }

    protected override Task Initialize(CancellationToken ct)
    {
        Directory.CreateDirectory(_paths.Workspace);
        
        // subscribe to file events
        _fileSystemWatcher.EnableRaisingEvents = true;
        _fileSystemWatcher.Created += (_, e) =>
        {
            if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory))
                return;
            
            // todo implement filtering of own session path
            if(!_updatedPaths.Contains(e.FullPath))
                _updatedPaths.Enqueue(e.FullPath);
        };
        
        _fileSystemWatcher.Changed += (_, e) =>
        {
            if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory))
                return;
            
            // todo implement filtering of own session path
            if(!_updatedPaths.Contains(e.FullPath))
                _updatedPaths.Enqueue(e.FullPath);
        };
        
        foreach (var file in Directory.EnumerateFiles(_paths.Workspace, string.Empty, SearchOption.AllDirectories))
            _updatedPaths.Enqueue(file);
        
        return Task.CompletedTask;
    }

    protected override async Task ProcessWork(CancellationToken ct)
    {
        while (_updatedPaths.TryPeek(out var path) && !ct.IsCancellationRequested)
        {
            var mutations  = await JsonNewlineFile.Read<Mutation>(path);
            
            foreach (var mutation in mutations)
                _mutationRead.Enqueue(mutation);

            _updatedPaths.TryDequeue(out _);
        }
    }
}