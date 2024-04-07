using Microsoft.EntityFrameworkCore;
using Stream.Mutations;

namespace Stream.FileStore;

public class MutationStreamStore(DbContextOptions<MutationStreamStore> options) : DbContext(options)
{
    public DbSet<CachedMutation> CachedMutations { get; set; }
}