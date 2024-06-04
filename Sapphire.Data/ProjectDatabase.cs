using Moonstone.Database;
using Sapphire.Data.Entities;

namespace Sapphire.Data;

public class ProjectDatabase : Database
{
    protected override Dictionary<int, Type> TypeMap { get; } = new()
    {
        { 0, typeof(ProjectAggregate) },
        { 1, typeof(TodoAggregate) },
        { 2, typeof(PossibleWorkerAssignment) },
        { 3, typeof(WorkerAggregate) },
    };

    public override void Create(string path, string session)
    {
        base.Create(path, session);
        
        // create initial objects
        var project = new ProjectAggregate()
        {
            Name = "Project Name"
        };
        Update(project);
        Update(new TodoAggregate()
        {
            Name = "Todo 1",
            ProjectId = project.Id,
        });
        Update(new TodoAggregate()
        {
            Name = "Todo 2",
            ProjectId = project.Id,
        });
    }
}