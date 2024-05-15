using System.Reactive.Subjects;

namespace Amber.Ws;

public class Document
{
    public Guid Id { get; private set; }
    public object Value { get; private set; }

    private readonly Subject<object> _valueSubject;
    public IObservable<object> ValueObservable => _valueSubject;

    public Document(Guid id, object value)
    {
        Id = id;
        Value = value;
        
        _valueSubject = new Subject<object>();
    }

    public void UpdateValue(object value)
    {
        Value = value;
        _valueSubject.OnNext(Value);
    }
}