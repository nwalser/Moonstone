using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moonstone.Framework.Stream;
using RT.Comb;
using Serilog;

namespace Moonstone.Workspace;

public class SnapshotManager<TProjection> where TProjection : new()
{
    private readonly HashSet<Guid> _mutationIds;
    private readonly MutationHandler<TProjection> _handler;
    private readonly StreamStore _store;
    private bool _initialized;

    private static readonly List<TimeSpan> WantedSnapshotAges =
    [
        TimeSpan.Zero,
        TimeSpan.FromSeconds(10),
        TimeSpan.FromMinutes(10),
        TimeSpan.FromDays(7)
    ];
    
    public SnapshotManager(StreamStore store, MutationHandler<TProjection> handler)
    {
        _handler = handler;
        _store = store;

        _mutationIds = new HashSet<Guid>();
    }
    
    public async Task Initialize(CancellationToken ct)
    {
        // load existing mutation ids into memory
        var mutationIds = await _store.CachedMutations
            .Select(m => m.MutationId)
            .ToListAsync(cancellationToken: ct);

        foreach (var mutationId in mutationIds)
            _mutationIds.Add(mutationId);

        _initialized = true;
    }
    
    public async Task IngestMutations(IEnumerable<Mutation> mutations, CancellationToken ct = default)
    {
        if (!_initialized)
            throw new InvalidOperationException();
        
        Mutation? oldestUnprocessedMutation = default;

        // ingest new mutations
        foreach (var mutation in mutations)
        {
            if (!_mutationIds.Add(mutation.Id))
                continue;
    
            _store.CachedMutations.Add(CachedMutation.FromMutation(mutation));
    
            if (oldestUnprocessedMutation is null | mutation.Id < oldestUnprocessedMutation?.Id)
                oldestUnprocessedMutation = mutation;
        }

        // invalidate expired projection caches
        if (oldestUnprocessedMutation is not null)
        {
            var invalidCaches = _store.CachedSnapshots
                .Where(s => oldestUnprocessedMutation.Id < s.LastMutationId)
                .ToList();

            _store.RemoveRange(invalidCaches);
        
            foreach (var invalidCache in invalidCaches)
                Log.Logger.Information("Invalidated Snapshot of age {SnapshotAge}", invalidCache.TargetAge);
        }

        // store changes to database
        await _store.SaveChangesAsync(ct);
    }


    public async Task<TProjection> RebuildLiveProjection(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
    
    public async Task<TProjection> RebuildBackupProjections(CancellationToken ct = default)
    {
        if (!_initialized)
            throw new InvalidOperationException();
        
        var now = DateTime.UtcNow;

        foreach (var wantedSnapshotAge in WantedSnapshotAges.OrderDescending())
        {
            var pro = new PostgreSqlCombProvider(new SqlDateTimeStrategy());
            var targetDateGuid = pro.Create(now - wantedSnapshotAge);
            
            var bestParent = await _store.CachedSnapshots
                .Where(s => s.LastMutationId < targetDateGuid)
                .OrderByDescending(s => s.LastMutationId)
                .FirstOrDefaultAsync(cancellationToken: ct);

            var snapshot = bestParent != null ? CachedSnapshot.CopyFromCached<TProjection>(bestParent) : Snapshot<TProjection>.Create();
            
            // apply remaining mutations on top of it
            var remainingMutations = await _store.CachedMutations
                .Where(m => m.MutationId > snapshot.LastMutationId)
                .Where(m => m.MutationId <= targetDateGuid)
                .OrderBy(m => m.MutationId)
                .ToListAsync(cancellationToken: ct);
            
            foreach (var cachedMutation in remainingMutations)
                snapshot.AppendMutation(CachedMutation.ToMutation(cachedMutation), _handler);
            
            var snapshotToReplace = _store.CachedSnapshots
                .SingleOrDefault(s => s.TargetAge == wantedSnapshotAge);
            
            if (snapshotToReplace is not null)
                _store.CachedSnapshots.Remove(snapshotToReplace);

            _store.CachedSnapshots.Add(CachedSnapshot.ToCached(Guid.NewGuid(), wantedSnapshotAge, snapshot));

            Log.Logger.Information("Rebuilt Snapshot with target age of {TargetAge} with parent {ParentAge}", wantedSnapshotAge, bestParent?.TargetAge);
        } 

        await _store.SaveChangesAsync(ct);

        var liveCachedSnapshot = await _store.CachedSnapshots.SingleAsync(s => s.TargetAge == TimeSpan.Zero, cancellationToken: ct);
        
        return CachedSnapshot.CopyFromCached<TProjection>(liveCachedSnapshot).Model;
    }
}