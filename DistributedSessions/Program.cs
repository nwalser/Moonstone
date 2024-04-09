﻿using System.Collections.Concurrent;
using System.Diagnostics;
using DistributedSessions;
using DistributedSessions.Mutations;
using DistributedSessions.Projection;

var temp = @"C:\Users\Nathaniel Walser\Documents\Repositories\Moonstone\DistributedSessions\Temp";
var workspace = @"C:\Users\Nathaniel Walser\OneDrive - esp-engineering gmbh\Moonstone\workspace3"; 
var sessionId = Guid.Parse("040461cf-f8cb-4bcb-9352-1edeb67c5d9a");

var sw = Stopwatch.StartNew();

var cts = new CancellationTokenSource();

var writeMutation = new ConcurrentQueue<Mutation>();
var newMutations = new ConcurrentQueue<Mutation>();
var newSnapshots = new ConcurrentQueue<Snapshot>();

var writer = new MutationWriter(workspace, sessionId, writeMutation);
var reader = new MutationReader(newMutations, workspace);
var stream = new MutationStream([], new SortedList<MutationOccurence, Mutation>(), newMutations, newSnapshots, []);

var writerTask = writer.ExecuteAsync(cts.Token);
var readerTask =  reader.ExecuteAsync(cts.Token);
var streamTask =  stream.ExecuteAsync(cts.Token);


var mut1 = new MutationOccurence()
{
    Occurence = new DateTime(2020, 01, 01),
    RandomId = Guid.NewGuid(),
};

var mut2 = new MutationOccurence()
{
    Occurence = new DateTime(2020, 01, 01),
    RandomId = Guid.NewGuid(),
};


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


for (var i = 0; i < 1_000; i++)
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

await Task.WhenAll(writerTask, readerTask, streamTask);
cts.Cancel();