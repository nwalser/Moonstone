using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moonstone.Framework.Mutations;
using Moonstone.Framework.Projection;
using Moonstone.Framework.Stream;

namespace Moonstone.Framework;

public class MutationStream : BackgroundWorker<MutationStream>
{
    private readonly HashSet<Guid> _mutationIds;
    
    private readonly ConcurrentQueue<Mutation> _newMutations;
    private readonly ConcurrentQueue<Snapshot> _newSnapshot;
    
    
    private readonly PathProvider _paths;
    private readonly StreamStore _store;
    
    private static readonly List<TimeSpan> WantedSnapshotAges =
    [
        TimeSpan.Zero,
        TimeSpan.FromSeconds(10),
        TimeSpan.FromMinutes(10),
        TimeSpan.FromDays(7)
    ];
    
    public MutationStream(ConcurrentQueue<Mutation> newMutations, ConcurrentQueue<Snapshot> newSnapshot, PathProvider paths, CancellationToken ct, ILogger<MutationStream> logger) : base(ct, logger)
    {
        _newMutations = newMutations;
        _newSnapshot = newSnapshot;
        _paths = paths;

        _mutationIds = new HashSet<Guid>();

        Directory.CreateDirectory(_paths.GetStreamStoreFolder());
        
        var optionsBuilder = new DbContextOptionsBuilder<StreamStore>()
            .UseSqlite($"Data Source={_paths.GetStreamStoreDbFile()}");
        
        _store = new StreamStore(optionsBuilder.Options);
        
        StartBackgroundWorker(ct);
    }

    
    protected override async Task Initialize(CancellationToken ct)
    {
        await _store.Database.EnsureCreatedAsync(ct);
        await _store.Database.MigrateAsync(cancellationToken: ct);
        
        // load existing mutation ids into memory
        var mutationIds = await _store.CachedMutations
            .Select(m => m.MutationId)
            .ToListAsync(cancellationToken: ct);

        foreach (var mutationId in mutationIds)
            _mutationIds.Add(mutationId);
    }
    
    protected override async Task ProcessWork(CancellationToken ct)
    {
        var eventsIngested = await IngestNewMutations(ct);

        if(!eventsIngested)
            return;
            
        await RebuildSnapshots(ct);
    }
    
    private async Task<bool> IngestNewMutations(CancellationToken ct)
    {
        var eventsIngested = false;

        Mutation? oldestMutation = default;
        
        while (_newMutations.TryDequeue(out var mutation) && !ct.IsCancellationRequested)
        {
            if (!_mutationIds.Add(mutation.Id))
                continue;
            
            _store.CachedMutations.Add(CachedMutation.FromMutation(mutation));
            
            eventsIngested = true;
            
            if (mutation.Occurence < oldestMutation?.Occurence)
                oldestMutation = mutation;
        }

        // invalidate caches
        if (oldestMutation is not null)
        {
            var invalidCaches = _store.CachedSnapshots
                .Where(s => oldestMutation.Occurence < s.LastMutationOccurence)
                .ToList();
            
            foreach (var invalidCache in invalidCaches)
                _store.Remove(invalidCache);
        }

        await _store.SaveChangesAsync(ct);
        
        return eventsIngested;
    }
    
    private async Task RebuildSnapshots(CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        foreach (var wantedSnapshotAge in WantedSnapshotAges.OrderDescending())
        {
            var targetDate = now - wantedSnapshotAge;
            
            var bestParent = await _store.CachedSnapshots
                .Where(s => s.LastMutationOccurence < targetDate)
                .OrderByDescending(s => s.LastMutationOccurence)
                .FirstOrDefaultAsync(cancellationToken: ct);

            var snapshot = bestParent != null ? CachedSnapshot.CopyFromCached(bestParent) : Snapshot.Create();
            
            // apply remaining mutations on top of it
            var remainingMutations = _store.CachedMutations
                .Where(m => m.Occurence > snapshot.LastMutationOccurence)
                .Where(m => m.Occurence <= targetDate)
                .OrderBy(m => m.Occurence)
                .AsAsyncEnumerable();
            
            await foreach (var cachedMutation in remainingMutations)
                snapshot.AppendMutation(CachedMutation.ToMutation(cachedMutation));
            
            var snapshotToReplace = _store.CachedSnapshots
                .SingleOrDefault(s => s.TargetAge == wantedSnapshotAge);
            
            if (snapshotToReplace is not null)
                _store.CachedSnapshots.Remove(snapshotToReplace);

            _store.CachedSnapshots.Add(CachedSnapshot.ToCached(Guid.NewGuid(), wantedSnapshotAge, snapshot));

            if (wantedSnapshotAge == TimeSpan.Zero)
                _newSnapshot.Enqueue(snapshot);
        }

        await _store.SaveChangesAsync(ct);
    }
}