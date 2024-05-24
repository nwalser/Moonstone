namespace Map.Sketch.Domain;

public class Project : IDocument
{
    public required Guid Id { get; init; }
    public required string Name { get; set; }
}