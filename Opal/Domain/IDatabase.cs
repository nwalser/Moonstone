namespace Opal.Domain;

public interface IDatabase
{
    public void Update(IDocument document);
    public void Remove(IDocument document);
}