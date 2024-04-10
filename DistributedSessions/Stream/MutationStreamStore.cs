using Microsoft.EntityFrameworkCore;

namespace DistributedSessions.Stream;

public class MutationStreamStore(DbContextOptions<MutationStreamStore> options) : DbContext(options)
{
    public required DbSet<CachedMutation> CachedMutations { get; set; }
}