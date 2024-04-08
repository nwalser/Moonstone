using System.Diagnostics;
using System.Reactive.Subjects;
using DistributedSessions;
using DistributedSessions.Mutations;

var temp = @"C:\Users\Nathaniel Walser\Documents\Repositories\Moonstone\DistributedSessions\Temp";
var workspace = @"C:\Users\Nathaniel Walser\OneDrive - esp-engineering gmbh\Moonstone\workspace3"; 
var sessionId = Guid.Parse("040461cf-f8cb-4bcb-9352-1edeb67c5d9a");

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

var externalMutationOccured = new Subject<Mutation>();

var writer = new MutationWriter(workspace, sessionId);
var fileStoreManager = new FileStoreManager(externalMutationOccured, workspace);

await writer.InitializeAsync();
await fileStoreManager.InitializeAsync();


for (var i = 0; i < 100_000; i++)
{
    await writer.StoreMutation(new CreateProjectMutation()
    {
        MutationId = Guid.NewGuid(),
        Occurence = DateTime.UtcNow,
        ProjectId = Guid.NewGuid(),
        Name = "Project 1",
    });
    
    Console.WriteLine(i);
}

Console.ReadKey();
