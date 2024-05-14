namespace Amber.Doc;

public class DocumentStore<TDocument> where TDocument : new()
{
    private string _name;
    private Guid _sessionId;
    private List<MutationDefinition<TDocument>> _mutationDefinitions;


    public List<DocumentEnvelope<TDocument>> Documents { get; init; } = [];
    
    
    public DocumentStore(string name, List<MutationDefinition<TDocument>> mutationDefinitions, Guid sessionId)
    {
        _name = name;
        _mutationDefinitions = mutationDefinitions;
        _sessionId = sessionId;
    }


    public Task Initialize()
    {
        throw new NotImplementedException();
    }
    
    
    public Task CacheAll()
    {
        throw new NotImplementedException();
    }

    public Task<DocumentEnvelope> Create()
    {
        throw new NotImplementedException();
    }
}