namespace Amber.Doc;

public class DocumentEnvelope<TDocument>
{
    public required Guid Id { get; set; }
    public required Type Type { get; set; }
}