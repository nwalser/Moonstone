using DistributedSessions.Projection;
using Microsoft.EntityFrameworkCore;

namespace DistributedSessions.Stream;

public class StreamStore(DbContextOptions<StreamStore> options) : DbContext(options)
{
    public DbSet<CachedMutation> CachedMutations { get; set; } = null!;
    public DbSet<CachedSnapshot> CachedSnapshots { get; set; } = null!;
}