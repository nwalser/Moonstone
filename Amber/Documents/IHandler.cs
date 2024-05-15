namespace Amber.Documents;

public interface IHandler<TDocument>
{
    public Dictionary<int, Type> MutationTypes { get; }
    public TDocument CreateNew();
    public void ApplyMutation(TDocument project, object mutation);
}