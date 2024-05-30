using Moonstone.Database;
using Sapphire.Data.Project.Entities;

namespace Sapphire.Data.Project;

public class ProjectDatabase : Database
{
    private static readonly Dictionary<int, Type> TypeMap = new()
    {
        { 0, typeof(Entities.Project) },
        { 1, typeof(Todo) },
        { 2, typeof(PossibleWorkerAssignment) },
    };

    public ProjectDatabase(string session, string path) : base(TypeMap, session, path)
    {
    }
    
    public static ProjectDatabase Open(string path, string session)
    {
        var database = new ProjectDatabase(session, path);
        database.Open();
        
        return database;
    }
}