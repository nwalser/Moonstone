namespace Opal;

public interface IDocument
{
    public Guid Id { get; }
    public DateTime LastWrite { get; }
}