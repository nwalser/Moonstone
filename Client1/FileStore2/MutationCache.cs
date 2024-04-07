using Microsoft.EntityFrameworkCore;

namespace Client1;

public class MutationCache : DbContext
{
    public DbSet<CachedMutation> CachedMutations { get; set; }
}