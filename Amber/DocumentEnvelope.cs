namespace Amber;

public class DocumentEnvelope
{
    public required Guid Id { get; init; }
    public required object Document { get; set; }
    
    public void ApplyMutation(object mutation)
    {
        // apply mutation to document
        throw new NotImplementedException();
    }
}