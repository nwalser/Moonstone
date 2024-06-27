using Moonstone.Database;

namespace Sapphire.Data.Entities.Todo;

public class TodoWorkerMetadata : Document
{
    public required Guid TodoId { get; init; }
    public required Guid? WorkerId { get; init; }
    public bool Expanded { get; set; } = true;
    public bool Editing { get; set; } = false;
}