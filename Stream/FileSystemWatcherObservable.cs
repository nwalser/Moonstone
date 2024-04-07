using System.Reactive.Linq;

namespace Stream;

public static class FileSystemWatcherObservable
{
    public static IObservable<string> Create(string folder)
    {
        return
            Observable
                .Defer(() => CreateWatcher(folder))
                .SelectMany(fsw =>
                    Observable
                        .Merge(Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(h => fsw.Created += h, h => fsw.Created -= h))
                        .Select(ep => ep.EventArgs.FullPath)
                        .Finally(fsw.Dispose))
                .Publish()
                .RefCount();
    }

    private static IObservable<FileSystemWatcher> CreateWatcher(string folder)
    {
        FileSystemWatcher fsw = new(folder);
        fsw.EnableRaisingEvents = true;

        return Observable.Return(fsw);
    }
}