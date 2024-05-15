using System.Diagnostics;
using Amber;
using Amber.Sapphire.Documents.Project;
using Amber.Sapphire.Documents.Project.Mutations;

var sw = Stopwatch.StartNew();

var folder = "C:\\Users\\Nathaniel Walser\\OneDrive - esp-engineering gmbh\\Moonstone\\workspace6";
var session = "794dcb19-a00e-4f5a-9eeb-5a2d3b582f60";
if(Directory.Exists(folder))
    Workspace.Delete(folder);

var workspace = Workspace.Create(folder, session, [new ProjectHandler()]);

LogStage("Init", sw);

var project = await workspace.CreateDocument<Project>();
LogStage("Create Project", sw);

project.ValueObservable.Subscribe(d => Console.WriteLine("Got: " + d));
    
await project.ApplyMutation(new ChangeProjectName()
{
    Name = "Project 1",
});
LogStage("Apply single mutation", sw);

for (var i = 0; i < 10; i++)
{
    await project.ApplyMutation(new IncreaseCounter()
    {
        Count = 1,
    });
    var proj = (Project)project.Value;
    Console.WriteLine($"{i}: {proj.Counter}");
}

LogStage("Apply 100 mutations", sw);


LogStage("Enumerate", sw);

Console.ReadKey();

void LogStage(string name, Stopwatch sw)
{
    Console.WriteLine($"{name}: " + sw.ElapsedMilliseconds);
    sw.Restart();
}