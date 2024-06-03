using Moonstone.Database;

namespace Sapphire.Data.Entities;

public class WorkerAggregate : Document
{
    public required string Name { get; set; }
}