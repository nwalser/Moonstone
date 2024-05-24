namespace EfCore.Sketch;

public class Todo
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    
    public required Project Project { get; set; }
}