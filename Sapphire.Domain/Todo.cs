using Moonstone.Database;

namespace Sapphire.Domain;

public class Todo : IDocument
{
    public required Guid Id { get; init; }
    public required DateTime LastWrite { get; set; }
    
    public required string Description { get; set; }
    public required bool IsChecked { get; set; }
}