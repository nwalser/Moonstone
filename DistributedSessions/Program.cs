using System.Collections.Concurrent;
using System.Diagnostics;
using DistributedSessions;
using DistributedSessions.Mutations;
using DistributedSessions.Projection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
var loggerFactory = new SerilogLoggerFactory(logger);


var temp = @"C:\Users\NathanielWalser\Desktop\temp";
var workspace = @"C:\Users\NathanielWalser\OneDrive - esp-engineering gmbh\Moonstone\workspace3";
var sessionId = Guid.Parse("040461cf-f8cb-4bcb-9352-1edeb67c5d9a");

var sw = Stopwatch.StartNew();

var cts = new CancellationTokenSource();

var writeMutation = new ConcurrentQueue<Mutation>();
var newMutations = new ConcurrentQueue<Mutation>();
var newSnapshots = new ConcurrentQueue<Snapshot>();

var writer = new MutationWriter(workspace, sessionId, writeMutation, cts.Token, loggerFactory.CreateLogger<MutationWriter>());
var reader = new MutationReader(workspace, newMutations);
var stream = new MutationStream(Path.Join(temp, sessionId.ToString()), newMutations, newSnapshots);

// todo: implement background worker
var readerTask =  reader.ExecuteAsync(cts.Token);
var streamTask =  stream.ExecuteAsync(cts.Token);

Task.Run(() =>
{
    while (true)
    {
        if (newSnapshots.TryDequeue(out var snapshot))
        {
            Console.WriteLine(snapshot.Model.CreatedProjects);
        }
    }
});

Task.Run(() =>
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
        MutationId = Guid.NewGuid(),
        Occurence = new MutationOccurence()
        {
            Occurence = DateTime.UtcNow,
            RandomId = Guid.NewGuid(),
        },
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
        MutationId = Guid.NewGuid(),
        Occurence = new MutationOccurence()
        {
            Occurence = DateTime.UtcNow,
            RandomId = Guid.NewGuid(),
        },
        Id = Guid.NewGuid(),
    });
} 

cts.Cancel();