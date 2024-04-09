﻿using System.Collections.Concurrent;
using System.Diagnostics;
using DistributedSessions;
using DistributedSessions.Mutations;
using DistributedSessions.Projection;

var temp = @"C:\Users\NathanielWalser\Documents\Repositories\Moonstone\DistributedSessions\Temp";
var workspace = @"C:\Users\NathanielWalser\OneDrive - esp-engineering gmbh\Moonstone\workspace3"; 
var sessionId = Guid.Parse("040461cf-f8cb-4bcb-9352-1edeb67c5d9a");

var sw = Stopwatch.StartNew();

var cts = new CancellationTokenSource();

var writeMutation = new ConcurrentQueue<Mutation>();
var newMutations = new ConcurrentQueue<Mutation>();
var newStreamMutations = new ConcurrentQueue<Mutation>();
var newSnapshots = new ConcurrentQueue<Snapshot>();

var writer = new MutationWriter(workspace, sessionId, writeMutation);
var reader = new MutationReader(newMutations, workspace);
var stream = new MutationStream([], new SortedList<DateTime, Mutation>(), newMutations, newStreamMutations, new Dictionary<TimeSpan, Snapshot>(), newSnapshots);

var writerTask = writer.ExecuteAsync(cts.Token);
var readerTask =  reader.ExecuteAsync(cts.Token);
var streamTask =  stream.ExecuteAsync(cts.Token);

for (var i = 0; i < 1_000; i++)
{
    writeMutation.Enqueue(new CreateProjectMutation()
    {
        MutationId = Guid.NewGuid(),
        Occurence = DateTime.UtcNow,
        ProjectId = Guid.NewGuid(),
        Name = "Project 1",
    });
    
    //Console.WriteLine(i);
}

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


Console.WriteLine("Enqueuing: " + sw.ElapsedMilliseconds);
sw.Restart();

while(newMutations.Count > 0){}


Console.ReadKey();
cts.Cancel();