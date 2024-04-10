using System.Collections.Concurrent;
using System.Net.Mime;
using System.Text;
using DistributedSessions.Mutations;
using DistributedSessions.Projection;
using DistributedSessions.Stream;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DistributedSessions;

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
    
    private static readonly JsonSerializerSettings Settings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
    };
    
    public MutationStream(ConcurrentQueue<Mutation> newMutations, ConcurrentQueue<Snapshot> newSnapshot, PathProvider paths, CancellationToken ct, ILogger<MutationStream> logger) : base(ct, logger)
    {
        _newMutations = newMutations;
        _newSnapshot = newSnapshot;
        _paths = paths;

        _mutationIds = new HashSet<Guid>();
        
        var optionsBuilder = new DbContextOptionsBuilder<StreamStore>()
            .UseSqlite($"Data Source={_paths.GetStreamStoreDbFile()}");
        
        _store = new StreamStore(optionsBuilder.Options);
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
            mutationIds.Add(mutationId);
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
        
        while (_newMutations.TryDequeue(out var mutation) && !ct.IsCancellationRequested)
        {
            if (!_mutationIds.Add(mutation.Id))
                continue;

            var json = JsonConvert.SerializeObject(mutation, Settings);
            var bytes = Encoding.UTF8.GetBytes(json);
            
            _store.CachedMutations.Add(new CachedMutation()
            {
                MutationId = mutation.Id,
                Occurence = mutation.Occurence,
                Mutation = bytes,
            });

            // invalidate caches
            var invalidCaches = _store.CachedSnapshots
                .Where(s => s.LastMutationOccurence > mutation.Occurence)
                .Where(s => s.LastMutationOccurence == mutation.Occurence && s.LastMutationId >= mutation.Id)
                .ToList();

            foreach (var invalidCache in invalidCaches)
                _store.Remove(invalidCache);

            await _store.SaveChangesAsync(ct);
            
            eventsIngested = true;
        }
        
        return eventsIngested;
    }
    
    private Task RebuildSnapshots(CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        foreach (var wantedSnapshotAge in WantedSnapshotAges.OrderDescending())
        {
            var targetDate = now - wantedSnapshotAge;

            var bestParent = _store.CachedSnapshots
                .Where(s => s.LastMutationOccurence < targetDate)
                .MaxBy(v => v.LastMutationOccurence?.Occurence);

            if (bestParent is null)
                bestParent = Snapshot.Create(wantedSnapshotAge);

            // if parent snapshot is not old self create a copy
            if (bestParent.TargetAge != wantedSnapshotAge)
                bestParent = Snapshot.Clone(wantedSnapshotAge, bestParent);

            // apply remaining mutations on top of it
            var remainingMutations = _mutations
                .Where(m => m.Key > bestParent.LastMutationOccurence)
                .Where(m => m.Key.Occurence <= targetDate);

            foreach (var (occurence, mutation) in remainingMutations)
                bestParent.AppendMutation(mutation);


            var snapshotToReplace = _snapshotCaches.SingleOrDefault(s => s.TargetAge == wantedSnapshotAge);
            if (snapshotToReplace is not null)
                _snapshotCaches.Remove(snapshotToReplace);

            _snapshotCaches.Add(bestParent);

            if (wantedSnapshotAge == TimeSpan.Zero)
                _newSnapshot.Enqueue(bestParent);
        }
    }
}