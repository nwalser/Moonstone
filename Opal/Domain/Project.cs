namespace Opal.Domain;

public record Project : IDocument
{
    public required Guid Id { get; init; }
    public DateTime LastWrite { get; init; } = DateTime.MinValue;
    
    public required string Name { get; init; }
}