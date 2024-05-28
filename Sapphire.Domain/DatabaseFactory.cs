using Moonstone.Database;

namespace Sapphire.Domain;

public static class DatabaseFactory
{
    public static Database Open(string path, string session)
    {
        var typeMap = new Dictionary<int, Type>
        {
            { 0, typeof(Project) },
            { 1, typeof(Todo) },
            { 2, typeof(Worker) },
            { 3, typeof(PossibleWorkerAssignment) },
        };
        
        var database = new Database(typeMap, session, path);
        database.Open();
        
        return database;
    }
}