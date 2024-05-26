using Opal.Domain;

namespace Opal;

public interface IDatabase
{
    public void Update(IDocument document);
    public void Remove(IDocument document);

    public IEnumerable<TType> Enumerate<TType>();
}