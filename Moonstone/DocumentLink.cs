using System.Diagnostics.CodeAnalysis;

namespace Moonstone;

public class DocumentLink<TDocument>(Guid id)
{
    public Guid Id { get; } = id;

    public static DocumentLink<TDocument> Create(Document document)
    {
        if (document.Type != typeof(TDocument)) throw new Exception();

        return new DocumentLink<TDocument>(id: document.Id);
    }
    
    public TDocument Load(IWorkspace workspace)
    {
        return workspace.Load<TDocument>(Id);
    }
}