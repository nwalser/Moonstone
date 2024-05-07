using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Opal.Cache;
using RT.Comb;

namespace Opal.Stream;

public class SnapshotManager<TProjection> where TProjection : IProjection
{
    private readonly CacheContext _store;
    private readonly ILogger<SnapshotManager<TProjection>> _logger;

    private readonly List<(int min, int max)> _wantedSnapshotAges;

    
    public SnapshotManager(CacheContext store, ILogger<SnapshotManager<TProjection>> logger, List<(int min, int max)> wantedSnapshotAges)
    {
        _store = store;
        _logger = logger;
        _wantedSnapshotAges = wantedSnapshotAges;
    }


    public async Task Initialize()
    {
        await UpdateProjections();
    }

    private async Task InvalidateCaches(CancellationToken ct = default)
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
    
    public async Task UpdateProjections(CancellationToken ct = default)
    {
        await InvalidateCaches(ct);
        
        // rebuild projections
        foreach (var wantedSnapshotAge in _wantedSnapshotAges)
        {
            
            
        }
    }
}