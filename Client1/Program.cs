using System.Reactive.Linq;
using Newtonsoft.Json;
using Stream;
using Stream.Mutations;
using Stream.Mutations.Project.CreateProject;

var localEventsFolder = Directory.CreateTempSubdirectory();
var eventFolder = @"C:\Users\Nathaniel Walser\OneDrive - esp-engineering gmbh\Moonstone\workspace1";

var fileSystemObservable = FileSystemWatcherObservable.Create(eventFolder);
var initialSyncObservable = InitialSyncObservable.Create(eventFolder);
var eventPathsObservable = fileSystemObservable.Merge(initialSyncObservable);




var mutationStream = new MutationStream()
{
    Mutations = []
};

eventPathsObservable.Subscribe(path =>
{
    var file = File.ReadAllText(path);
    var obj = JsonConvert.DeserializeObject<Mutation>(file, new JsonSerializerSettings(){ TypeNameHandling = TypeNameHandling.All });
    if (obj != null) 
        mutationStream.IngestMutation(obj);
});


var mutation1 = new CreateProjectMutation()
{
    MutationId = Guid.NewGuid(),
    ProjectId = Guid.NewGuid(),
    Name = "Project 1",
};

var mutation2 = new CreateProjectMutation()
{
    MutationId = Guid.NewGuid(),
    ProjectId = Guid.NewGuid(),
    Name = "Project 2",
};

void WriteMutation(Mutation mutation)
{
    var text = JsonConvert.SerializeObject(mutation, new JsonSerializerSettings(){ TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented});

    var eventPath = Path.Join(eventFolder, mutation.MutationId.ToString());
    if (!File.Exists(eventPath))
        File.WriteAllText(eventPath, text);
}

Console.WriteLine("Listening for events");
eventPathsObservable.Subscribe(e => Console.WriteLine(e));


WriteMutation(mutation1);
WriteMutation(mutation2);

Console.ReadKey();