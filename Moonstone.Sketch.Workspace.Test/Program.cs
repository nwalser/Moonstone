using System.Diagnostics;
using Moonstone.Domain.Mutations.Project.ChangeName;
using Moonstone.Domain.Mutations.Project.Create;
using Moonstone.Domain.Mutations.Project.Delete;
using Moonstone.Framework;
using Moonstone.Framework.Stream;
using Moonstone.Workspace;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var sw = Stopwatch.StartNew();

var paths = new PathProvider()
{
    Temporary = @"C:\Users\Nathaniel Walser\Desktop\temp",
    Session = "040461cf-f8cb-4bcb-9352-1edeb67c5d9a",
    Workspace = @"C:\Users\Nathaniel Walser\OneDrive - esp-engineering gmbh\Moonstone\workspace3",
};

var handler = new MutationHandler<ProjectionModel>()
{
    MutationHandlers = new Dictionary<Type, object>()
    {
        {typeof(ChangeProjectName), new ChangeProjectNameHandler()},
        {typeof(CreateProject), new CreateProjectHandler()},
        {typeof(DeleteProject), new DeleteProjectHandler()},
    }
};

var workspace = new Workspace<ProjectionModel>(paths, handler);

workspace.Projection.Subscribe(p => Log.Information("{Count}", p.CreatedProjects));

Log.Information("Init of objects took: {Seconds:N3}s", (double)sw.ElapsedMilliseconds/1000);
sw.Restart();

await workspace.Open();

Log.Information("Opening workspace took: {Seconds:N3}s", (double)sw.ElapsedMilliseconds/1000);
sw.Restart();

while (true)
{
    Log.Information("Ready for input");
    var key = Console.ReadKey();

    if (key.KeyChar == 'b')
        break;
    
    workspace.ApplyMutation(new CreateProject()
    {
        ProjectId = Guid.NewGuid(),
        Name = "Project 1",
    });
}

Log.Information("Applying mutations took: {Seconds:N3}s", (double)sw.ElapsedMilliseconds/1000);
sw.Restart();

await workspace.Close();

Log.Information("Closing workspace took: {Seconds:N3}s", (double)sw.ElapsedMilliseconds/1000);
sw.Restart();
