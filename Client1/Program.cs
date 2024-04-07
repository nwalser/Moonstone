using System.Diagnostics;
using System.Reactive.Subjects;
using Microsoft.EntityFrameworkCore;
using Stream.FileStore;
using Stream.Mutations;

var localEventsFolder = Directory.CreateTempSubdirectory();
var localCache = Path.Join(localEventsFolder.FullName, "app.db");
var eventFolder = @"C:\Users\Nathaniel Walser\OneDrive - esp-engineering gmbh\Moonstone\workspace1";
var session = "04991280-139f-43a9-999c-ca75acf87f83";

var externalMutation = new Subject<Mutation>();
var persistMutation = new Subject<Mutation>();
var mutationCached = new Subject<Mutation>();


var optionsBuilder = new DbContextOptionsBuilder<MutationStreamStore>().UseSqlite($"Data Source={localCache}");
var mutationCache = new MutationStreamStore(optionsBuilder.Options);

mutationCache.Database.EnsureCreated();

var fileStoreManager = new FileStoreManager(externalMutation, eventFolder, session, persistMutation);
var mutationStream = new MutationStream(externalMutation, persistMutation, mutationCached, mutationCache);
var sw2 = Stopwatch.StartNew();

await fileStoreManager.ActivateAsync();
Console.WriteLine(sw2.ElapsedMilliseconds);

var sw = Stopwatch.StartNew();

//mutationStream.AddMutation(new CreateProjectMutation()
//{
//    MutationId = Guid.NewGuid(),
//    Occurence = DateTime.UtcNow,
//    ProjectId = Guid.NewGuid(),
//    Name = "Project 1",
//});
//
//
//for (int i = 0; i < 1000; i++)
//{
//    mutationStream.AddMutation(new CreateProjectMutation()
//    {
//        MutationId = Guid.NewGuid(),
//        Occurence = DateTime.UtcNow,
//        ProjectId = Guid.NewGuid(),
//        Name = "Project 2",
//    });
//}

Console.WriteLine(sw.ElapsedMilliseconds);

Console.ReadKey();