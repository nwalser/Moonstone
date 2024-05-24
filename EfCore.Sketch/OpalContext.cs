using EfCore.Sketch.Sync;
using Microsoft.EntityFrameworkCore;

namespace EfCore.Sketch;

public class OpalContext(DbContextOptions<SyncContext> options) : SyncContext(options)
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<Todo> Todos { get; set; }
}