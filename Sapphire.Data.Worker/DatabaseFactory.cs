using Moonstone.Database;

namespace Sapphire.Data.Worker;

public class WorkerDatabase : Database
{
    private static readonly Dictionary<int, Type> TypeMap = new()
    {
        { 0, typeof(Entities.Worker) },
    };

    public WorkerDatabase(string session, string path) : base(TypeMap, session, path)
    {
    }
    
    public static WorkerDatabase Open(string path, string session)
    {
        var database = new WorkerDatabase(session, path);
        database.Open();
        
        return database;
    }
}