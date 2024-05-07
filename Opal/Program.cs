using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Opal;
using Opal.Cache;
using Opal.Mutations;
using Opal.Stream;
using RT.Comb;

var sw = Stopwatch.StartNew();

var workspacePath = @"C:\Users\Nathaniel Walser\OneDrive - esp-engineering gmbh\Moonstone\workspace4";
var sessionId = Guid.Parse("794dcb19-a00e-4f5a-9eeb-5a2d3b582f60");
var cachePath = @"C:\Users\Nathaniel Walser\Desktop\temp\cache.db";
var mutationsPath = Path.Join(workspacePath, "mutations");

var comb = new PostgreSqlCombProvider(new SqlDateTimeStrategy());

Directory.CreateDirectory(mutationsPath);

var optionsBuilder = new DbContextOptionsBuilder<CacheContext>()
    .UseSqlite($"Data Source={cachePath}");
var store = new CacheContext(optionsBuilder.Options);
await store.Database.EnsureCreatedAsync();

var snapshotManager = new SnapshotManager<Projection>(store, new Logger<SnapshotManager<Projection>>(new LoggerFactory()),
    [
        (0, 0),
        (10, 100),
        (100, 1000),
        (1000, 10000)
    ]);
await snapshotManager.Initialize();

Console.WriteLine("Init: " + sw.ElapsedMilliseconds);
sw.Restart();

var mutationSync = new MutationSync<MutationBase>(mutationsPath, store);
mutationSync.Initialize();

Console.WriteLine("Sync: " + sw.ElapsedMilliseconds);
sw.Restart();

var mutationWriter = new MutationWriter<MutationBase>(mutationsPath, sessionId);
mutationWriter.Initialize();


for (var i = 0; i < 0; i++)
{
    mutationWriter.Append(new MutationEnvelope<MutationBase>()
    {
        Id = comb.Create(),
        Mutation = new CreateTask()
        {
            Id = Guid.NewGuid(),
            Name = "Task 1"
        }
    });
}

await mutationWriter.ProcessWork();

Console.WriteLine("Write: " + sw.ElapsedMilliseconds);
sw.Restart();

await mutationSync.ProcessWork();

Console.WriteLine("Ingest: " + sw.ElapsedMilliseconds);
sw.Restart();

Console.WriteLine("Done");