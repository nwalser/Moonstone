namespace Moonstone.Database;

public interface IDatabase
{
    public void Update(Document document);
    public void Remove(Document document);

    public IEnumerable<TType> Enumerate<TType>() where TType : Document;
}