namespace Opal.Domain;

public record Project : IDocument
{
    public required Guid Id { get; init; }
    public DateTime LastWrite { get; set; } = DateTime.MinValue;
    
    public required string Name { get; set; }
    public string Name2 { get; set; }
    public string Name3 { get; set; }
    public string Name4 { get; set; }
    public string Name5 { get; set; }
    public string Name6 { get; set; }
    public string Name7 { get; set; }
    public string Name8 { get; set; }
    public string Name9 { get; set; }
}