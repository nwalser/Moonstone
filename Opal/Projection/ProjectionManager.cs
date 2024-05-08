using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Opal.Cache;

namespace Opal.Projection;

public class ProjectionManager<TProjection> where TProjection : IProjection
{
    private readonly CacheContext _store;
    private readonly ILogger<ProjectionManager<TProjection>> _logger;

    private readonly List<(int min, int max)> _wantedSnapshotAges;

    
    public ProjectionManager(CacheContext store, ILogger<ProjectionManager<TProjection>> logger, List<(int min, int max)> wantedSnapshotAges)
    {
        _store = store;
        _logger = logger;
        _wantedSnapshotAges = wantedSnapshotAges;
    }


    public async Task Initialize()
    {
        await UpdateProjections();
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

    public async Task RebuildLiveProjection()
    {
        
    }
    
    public async Task UpdateProjections(CancellationToken ct = default)
    {
        await InvalidateSnapshotCaches(ct);
        
        // rebuild projections
        foreach (var wantedSnapshotAge in _wantedSnapshotAges)
        {
            //var parentLastMutation = _store.Snapshots.Where(s => s.)
            
            
            // load from parent
            // load default values
            // delete other entries in database
            // save
            
            
        }
    }
}