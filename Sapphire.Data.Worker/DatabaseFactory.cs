using Moonstone.Database;

namespace Sapphire.Data.Worker;

public class WorkerDatabase : Database
{
    protected override Dictionary<int, Type> TypeMap { get; } = new()
    {
        { 0, typeof(Entities.Worker) },
    };
}