﻿using Microsoft.EntityFrameworkCore;
using Moonstone.Workspace.Data;

namespace Moonstone.Workspace.Stream;

public class StreamStore(DbContextOptions<StreamStore> options) : DbContext(options)
{
    public DbSet<CachedMutation> CachedMutations { get; set; } = null!;
    public DbSet<CachedSnapshot> CachedSnapshots { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<CachedMutation>(e =>
        {
            e.HasKey(u => u.MutationId);
            e.HasIndex(u => u.MutationId);
        });
        
        builder.Entity<CachedSnapshot>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.TargetAge).IsUnique();
        });
    }
}