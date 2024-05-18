﻿using System.Diagnostics;
using Amber.Domain.Documents.Todo;
using Moonstone;


var tempPath = "C:\\Users\\Nathaniel Walser\\Desktop\\test";
//Directory.Delete(tempPath, recursive: true);
var workspace = await Workspace.Open(tempPath, "session1", [new TodoHandler()]);

var sw = Stopwatch.StartNew();

for (var i = 0; i < 100; i++)
{
    var id = Guid.NewGuid();
    await workspace.Create<TodoAggregate>(id);

    for (var j = 0; j < 10; j++)
    {
        await workspace.ApplyMutation<TodoAggregate>(id, new ChangeName($"Name {j}"));
    }
}

Console.WriteLine("Create: " + sw.ElapsedMilliseconds);
sw.Restart();

// read back from fs
var files = Directory.EnumerateFiles(tempPath, "", SearchOption.AllDirectories);

foreach (var file in files)
{
    var stream = File.OpenText(file);
    while (!stream.EndOfStream) stream.ReadLine();
}

Console.WriteLine("Read: " + sw.ElapsedMilliseconds);