namespace Moonstone;

public interface IWorkspace
{
    public IEnumerable<IDocument> EnumerateDocuments();
    
    public Task Create<TDocument>(Guid? id = default);
    
    public TDocument Load<TDocument>(Guid id);
    public IObservable<TDocument> Observe<TDocument>(Guid id);
    
    public Task ApplyMutation<TDocument>(Guid documentId, object mutation);
    
    public Task Delete<TDocument>(Guid documentId);
}