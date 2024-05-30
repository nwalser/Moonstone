using Moonstone.Database;

namespace Sapphire.Data.ProjectData.Entities;

public class PossibleWorkerAssignment : Document
{
    public required Guid WorkerId { get; set; }
    public required Guid TodoId { get; set; }
}