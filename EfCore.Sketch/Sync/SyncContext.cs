using EfCore.Sketch.Sync.Deltas;
using Microsoft.EntityFrameworkCore;

namespace EfCore.Sketch.Sync;

public class SyncContext : DbContext
{
    public SyncContext(DbContextOptions<SyncContext> options) : base(options)
    {
        
        
    }

    protected DbSet<LwwEntry> LwwEntries { get; set; }

    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        var entries = ChangeTracker.Entries();

        var deltas = new List<IDelta>();

        foreach (var entry in entries)
        {
            var ticks = DateTime.UtcNow.Ticks;
            var table = entry.Metadata.GetTableName() ?? throw new Exception();
            
            switch (entry.State)
            {
                case EntityState.Added:
                    foreach (var property in entry.Properties)
                    {
                        deltas.Add(new Changed()
                        {
                            Ticks = ticks,
                            Table = table,
                            Row = entry.Property("Id").CurrentValue.ToString(),
                            Column = property.Metadata.GetColumnName(),
                            Value = property.CurrentValue.ToString()
                        });
                    }

                    break;
                case EntityState.Modified:
                    foreach (var property in entry.Properties)
                    {
                        if (property.IsModified)
                        {
                            deltas.Add(new Changed()
                            {
                                Ticks = ticks,
                                Table = table,
                                Row = entry.Property("Id").CurrentValue.ToString(),
                                Column = property.Metadata.GetColumnName(),
                                Value = property.CurrentValue.ToString()
                            });
                        }
                    }
                    break;
            }
        }

        foreach (var delta in deltas)
        {
            switch (delta)
            {
                case Changed changed:
                    Console.WriteLine($"Changed: {changed.Ticks} {changed.Table} {changed.Row} {changed.Column} {changed.Value}");
                    break;
            }
        }
        
        // save to database
        foreach (var delta in deltas)
            IngestChange(delta);
        
        // save to file
        
        // reset change tracker
        ChangeTracker.AcceptAllChanges();
        return Task.FromResult(0);
    }


    private void IngestChange(IDelta change)
    {
        switch (change)
        {
            case Changed changed:
            {
                var sql1 = $"SELECT CASE WHEN EXISTS (SELECT 1 FROM {changed.Table} WHERE Id = @p0) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END";
                var exists = Database.SqlQueryRaw<bool>(sql1, changed.Row).AsEnumerable().FirstOrDefault();
                
                
                if (!exists)
                {
                    var sql2 = $"INSERT INTO {changed.Table} (Id) " +
                               $"VALUES (@p0)";
                    Database.ExecuteSqlRaw(sql2, changed.Row);
                }
                
                var sql3 = $"UPDATE {changed.Table} " +
                           $"SET {changed.Column} = {changed.Value} " +
                           $"WHERE Id = @p0";
                Database.ExecuteSqlRaw(sql3, changed.Row);
                
                break;
            }
        }
    }
    
    private void OnSavedChanges(object? sender, SavedChangesEventArgs e)
    {
        Console.WriteLine("Changes saved");
    }
}