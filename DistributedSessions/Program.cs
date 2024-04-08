﻿using System.Collections.Concurrent;
using System.Diagnostics;
using DistributedSessions;
using DistributedSessions.Mutations;

var temp = @"C:\Users\Nathaniel Walser\Documents\Repositories\Moonstone\DistributedSessions\Temp";
var workspace = @"C:\Users\Nathaniel Walser\OneDrive - esp-engineering gmbh\Moonstone\workspace3"; 
var sessionId = Guid.Parse("040461cf-f8cb-4bcb-9352-1edeb67c5d9a");

var sw = Stopwatch.StartNew();

var cts = new CancellationTokenSource();

var writeMutation = new ConcurrentQueue<Mutation>();
var newMutations = new ConcurrentQueue<Mutation>();

var writer = new MutationWriter(workspace, sessionId, writeMutation);
var reader = new MutationReader(newMutations, workspace);

var writerTask = writer.ExecuteAsync(cts.Token);
var fileStoreTask =  reader.ExecuteAsync(cts.Token);





for (var i = 0; i < 100; i++)
{
    writeMutation.Enqueue(new CreateProjectMutation()
    {
        MutationId = Guid.NewGuid(),
        Occurence = DateTime.UtcNow,
        ProjectId = Guid.NewGuid(),
        Name = "Project 1",
    });
    
    Console.WriteLine(i);
}

Console.ReadKey();
cts.Cancel();