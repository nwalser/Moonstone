namespace Amber.Documents;

public interface IHandler
{
    public int DocumentTypeId { get; }
    public Type DocumentType { get; }
    public Dictionary<int, Type> MutationTypes { get; }
    
    public object CreateNew();
    public void ApplyMutation(object project, object mutation);
}
