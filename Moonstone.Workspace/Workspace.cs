using System.Collections.Concurrent;
using System.Reactive.Subjects;
using Microsoft.EntityFrameworkCore;
using Moonstone.Framework;
using Moonstone.Framework.Stream;
using static System.Linq.AsyncEnumerable;

namespace Moonstone.Workspace;

public class Workspace<TProjection> where TProjection : new()
{
    private readonly PathProvider _paths; 
    private readonly MutationHandler<TProjection> _mutationHandler;
    
    private FileSystemWatcher? _fileSystemWatcher;
    private SnapshotManager<TProjection>? _mutationStream;

    private readonly ConcurrentQueue<string> _changedFiles = new();
    private readonly ConcurrentQueue<Mutation> _possibleNewMutations = new();

    public IObservable<TProjection> Projection => _projection;
    private readonly Subject<TProjection> _projection = new();
    
    
    public Workspace(PathProvider paths, MutationHandler<TProjection> mutationHandler)
    {
        _paths = paths;
        _mutationHandler = mutationHandler;
    }

    
    public async Task Open(CancellationToken ct = default)
    {
        // create all directories needed
        Directory.CreateDirectory(_paths.Workspace);
        Directory.CreateDirectory(_paths.Temporary);
        Directory.CreateDirectory(_paths.GetStreamStoreFolder());
        Directory.CreateDirectory(_paths.GetSessionMutationsFolder());
        
        // open database for stream storage
        var optionsBuilder = new DbContextOptionsBuilder<StreamStore>()
            .UseSqlite($"Data Source={_paths.GetStreamStoreDbFile()}");
        
        var store = new StreamStore(optionsBuilder.Options);
        await store.Database.EnsureCreatedAsync(ct);
        await store.Database.MigrateAsync(cancellationToken: ct);

        _mutationStream = new SnapshotManager<TProjection>(store, _mutationHandler);
        await _mutationStream.Initialize(ct);
        
        // create filesystem watcher
        _fileSystemWatcher = new FileSystemWatcher
        {
            Path = _paths.Workspace,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.LastWrite,
        };
        _fileSystemWatcher.Created += FileChanged;
        _fileSystemWatcher.Changed += FileChanged;

        // ingest all events that might have occured
        var changedFiles = Directory
            .EnumerateFiles(_paths.Workspace, string.Empty, SearchOption.AllDirectories)
            .ToList();

        foreach (var changedFile in changedFiles)
        {
            var mutations = await JsonNewlineFile.Read<Mutation>(changedFile);
            await _mutationStream.IngestMutations(mutations, ct);
        }

        // calculate initial projection
        var newProjection = await _mutationStream.RebuildProjection(ct);
        _projection.OnNext(newProjection);
        
        // create mutation writer
        
        
        
        // start background jobs
        // todo:
    }

    private void FileChanged(object sender, FileSystemEventArgs e)
    {
        if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory))
            return;
            
        var relativePath = Path.GetRelativePath(_paths.Workspace, e.FullPath);
            
        if(!_changedFiles.Contains(relativePath))
            _changedFiles.Enqueue(relativePath);
    }
    
    private async Task ProcessChangedFiles(CancellationToken ct = default)
    {
        while (_changedFiles.TryPeek(out var path) && !ct.IsCancellationRequested)
        {
            var mutations  = await JsonNewlineFile.Read<Mutation>(path);
            
            foreach (var mutation in mutations)
                _possibleNewMutations.Enqueue(mutation);

            _changedFiles.TryDequeue(out _);
        }
    }

    private async Task ProcessNewMutations(CancellationToken ct = default)
    {
        while (_possibleNewMutations.TryPeek(out var path) && !ct.IsCancellationRequested)
        {
            
            
            _possibleNewMutations.TryDequeue(out _);
        }
    }
    

    public Task Close()
    {
        throw new NotImplementedException();
    }
}