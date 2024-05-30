using Moonstone.Database;
using Sapphire.Data.ProjectData.Entities;

namespace Sapphire.Data.ProjectData;

public class ProjectDatabase : Database
{
    protected override Dictionary<int, Type> TypeMap { get; } = new()
    {
        { 0, typeof(Project) },
        { 1, typeof(Todo) },
        { 2, typeof(PossibleWorkerAssignment) },
    };

    public override void Create(string path, string session)
    {
        base.Create(path, session);
        
        // create initial objects
        Update(new Project()
        {
            Name = "Project Name"
        });
        Update(new Todo()
        {
            Name = "Todo 1"
        });
        Update(new Todo()
        {
            Name = "Todo 2"
        });
    }

    public Project GetProject()
    {
        return Enumerate<Project>().Single();
    }
}