namespace Moonstone;

public interface IHandler<TDocument>
{
    public Dictionary<int, Type> MutationTypes { get; }
    
    public TDocument CreateNew();
    public void ApplyMutation(TDocument aggregate, object mutation);
}