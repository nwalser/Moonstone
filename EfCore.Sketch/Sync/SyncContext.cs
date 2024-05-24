using EfCore.Sketch.Sync.Deltas;
using Microsoft.EntityFrameworkCore;

namespace EfCore.Sketch.Sync;

public class SyncContext(DbContextOptions<SyncContext> options) : DbContext(options)
{
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        var entries = ChangeTracker.Entries();

        var deltas = new List<object>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    deltas.Add(new Created()
                    {
                        Ticks = DateTime.UtcNow.Ticks,
                        Type = entry.Metadata.ClrType,
                        Id = (Guid)entry.Property("Id").CurrentValue!,
                    });

                    foreach (var property in entry.Properties)
                    {
                        deltas.Add(new Changed()
                        {
                            Ticks = DateTime.UtcNow.Ticks,

                            Type = entry.Metadata.ClrType,
                            Id = (Guid)entry.Property("Id").CurrentValue!,
                            
                            Field = property.Metadata.Name,
                            Value = property.CurrentValue,
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
                                Ticks = DateTime.UtcNow.Ticks,

                                Type = entry.Metadata.ClrType,
                                Id = (Guid)entry.Property("Id").CurrentValue!,
                            
                                Field = property.Metadata.Name,
                                Value = property.CurrentValue,
                            });
                        }
                    }
                    break;
                case EntityState.Deleted:
                    deltas.Add(new Deleted()
                    {
                        Ticks = DateTime.UtcNow.Ticks,

                        Type = entry.Metadata.ClrType,
                        Id = (Guid)entry.Property("Id").CurrentValue!,
                    });
                    break;
            }
        }

        foreach (var delta in deltas)
        {
            switch (delta)
            {
                case Created created:
                    Console.WriteLine($"Created: {created.Ticks} {created.Type} {created.Id}");
                    break;
                case Deleted deleted:
                    Console.WriteLine($"Deleted: {deleted.Ticks} {deleted.Type} {deleted.Id}");
                    break;
                case Changed changed:
                    Console.WriteLine($"Changed: {changed.Ticks} {changed.Type} {changed.Id} {changed.Field} {changed.Value}");
                    break;
            }
        }

        var result =  base.SaveChangesAsync(cancellationToken);
        
        return result;
    }

    
    private void OnSavedChanges(object? sender, SavedChangesEventArgs e)
    {
        Console.WriteLine("Changes saved");
    }
}