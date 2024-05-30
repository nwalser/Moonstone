using Moonstone.Database;
using Sapphire.Data.WorkerData.Entities;

namespace Sapphire.Data.WorkerData;

public class WorkerDatabase : Database
{
    protected override Dictionary<int, Type> TypeMap { get; } = new()
    {
        { 0, typeof(Worker) },
    };
}