using System.Reactive.Subjects;

namespace Moonstone;

public class Document(Guid id, Type type, object value) : IDocument
{
    public Guid Id { get; } = id;
    public Type Type { get; } = type;
    private readonly BehaviorSubject<object> _valueSubject = new(value);

    public object Value => _valueSubject.Value;
    public BehaviorSubject<object> ValueObservable => _valueSubject;

    
    public void UpdateValue(object value)
    {
        _valueSubject.OnNext(value);
    }
}