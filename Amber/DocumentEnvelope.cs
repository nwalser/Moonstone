using System.Reactive.Subjects;

namespace Amber;

public class DocumentEnvelope
{
    public Guid Id { get; private set; }
    public object Value { get; private set; }

    private readonly BehaviorSubject<object> _valueSubject;
    public IObservable<object> ValueObservable => _valueSubject;

    private readonly Func<DocumentEnvelope, object, Task> _applyMutation;
    
    public DocumentEnvelope(Guid id, object value, Func<DocumentEnvelope, object, Task> applyMutation)
    {
        Id = id;
        Value = value;
        
        _applyMutation = applyMutation;
        _valueSubject = new BehaviorSubject<object>(value);
    }

    public void UpdateValue(object value)
    {
        Value = value;
        _valueSubject.OnNext(Value);
    }

    public async Task ApplyMutation(object mutation)
    {
        await _applyMutation(this, mutation);
    }
}

public class DocumentEnvelope<TDocument>
{
    public Guid Id => _documentEnvelope.Id;
    public TDocument Value => (TDocument)_documentEnvelope.Value;
    public IObservable<TDocument> ValueObservable => (IObservable<TDocument>)_documentEnvelope.ValueObservable;

    private readonly DocumentEnvelope _documentEnvelope;
    
    public DocumentEnvelope(DocumentEnvelope documentEnvelope)
    {
        _documentEnvelope = documentEnvelope;
    }

    public async Task ApplyMutation(object mutation)
    {
        await _documentEnvelope.ApplyMutation(mutation);
    }
}