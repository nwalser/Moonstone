using System.Reactive.Subjects;
using Stream.Mutations;

namespace Stream.FileStore;

public class FileStoreManager
{
    private readonly string _folder;
    private readonly string _session;
    private readonly Subject<Mutation> _externalMutation;
    private readonly IObservable<Mutation> _persistMutation;
    private readonly FileSystemWatcher _fileSystemWatcher;
    
    private string SessionPath => Path.Join(_folder, _session);
    
    
    public FileStoreManager(Subject<Mutation> externalMutation, string folder, string session, IObservable<Mutation> persistMutation)
    {
        _externalMutation = externalMutation;
        _folder = folder;
        _session = session;
        _persistMutation = persistMutation;

        _fileSystemWatcher = new FileSystemWatcher(_folder)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = false,
            NotifyFilter = NotifyFilters.LastWrite
        };
    }

    public Task ActivateAsync()
    {
        return Task.Run(() =>
        {
            _persistMutation.Subscribe(PersistMutation);
            
            // subscribe to file events
            _fileSystemWatcher.EnableRaisingEvents = true;
            _fileSystemWatcher.Created += (_, e) =>
            {
                if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory))
                    return;

                PushExternalMutation(e.FullPath);
            };
            
            _fileSystemWatcher.Changed += (_, e) =>
            {
                if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory))
                    return;

                PushExternalMutation(e.FullPath);
            };

            Directory.CreateDirectory(SessionPath);
            foreach (var file in Directory.EnumerateFiles(SessionPath, string.Empty, SearchOption.AllDirectories))
            {
                PushExternalMutation(file);
            }
        });
    }

    private void PersistMutation(Mutation mutation)
    {
        Console.WriteLine($"Mutation Persisted: {mutation.MutationId}");

        var filePath = Path.Join(SessionPath, mutation.MutationId.ToString());
        var json = MutationSerializer.Serialize(mutation);
        File.WriteAllText(filePath, json);
    }
    
    private void PushExternalMutation(string mutationPath)
    {
        var json = File.ReadAllText(mutationPath);
        var mutation = MutationSerializer.Deserialize(json);
        
        _externalMutation.OnNext(mutation);
    }
}