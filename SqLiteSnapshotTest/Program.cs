using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SqLiteSnapshotTest;

var sw = Stopwatch.StartNew();

var folder = "./databases";
var liveDbPath = Path.Join(folder, "live.db");
var optionsBuilder = new DbContextOptionsBuilder<TestContext>()
    .UseSqlite($"Data Source={liveDbPath};Pooling=false");

Directory.Delete(folder, recursive: true);
Directory.CreateDirectory(folder);

{
    await using var store = new TestContext(optionsBuilder.Options);
    await store.Database.EnsureCreatedAsync();
    Console.WriteLine("Init: " + sw.ElapsedMilliseconds);
    sw.Restart();
}


await WriteStuff();
CreateSnapshot("snap1");
await WriteStuff();
CreateSnapshot("snap2");
await WriteStuff();

await DumpContents();
RestoreSnapshot("snap1");
await DumpContents();
RestoreSnapshot("snap2");
await DumpContents();

DeleteSnapshot("snap1");
DeleteSnapshot("snap2");

void CreateSnapshot(string name)
{
    //var sw = Stopwatch.StartNew();
    //SqliteConnection.ClearAllPools();

    var path = Path.Join(folder, $"{name}.db");
    File.Copy(liveDbPath, path);
    Console.WriteLine($"Created Snapshot {name} in {sw.ElapsedMilliseconds}ms");
}

void RestoreSnapshot(string name)
{
    //var sw = Stopwatch.StartNew();
    //SqliteConnection.ClearAllPools();

    var path = Path.Join(folder, $"{name}.db");
    File.Copy(path, liveDbPath, overwrite: true);
    Console.WriteLine($"Restored Snapshot {name} in {sw.ElapsedMilliseconds}ms");
}

void DeleteSnapshot(string name)
{    
    //var sw = Stopwatch.StartNew();
    //SqliteConnection.ClearAllPools();

    var path = Path.Join(folder, $"{name}.db");
    File.Delete(path);
    Console.WriteLine($"Deleted Snapshot {name} in {sw.ElapsedMilliseconds}ms");
}

async Task WriteStuff()
{
    var sw = Stopwatch.StartNew();
    await using var store = new TestContext(optionsBuilder.Options);
    const int numEntries = 100_000;
    for (var i = 0; i < numEntries; i++)
    {
        store.Todos.Add(new Todo()
        {
            Id = Guid.NewGuid(),
            Text = "Todo 1"
        });
    }
    await store.SaveChangesAsync();
    Console.WriteLine($"Wrote {numEntries} in {sw.ElapsedMilliseconds}ms");
}

async Task DumpContents()
{
    await using var store = new TestContext(optionsBuilder.Options);
    Console.WriteLine(await store.Todos.CountAsync());
}