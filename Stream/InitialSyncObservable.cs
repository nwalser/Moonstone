using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Stream;

public static class InitialSyncObservable
{
    public static IObservable<string> Create(string folder)
    {
        return Observable.Create<string>(o =>
        {
            foreach (var file in Directory.EnumerateFiles(folder)) 
                o.OnNext(file);
            
            o.OnCompleted();
            return Disposable.Empty;
        });
    }
}