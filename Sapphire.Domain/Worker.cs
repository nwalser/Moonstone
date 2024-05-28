using Moonstone.Database;

namespace Sapphire.Domain;

public class Worker : Document
{
    public required string Name { get; set; }
}