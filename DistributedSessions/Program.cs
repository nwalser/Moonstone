using System.Collections.Concurrent;
using DistributedSessions;
using DistributedSessions.Mutations;
using DistributedSessions.Projection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

var cts = new CancellationTokenSource();

var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
var loggerFactory = new SerilogLoggerFactory(logger);

var paths = new PathProvider()
{
    Temporary = @"C:\Users\NathanielWalser\Desktop\temp",
    Session = "040461cf-f8cb-4bcb-9352-1edeb67c5d9a",
    Workspace = @"C:\Users\NathanielWalser\OneDrive - esp-engineering gmbh\Moonstone\workspace3",
};

var writeMutation = new ConcurrentQueue<Mutation>();
var newMutations = new ConcurrentQueue<Mutation>();
var newSnapshots = new ConcurrentQueue<Snapshot>();
    
var writer = new MutationWriter(writeMutation, paths, cts.Token, loggerFactory.CreateLogger<MutationWriter>());
var reader = new MutationReader(newMutations, paths, cts.Token, loggerFactory.CreateLogger<MutationReader>());
var stream = new MutationStream(newMutations, newSnapshots, paths, cts.Token, loggerFactory.CreateLogger<MutationStream>());


// tests ----------
var t1 = Task.Run(() =>
{
    while (true)
    {
        if (newSnapshots.TryDequeue(out var snapshot))
        {
            Console.WriteLine(snapshot.Model.CreatedProjects);
        }
    }
});

var t2 = Task.Run(() =>
{
    while (true)
    {
        Console.WriteLine($"Write: {writeMutation.Count} - Read: {newMutations.Count}");
        Thread.Sleep(1000);
    }
});


for (var i = 0; i < 100_000; i++)
{
    writeMutation.Enqueue(new CreateProjectMutation()
    {
        Id = Guid.NewGuid(),
        Occurence = DateTime.UtcNow,
        ProjectId = Guid.NewGuid(),
        Name = "Project 1",
    });
}

while (true)
{
    Console.WriteLine("Ready for Input");
    Console.ReadKey();
    writeMutation.Enqueue(new DeleteProjectMutation()
    {
        Id = Guid.NewGuid(),
        Occurence = DateTime.UtcNow,
        ProjectId = Guid.NewGuid(),
    });
} 

cts.Cancel();