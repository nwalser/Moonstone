using System.Diagnostics;
using MsgPack.Serialization;

var knownIds = @"C:\Users\Nathaniel Walser\Documents\Respositories\Moonstone\DistributedSessions\Temp\knownIds.bin";
var workspace = @"C:\Users\Nathaniel Walser\OneDrive - esp-engineering gmbh\Moonstone\workspace2"; 
var session = "nathaniel-desktop";
var serializer = MessagePackSerializer.Get<List<Guid>>();
var sw = Stopwatch.StartNew();


// workspace2
// - session1
// -- log1
// - session2
// -- log1
// -- log2
// -- log3 // multiple 

// temp
// - knownFiles

// ---- recipe
// find all files in all directories
// subscribe to all changes and creations of files -> rescan those
// rescan all the ones not already read
// rescan the newest one per session directory


//Directory.Delete(workspace, recursive: true);
Directory.CreateDirectory(workspace);

// unpack from file
var ids = Read();

Console.WriteLine("Read Ids From File " + sw.ElapsedMilliseconds);
sw.Restart();

for (var i = 0; i < 1_000; i++)
{
    var id = Guid.NewGuid();
    File.WriteAllText(Path.Join(workspace, id.ToString()), "Hello world");
    ids.Add(id);
}

Console.WriteLine("Writing Files " + sw.ElapsedMilliseconds);
sw.Restart();

var files = Directory.EnumerateFiles(workspace);
foreach (var file in files)
{
    var fileName = Path.GetFileName(file);
    var id = Guid.Parse(fileName);
    var inSet = ids.Contains(id);
}

Console.WriteLine("Comparing with DB " + sw.ElapsedMilliseconds);
sw.Restart();

// pack into file
Write(ids);

Console.WriteLine("Store ids to file " + sw.ElapsedMilliseconds);
sw.Restart();

Console.ReadKey();
return;

HashSet<Guid> Read()
{
    if (!File.Exists(knownIds))
        return [];
    
    using var stream = File.OpenRead(knownIds);
    var eventIds = serializer.Unpack(stream);
    return eventIds.ToHashSet();
}

void Write(HashSet<Guid> ids)
{
    File.Delete(knownIds);
    using var stream = File.Create(knownIds);
    serializer.Pack(stream, ids.ToList());
    stream.Flush();
}