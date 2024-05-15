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
using Amber.Mutation;

var workspace = "C:\\Users\\Nathaniel Walser\\OneDrive - esp-engineering gmbh\\Moonstone\\workspace5";
var session = "794dcb19-a00e-4f5a-9eeb-5a2d3b582f60";

var pathDoc1 = Path.Join(workspace, "project", "doc1");
var pathDoc2 = Path.Join(workspace, "project", "doc2");

Directory.Delete(pathDoc1);

var sw = Stopwatch.StartNew();

var documentStore = DocumentFileStore<Project>.Create(pathDoc1, session, new ProjectHandler());

LogStage("Create", sw);

var fsw = new FileSystemWatcher()


await documentStore.Append(pathDoc1, new ChangeProjectName()
{
    Name = "Project 1",
});

LogStage("Append One", sw);

for (var i = 0; i < 10_000; i++)
{
    await documentStore.Append(pathDoc1, new IncreaseCounter()
    {
        Count = 1,
    });
    await Task.Delay(TimeSpan.FromMilliseconds(0.01));
}

LogStage("Append 10k", sw);

var document = await documentStore.Read(pathDoc1);

LogStage("Read 10k", sw);

Console.WriteLine(document);



void LogStage(string name, Stopwatch sw)
{
    Console.WriteLine($"{name}: " + sw.ElapsedMilliseconds);
    sw.Restart();
}