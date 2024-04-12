using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Moonstone.Domain.Mutations.Project.ChangeName;
using Moonstone.Domain.Mutations.Project.Create;
using Moonstone.Domain.Mutations.Project.Delete;
using Moonstone.Framework;
using Moonstone.Framework.Stream;
using Serilog;
using Serilog.Extensions.Logging;

var cts = new CancellationTokenSource();

var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
var loggerFactory = new SerilogLoggerFactory(logger);

var paths = new PathProvider()
{
    Temporary = @"C:\Users\NathanielWalser\Desktop\temp",
    Session = "040461cf-f8cb-4bcb-9352-1edeb67c5d9c",
    Workspace = @"C:\Users\NathanielWalser\OneDrive - esp-engineering gmbh\Moonstone\workspace3",
};

var writeMutation = new ConcurrentQueue<Mutation>();
var newMutations = new ConcurrentQueue<Mutation>();
var newSnapshots = new ConcurrentQueue<Snapshot<ProjectionModel>>();

var handler = new MutationHandler<ProjectionModel>()
{
    MutationHandlers = new Dictionary<Type, object>()
    {
        {typeof(ChangeProjectName), new ChangeProjectNameHandler()},
        {typeof(CreateProject), new CreateProjectHandler()},
        {typeof(DeleteProject), new DeleteProjectHandler()},
    }
};
    
    
var writer = new MutationWriter(writeMutation, paths, cts.Token, loggerFactory.CreateLogger<MutationWriter>());
var reader = new MutationReader(newMutations, paths, cts.Token, loggerFactory.CreateLogger<MutationReader>());
var stream = new MutationStream<ProjectionModel>(newMutations, newSnapshots, paths, cts.Token, loggerFactory.CreateLogger<MutationStream<ProjectionModel>>(), handler);


// tests
var t1 = Task.Run(() =>
{
    while (true)
    {
        while (newSnapshots.TryDequeue(out var snapshot))
        {
            logger.Information("Projects: {Counter}", snapshot.Model.CreatedProjects);
        }
        
        Thread.Sleep(10);
    }
});

var t2 = Task.Run(() =>
{
    while (true)
    {
        //logger.Information("Write: {WriteMutationCount} - Read: {NewMutationCount}", writeMutation.Count, newMutations.Count);
        Thread.Sleep(1000);
    }
});


for (var i = 0; i < 1; i++)
{
    writeMutation.Enqueue(new CreateProject()
    {
        ProjectId = Guid.NewGuid(),
        Name = "Project 1",
    });
    
    Thread.Sleep(TimeSpan.FromMilliseconds(0.1));
}

while (true)
{
    logger.Information("Ready for input");
    Console.ReadKey();
    
    writeMutation.Enqueue(new DeleteProject()
    {
        ProjectId = Guid.NewGuid(),
    });
} 
