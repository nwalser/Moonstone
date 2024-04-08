using System.Reactive.Subjects;
using ProtoBuf;

namespace DistributedSessions;

public class FileStoreManager
{
    private readonly string _workspace;
    private readonly Subject<Mutation> _externalMutation;
    private readonly FileSystemWatcher _fileSystemWatcher;
    
    public FileStoreManager(Subject<Mutation> externalMutation, string workspace)
    {
        _externalMutation = externalMutation;
        _workspace = workspace;

        _fileSystemWatcher = new FileSystemWatcher(_workspace)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = false,
            NotifyFilter = NotifyFilters.LastWrite
        };
    }

    public Task InitializeAsync()
    {
        return Task.Run(() =>
        {
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

            Directory.CreateDirectory(_workspace);
            
            foreach (var file in Directory.EnumerateFiles(_workspace, string.Empty, SearchOption.AllDirectories))
                PushExternalMutation(file);
        });
    }
    
    private void PushExternalMutation(string mutationPath)
    {
        lock (_externalMutation)
        {
            using var stream = File.Open(mutationPath, FileMode.Open, FileAccess.Read, FileShare.Write);
            var mutations = Serializer.DeserializeItems<Mutation>(stream, PrefixStyle.Base128, 0).ToList();

            foreach (var mutation in mutations)
                _externalMutation.OnNext(mutation);
        }
    }
}