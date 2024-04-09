using System.Collections.Concurrent;
using DistributedSessions.Mutations;
using ProtoBuf;

namespace DistributedSessions;

public class MutationReader
{
    private readonly string _workspace;
    private readonly ConcurrentQueue<Mutation> _mutationRead;
    private readonly FileSystemWatcher _fileSystemWatcher;
    private readonly ConcurrentQueue<string> _updatedPaths = new();

    
    public MutationReader(ConcurrentQueue<Mutation> mutationRead, string workspace)
    {
        _mutationRead = mutationRead;
        _workspace = workspace;

        _fileSystemWatcher = new FileSystemWatcher(_workspace)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = false,
            NotifyFilter = NotifyFilters.LastWrite
        };
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        // subscribe to file events
        _fileSystemWatcher.EnableRaisingEvents = true;
        _fileSystemWatcher.Created += (_, e) =>
        {
            if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory))
                return;

            if(!_updatedPaths.Contains(e.FullPath))
                _updatedPaths.Enqueue(e.FullPath);
        };
        
        _fileSystemWatcher.Changed += (_, e) =>
        {
            if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory))
                return;

            if(!_updatedPaths.Contains(e.FullPath))
                _updatedPaths.Enqueue(e.FullPath);
        };

        Directory.CreateDirectory(_workspace);
        
        foreach (var file in Directory.EnumerateFiles(_workspace, string.Empty, SearchOption.AllDirectories))
            _updatedPaths.Enqueue(file);

        while (!ct.IsCancellationRequested)
        {
            await UpdatePaths(ct);
            await Task.Delay(10, ct);
        }
    }


    private async Task UpdatePaths(CancellationToken ct)
    {
        while (_updatedPaths.TryDequeue(out var path) && !ct.IsCancellationRequested)
        {
            try
            {
                await using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Write);
                var mutations = Serializer.DeserializeItems<Mutation>(stream, PrefixStyle.Base128, 0).ToList();

                foreach (var mutation in mutations)
                    _mutationRead.Enqueue(mutation);
            }
            catch (IOException ex)
            {
                // retry later
                _updatedPaths.Enqueue(path);
                Console.WriteLine(ex);
            }
            
            // todo implement proper error handling without unlimited retries
        }
    }
}