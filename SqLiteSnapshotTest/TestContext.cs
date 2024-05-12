using Microsoft.EntityFrameworkCore;

namespace SqLiteSnapshotTest;

public class TestContext(DbContextOptions<TestContext> options) : DbContext(options)
{
    public DbSet<Todo> Todos { get; set; }
}