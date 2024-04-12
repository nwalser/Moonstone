﻿using System.Collections.Concurrent;
using System.Reactive.Subjects;
using Microsoft.EntityFrameworkCore;
using Moonstone.Framework;
using Moonstone.Framework.Stream;
using Serilog;

namespace Moonstone.Workspace;

public class Workspace<TProjection> where TProjection : new()
{
    private readonly PathProvider _paths; 
    private readonly MutationHandler<TProjection> _mutationHandler;
    
    private FileSystemWatcher? _fileSystemWatcher;
    private SnapshotManager<TProjection>? _mutationStream;
    private MutationWriter? _mutationWriter;

    private readonly ConcurrentQueue<string> _changedFiles = new();
    private readonly ConcurrentQueue<Mutation> _mutationsToWrite = new();

    public IObservable<TProjection> Projection => _projection;
    private readonly Subject<TProjection> _projection = new();

    private Task? _backgroundTask;
    private CancellationTokenSource? _backgroundTaskCts;
    
    private bool _initialized;
    
    public Workspace(PathProvider paths, MutationHandler<TProjection> mutationHandler)
    {
        _paths = paths;
        _mutationHandler = mutationHandler;
    }

    
    public async Task Open(CancellationToken ct = default)
    {
        Log.Logger.Information("Start opening workspace");
        
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
            var mutations = await JsonNewlineFile.ReadAsync<Mutation>(changedFile, ct);
            await _mutationStream.IngestMutations(mutations, ct);
        }

        // calculate initial projection
        await RebuildProjection(ct);
        
        // create mutation writer
        _mutationWriter = new MutationWriter(_paths);
        await _mutationWriter.Initialize(ct);
        
        // start background jobs
        _backgroundTaskCts = new CancellationTokenSource();
        _backgroundTask = Task.Run(async () => await WorkerTask(_backgroundTaskCts.Token), _backgroundTaskCts.Token);

        _initialized = true;
        Log.Logger.Information("Workspace opened");
    }
    
    public async Task Close(CancellationToken ct = default)
    {
        Log.Logger.Information("Start closing workspace");

        if (_backgroundTask is null || _backgroundTaskCts is null)
            throw new InvalidOperationException();
        
        // cancel background task and wait until it completes
        await _backgroundTaskCts.CancelAsync();
        await _backgroundTask;

        Log.Logger.Information("Flush remaining mutations to disk");

        // write remaining mutations to disk
        await ProcessMutationWrites(ct);
        Log.Logger.Information("Workspace closed");
    }

    public void ApplyMutation(Mutation mutation)
    {
        if (!_initialized)
            throw new InvalidOperationException("Workspace needs to be opened before mutations may be written to it");
        
        _mutationsToWrite.Enqueue(mutation);
    }

    
    private async Task WorkerTask(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await ProcessMutationWrites(ct);
                await ProcessChangedFiles(ct);

                await Task.Delay(100, ct);
                // todo implement good retry strategy
            }
            catch (TaskCanceledException)
            {
                Log.Logger.Information("Workspace background task got cancelled");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Workspace background task encountered an error");   
            }
        }
    }
    
    private void FileChanged(object sender, FileSystemEventArgs e)
    {
        if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory))
            return;

        // do not process changes from own session directory
        if (DirectoryHelper.IsSubdirectory(_paths.GetSessionPath(), e.FullPath))
            return;
        
        var relativePath = Path.GetRelativePath(_paths.Workspace, e.FullPath);
            
        if(!_changedFiles.Contains(relativePath))
            _changedFiles.Enqueue(relativePath);
    }
    
    private async Task ProcessChangedFiles(CancellationToken ct = default)
    {
        if (_mutationStream is null)
            throw new InvalidOperationException();

        if (_changedFiles.IsEmpty) 
            return;
        
        while (_changedFiles.TryPeek(out var relativePath) && !ct.IsCancellationRequested)
        {
            var path = Path.Join(_paths.Workspace, relativePath);
            var mutations  = await JsonNewlineFile.ReadAsync<Mutation>(path, ct);
            
            await _mutationStream.IngestMutations(mutations, ct);
            
            _changedFiles.TryDequeue(out _);
        }
        
        await RebuildProjection(ct);
    }

    private async Task ProcessMutationWrites(CancellationToken ct = default)
    {
        if (_mutationStream is null || _mutationWriter is null)
            throw new InvalidOperationException();

        if (_mutationsToWrite.IsEmpty) 
            return;

        var mutationsToIngest = new List<Mutation>();
        while (_mutationsToWrite.TryPeek(out var mutation) && !ct.IsCancellationRequested)
        {
            await _mutationWriter.WriteMutation(mutation);
            mutationsToIngest.Add(mutation);
            
            _mutationsToWrite.TryDequeue(out _);
        }
        
        await _mutationStream.IngestMutations(mutationsToIngest, ct);

        await RebuildProjection(ct);
    }

    private async Task RebuildProjection(CancellationToken ct = default)
    {
        if (_mutationStream is null)
            throw new InvalidOperationException();
        
        var newProjection = await _mutationStream.RebuildProjection(ct);
        _projection.OnNext(newProjection);
    }
}