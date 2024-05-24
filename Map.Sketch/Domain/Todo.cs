namespace Map.Sketch.Domain;

public class Todo : IDocument
{
    public required Guid Id { get; init; }
    public required string Name { get; set; }
}