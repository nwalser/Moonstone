namespace Moonstone.Database;

public class Document
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime LastWrite { get; internal set; } = DateTime.UtcNow;
}