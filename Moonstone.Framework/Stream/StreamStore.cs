using Microsoft.EntityFrameworkCore;

namespace Moonstone.Framework.Stream;

public class StreamStore(DbContextOptions<StreamStore> options) : DbContext(options)
{
    public DbSet<CachedMutation> CachedMutations { get; set; } = null!;
    public DbSet<CachedSnapshot> CachedSnapshots { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<CachedMutation>(e =>
        {
            e.HasKey(u => u.MutationId);
        });
    }
}