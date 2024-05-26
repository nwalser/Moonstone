using System.Diagnostics;
using Opal;
using Opal.Domain;

var sw = Stopwatch.StartNew();

var path = @"C:\Users\Nathaniel Walser\OneDrive - esp-engineering gmbh\Moonstone\workspace16";
var session = "session1";

var typeMap = new Dictionary<int, Type>()
{
    { 0, typeof(Project) },
    { 1, typeof(Todo) },
};

var database = new Database(typeMap, session, path);
database.Open();

Console.WriteLine($"Startup: {sw.ElapsedMilliseconds}ms");
sw.Restart();

var projects = Enumerable
    .Range(0, 1)
    .Select(i => new Project()
    {
        Id = Guid.NewGuid(),
        Name = $"Project {i}",
        Name2 = $"Project {i}",
        Name3 = $"Project {i}",
        Name4 = $"Project {i}",
        Name5 = $"Project {i}",
        Name6 = $"Project {i}",
        Name7 = $"Project {i}",
        Name8 = $"Project {i}",
        Name9 = $"Project {i}",
    });


database.Update(projects);
Console.WriteLine($"Insert: {sw.ElapsedMilliseconds}ms");

await Task.Delay(1000);