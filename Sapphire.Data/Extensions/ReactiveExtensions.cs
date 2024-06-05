using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Sapphire.Data.Extensions;

public static class ReactiveExtensions
{
    public static IDisposable SubscribeWithoutOverlap<T>(this IObservable<T> source, Action<T> action)
    {
        var sampler = new Subject<Unit>();

        var sub = source
            .Sample(sampler)
            .ObserveOn(Scheduler.Default)
            .Subscribe(l =>
            {
                action(l);
                sampler.OnNext(Unit.Default);
            });

        // start sampling when we have a first value
        source.Take(1).Subscribe(_ => sampler.OnNext(Unit.Default));

        return sub;
    }
}