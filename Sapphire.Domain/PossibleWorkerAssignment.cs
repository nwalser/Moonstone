using Moonstone.Database;

namespace Sapphire.Domain;

public class PossibleWorkerAssignment : Document
{
    public required Guid WorkerId { get; set; }
    public required Guid TodoId { get; set; }
}