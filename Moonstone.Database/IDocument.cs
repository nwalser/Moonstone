namespace Moonstone.Database;

public interface IDocument
{
    public Guid Id { get; }
    public DateTime LastWrite { get; set; }
}