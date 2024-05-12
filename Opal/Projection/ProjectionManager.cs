using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Opal.Cache;
using Opal.Mutations;

namespace Opal.Projection;

public class ProjectionManager<TProjection> where TProjection : IProjection, new()
{
    private readonly CacheContext _store;
    private readonly ILogger<ProjectionManager<TProjection>> _logger;
    private readonly List<Region> _snapshotRegions;

    
    public ProjectionManager(CacheContext store, ILogger<ProjectionManager<TProjection>> logger, List<Region> snapshotRegions)
    {
        _store = store;
        _logger = logger;
        _snapshotRegions = snapshotRegions;
    }


    public async Task Initialize()
    {
        //_store.Database.ExecuteSql() (TransactionalBehavior.DoNotEnsureTransaction, "VACUUM;");
        await UpdateSnapshotCaches();
    }

    private async Task InvalidateSnapshotCaches(CancellationToken ct = default)
    {
        var oldestUnknownMutation = await _store.Mutations
            .Where(m => m.CacheInvalidated == false)
            .OrderBy(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken: ct);

        if (oldestUnknownMutation is null)
            return;
            
        await _store.Snapshots
            .Where(s => s.Id >= oldestUnknownMutation.Id)
            .ExecuteDeleteAsync(ct);
        
        await _store.Mutations
            .Where(m => m.CacheInvalidated == false)
            .ExecuteUpdateAsync(u => u.SetProperty(p => p.CacheInvalidated, true), cancellationToken: ct);
        
        await _store.SaveChangesAsync(ct);
    }

    public async Task UpdateSnapshotCaches(CancellationToken ct = default)
    {
        await InvalidateSnapshotCaches(ct);
        
        var totalEvents = await _store.Mutations.CountAsync(cancellationToken: ct);

        // rebuild regions with no valid snapshot
        {
            var existingSnapshots = await _store.Snapshots
                .Select(s => new { s.Id, s.LastMutationId, s.NumberOfAppliedMutations })
                .ToListAsync(cancellationToken: ct);

            var regionsWithNoSnapshot = _snapshotRegions
                .Where(r => !existingSnapshots
                    .Where(s => s.NumberOfAppliedMutations >= totalEvents - r.MaxAge)
                    .Any(s => s.NumberOfAppliedMutations <= totalEvents - r.MinAge))
                .ToList();
            
            foreach (var regionWithNoSnapshot in regionsWithNoSnapshot.OrderByDescending(r => r.MinAge))
            {
                var regionMaximumMutations = totalEvents - regionWithNoSnapshot.MinAge;
                var snapshot = await _store.Snapshots
                    .Where(s => s.NumberOfAppliedMutations < regionMaximumMutations)
                    .OrderByDescending(s => s.NumberOfAppliedMutations)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken: ct);
                
                // if no parent exists create new empty snapshot
                snapshot ??= CreateInitial();
                snapshot.NewId();
                var projection = JsonSerializer.Deserialize<TProjection>(snapshot.Projection) ?? throw new InvalidOperationException();

                // update projection
                var remainingMutations = _store.Mutations
                    .Where(m => m.Id > snapshot.LastMutationId)
                    .OrderBy(m => m.Id)
                    .AsNoTracking()
                    .AsAsyncEnumerable();

                await foreach (var remainingMutation in remainingMutations)
                {
                    var mutationEnvelope = remainingMutation.ToMutationEnvelope<MutationBase>();
                    snapshot.ApplyMutation(mutationEnvelope);
                    projection.ApplyMutation(mutationEnvelope.Mutation);

                    if (snapshot.NumberOfAppliedMutations >= regionMaximumMutations)
                        break;
                }

                snapshot.Projection = JsonSerializer.SerializeToUtf8Bytes(projection);

                await _store.AddAsync(snapshot, ct);
                await _store.SaveChangesAsync(ct);
                
                _logger.LogInformation("Rebuilt Projection for Region ({MinAge}, {MaxAge}) with number of mutations {NumberOfMutations}", regionWithNoSnapshot.MinAge, regionWithNoSnapshot.MaxAge, snapshot.NumberOfAppliedMutations);
            }
        }
        
        // remove snapshots that do not belong to a region
        {
            var existingSnapshots = await _store.Snapshots
                .Select(s => new { s.Id, s.NumberOfAppliedMutations })
                .ToListAsync(cancellationToken: ct);

            var keepingSnapshotIds = _snapshotRegions
                .Select(r => existingSnapshots
                    .Where(s => s.NumberOfAppliedMutations >= totalEvents - r.MaxAge)
                    .Where(s => s.NumberOfAppliedMutations <= totalEvents - r.MinAge)
                    .MaxBy(s => s.NumberOfAppliedMutations))
                .Select(s => s?.Id)
                .ToList();
            
            // delete all other snapshots from database
            var snapshotsToDelete = existingSnapshots
                .Select(s => s.Id)
                .Where(id => keepingSnapshotIds.All(kid => kid != id))
                .ToList();
            
            await _store.Snapshots
                .Where(s => snapshotsToDelete.Contains(s.Id))
                .ExecuteDeleteAsync(ct);
        }
    }
}