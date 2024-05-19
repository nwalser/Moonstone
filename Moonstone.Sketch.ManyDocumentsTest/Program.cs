using System.Diagnostics;
using Amber.Domain.Documents.Project;
using Amber.Domain.Documents.Todo;
using Moonstone;
using ChangeName = Amber.Domain.Documents.Todo.ChangeName;


var tempPath = "C:\\Users\\Nathaniel Walser\\Desktop\\test";
if(Directory.Exists(tempPath)) Directory.Delete(tempPath, recursive: true);
Directory.CreateDirectory(tempPath);
var workspace = new Workspace(tempPath, new Dictionary<int, Type>
{
    { 0, typeof(ProjectAggregate) },
    { 1, typeof(TodoAggregate) }
});

var todoReader = new Reader<TodoAggregate>(tempPath, "session1", new TodoHandler());

var sw = Stopwatch.StartNew();

for (var i = 0; i < 100; i++)
{
    var identity = workspace.Create<TodoAggregate>();

    for (var j = 0; j < 100; j++)
    {
        todoReader.AppendMutation(identity, new ChangeName($"Name {j}"));
    }
}

Console.WriteLine("Create: " + sw.ElapsedMilliseconds);
sw.Restart();

// read back from fs
var documents = workspace.EnumerateDocuments();

foreach (var identity in documents)
{
    var todoAggregate = todoReader.Read(identity);
    Console.WriteLine(todoAggregate.Name);
}

Console.WriteLine("Read: " + sw.ElapsedMilliseconds);
