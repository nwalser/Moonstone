using System.Reactive.Subjects;

namespace Moonstone;

public class Document(Guid id, object value)
{
    public Guid Id { get; } = id;
    private readonly BehaviorSubject<object> _valueSubject = new(value);

    public object Value => _valueSubject.Value;
    public IObservable<object> ValueObservable => _valueSubject;

    
    public void UpdateValue(object value)
    {
        _valueSubject.OnNext(value);
    }
}