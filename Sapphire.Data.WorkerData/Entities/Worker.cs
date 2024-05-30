using Moonstone.Database;

namespace Sapphire.Data.WorkerData.Entities;

public class Worker : Document
{
    public required string Name { get; set; }
}