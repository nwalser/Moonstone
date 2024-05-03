using System.Collections.Concurrent;
using Opal.Cache;
using Opal.Mutations;

namespace Opal.Log;

public class StreamSync
{
    private readonly string _mutationsPath;
    private readonly CacheContext _store;
    private readonly ConcurrentQueue<MutationFile> _changedFiles;
    
    public StreamSync(string mutationsPath, CacheContext store)
    {
        _mutationsPath = mutationsPath;
        _store = store;
        _changedFiles = new ConcurrentQueue<MutationFile>();
    }
    
    public async Task Initialize()
    {
        // create and enable file system watcher
        var fileSystemWatcher = new FileSystemWatcher
        {
            Path = _mutationsPath,
            IncludeSubdirectories = false,
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.LastWrite,
        };
        fileSystemWatcher.Created += FileChanged;
        fileSystemWatcher.Changed += FileChanged;

        // catch up with changes that occured while being offline
        foreach (var file in GetAllMutationFiles())
            _changedFiles.Enqueue(file);

        await ProcessChangedFiles();
    }

    public async Task ProcessChangedFiles()
    {
        while (_changedFiles.TryDequeue(out var changedFile))
            await SyncFile(changedFile);
    }

    private IEnumerable<MutationFile> GetAllMutationFiles()
    {
        return Directory.EnumerateFiles(_mutationsPath)
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .Select(f => MutationFile.ParseFromFilename(f));
    }
    
    private void FileChanged(object sender, FileSystemEventArgs e)
    {
        if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory))
            return;

        var filename = Path.GetFileNameWithoutExtension(e.FullPath);
        var mutationFile = MutationFile.ParseFromFilename(filename);
    
        if(!_changedFiles.Contains(mutationFile))
            _changedFiles.Enqueue(mutationFile);
    }

    private async Task SyncFile(MutationFile file)
    {
        var filePointer = _store.FilePointers.SingleOrDefault(p => p.FileId == file.FileId);

        if (filePointer is null)
            filePointer = FilePointer.Create(file.FileId);

        if (filePointer.ReadToEnd)
            return;
    
        using var logReader = LogReader.Open(Path.Join(_mutationsPath, file.GetFilename()));
    
        logReader.Skip(filePointer.NumberOfReadEntries);

        while (!logReader.EndOfStream)
        {
            var mutationEnvelope = logReader.ReadNext<MutationEnvelope<MutationBase>>();
            var mutation = Mutation.FromMutationEnvelope(mutationEnvelope);
        
            _store.Mutation.Add(mutation);
            filePointer.NumberOfReadEntries++;
        }

        if (file.Lock == LockState.Closed)
            filePointer.ReadToEnd = true;

        _store.Update(filePointer);
        await _store.SaveChangesAsync();
    }
}