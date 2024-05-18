namespace Moonstone;

public interface IDocument
{
    public Guid Id { get; }
    public Type Type { get; }
}