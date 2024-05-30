using Moonstone.Database;

namespace Sapphire.Data.Worker.Entities;

public class Worker : Document
{
    public required string Name { get; set; }
}