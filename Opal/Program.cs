using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Opal.Cache;
using Opal.Log;
using Opal.Mutations;

var sw = Stopwatch.StartNew();

var workspacePath = @"C:\Users\Nathaniel Walser\OneDrive - esp-engineering gmbh\Moonstone\workspace4";
var sessionId = Guid.Parse("794dcb19-a00e-4f5a-9eeb-5a2d3b582f60");
var cachePath = @"C:\Users\Nathaniel Walser\Desktop\temp\cache.db";
var mutationsPath = Path.Join(workspacePath, "mutations");

Directory.CreateDirectory(mutationsPath);

var optionsBuilder = new DbContextOptionsBuilder<CacheContext>()
    .UseSqlite($"Data Source={cachePath}");
var store = new CacheContext(optionsBuilder.Options);
await store.Database.EnsureCreatedAsync();

Console.WriteLine("Init: " + sw.ElapsedMilliseconds);
sw.Restart();

var streamSync = new StreamSync(mutationsPath, store);
await streamSync.Initialize();

Console.WriteLine("Sync: " + sw.ElapsedMilliseconds);
sw.Restart();

var streamWriter = RollingMutationWriter.InitializeFrom(mutationsPath, sessionId);

for (var i = 0; i < 100; i++)
{
    streamWriter.Append(new MutationEnvelope<MutationBase>()
    {
        Id = Guid.NewGuid(),
        Mutation = new CreateTask()
        {
            Id = Guid.NewGuid(),
            Name = "Task 1"
        }
    });
}

Console.WriteLine("Write: " + sw.ElapsedMilliseconds);
sw.Restart();

await streamSync.ProcessChangedFiles();

Console.WriteLine("Ingest: " + sw.ElapsedMilliseconds);
sw.Restart();

Console.WriteLine("Done");