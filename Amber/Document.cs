using System.Reactive.Subjects;

namespace Amber;

public class Document
{
    public Guid Id { get; private set; }
    public object Value { get; private set; }

    private readonly BehaviorSubject<object> _valueSubject;
    public IObservable<object> ValueObservable => _valueSubject;

    private readonly Func<Document, object, Task> _applyMutation;
    
    public Document(Guid id, object value, Func<Document, object, Task> applyMutation)
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