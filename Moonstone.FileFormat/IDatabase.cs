using Moonstone.FileFormat.Domain;

namespace Moonstone.FileFormat;

public interface IDatabase
{
    public void Update(IDocument document);
    public void Remove(IDocument document);

    public IEnumerable<TType> Enumerate<TType>();
}