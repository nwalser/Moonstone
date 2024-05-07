using Microsoft.EntityFrameworkCore;

namespace Opal.Cache;

public class CacheContext(DbContextOptions<CacheContext> options) : DbContext(options)
{
    public DbSet<Mutation> Mutations { get; set; }
    public DbSet<FilePointer> FilePointers { get; set; }
    public DbSet<Snapshot> Snapshots { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Mutation>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.CacheInvalidated);
        });
        
        builder.Entity<FilePointer>(e =>
        {
            e.HasKey(u => u.FileId);
        });
    }
}