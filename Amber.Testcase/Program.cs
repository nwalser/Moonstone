using System.Diagnostics;
using Amber;
using Amber.Sapphire.Documents.Project;
using Amber.Sapphire.Documents.Project.Mutations;

var sw = Stopwatch.StartNew();

var folder = "C:\\Users\\Nathaniel Walser\\OneDrive - esp-engineering gmbh\\Moonstone\\workspace6";
var session = "794dcb19-a00e-4f5a-9eeb-5a2d3b582f60";
var doc1Id = Guid.Parse("794dcb19-a00e-4f5a-9eeb-5a2d3b582f62");
var seed = false;

if (seed)
{
    if(Directory.Exists(folder))
        Workspace.Delete(folder);
    
    var workspace1 = await Workspace.Create(folder, session, [new ProjectHandler()]);
    await workspace1.Create<Project>(doc1Id);
    await workspace1.Close();
}

var workspace = await Workspace.Open(folder, session, [new ProjectHandler()]);

LogStage("Init", sw);

LogStage("Create Project", sw);

workspace.Observe<Project>(doc1Id).Subscribe(d => Console.WriteLine("Got: " + d));

await workspace.ApplyMutation<Project>(doc1Id, new ChangeProjectName()
{
    Name = "Project 1",
});
LogStage("Apply single mutation", sw);

for (var i = 0; i < 10; i++)
{
    await workspace.ApplyMutation<Project>(doc1Id, new IncreaseCounter()
    {
        Count = 1,
    });
}

LogStage("Apply 100 mutations", sw);


LogStage("Enumerate", sw);

while (true)
{
    Console.ReadKey();
    
    await workspace.ApplyMutation<Project>(doc1Id, new IncreaseCounter()
    {
        Count = 1,
    });
}

void LogStage(string name, Stopwatch sw)
{
    Console.WriteLine($"{name}: " + sw.ElapsedMilliseconds);
    sw.Restart();
}