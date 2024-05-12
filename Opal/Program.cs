using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Opal.Cache;
using Opal.Mutations;
using Opal.Projection;
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

var snapshotManager = new ProjectionManager<Projection>(store,
    new Logger<ProjectionManager<Projection>>(new LoggerFactory()),
    [
        new Region(100, 999),
        new Region(1000, 9999),
        new Region(10000, 99999)
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


for (var i = 0; i < 1; i++)
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

await snapshotManager.UpdateSnapshotCaches();

Console.WriteLine("Rebuild Projections: " + sw.ElapsedMilliseconds);
sw.Restart();

var snapshots = await store.Snapshots.ToListAsync();
foreach (var snapshot in snapshots)
{
    var projection = JsonSerializer.Deserialize<Projection>(snapshot.Projection);
    Console.WriteLine(projection.Counter);
}

Console.WriteLine("Done");