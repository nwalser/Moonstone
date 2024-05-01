using App;
using Framework;
using Serilog;
using Log = Serilog.Log;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
    
var paths = new PathProvider()
{
    Temporary = @"C:\Users\Nathaniel Walser\Desktop\temp",
    Session = "040461cf-f8cb-4bcb-9352-1edeb67c5d9a",
    Workspace = @"C:\Users\Nathaniel Walser\OneDrive - esp-engineering gmbh\Moonstone\workspace4",
};

var workspace = Workspace<Projection>.InitializeFrom(paths);

const int totalMutations = 100_000;
for (var i = 0; i < totalMutations; i++)
{
    workspace.AppendMutation(new CreateProject()
    {
        Id = Guid.NewGuid(),
        Name = "Project 1"
    });
    
    if(i % 1000 == 0) 
        Log.Information("{CurrentMutation}/{TotalMutations}", i, totalMutations);
}