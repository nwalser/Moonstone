// workspace
// -- contains many documents
// -- manages lazy loading of documents, implement lazy loading with json documents
// -- keeps log of deleted documents forever

// document
// -- has a type
// -- keeps log of mutations, also session based
// -- is rebuilt every time the mutation log changes
// -- can be deleted
// -- knows how to apply mutations to it

// special remarks
// -- do not list every available document
// -- allow cold fetching with type and guid
// create own store for every document type


using System.Diagnostics;
using Amber.Documents.Project;
using Amber.Documents.Project.Mutations;
using Amber.Ws;

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