using System.Collections.Concurrent;
using Opal.Cache;
using Opal.Mutations;
using ProtoBuf;

namespace Opal.Log;

public class MutationSync<TMutation>
{
    private const PrefixStyle PrefixStyle = ProtoBuf.PrefixStyle.Base128;
    private const int FieldNumber = 0;
    
    private readonly string _mutationsPath;
    private readonly CacheContext _store;
    private readonly ConcurrentQueue<MutationFile> _changedFiles;
    
    public MutationSync(string mutationsPath, CacheContext store)
    {
        _mutationsPath = mutationsPath;
        _store = store;
        _changedFiles = new ConcurrentQueue<MutationFile>();
    }
    
    public void Initialize()
    {
        // todo watcher does not work properly
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

        // retest all files for changes
        foreach (var file in GetAllMutationFiles())
            _changedFiles.Enqueue(file);
    }

    public async Task ProcessWork(CancellationToken ct = default)
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
        {
            filePointer = FilePointer.Create(file.FileId);
            _store.Add(filePointer);
            await _store.SaveChangesAsync();
        }

        var filePath = Path.Join(_mutationsPath, file.GetFilenameWithExtension());
        var fileSize = new FileInfo(filePath).Length;

        if (filePointer.ReadPosition == fileSize)
            return;
        
        await using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        stream.Seek(filePointer.ReadPosition, SeekOrigin.Begin);
        
        while (stream.Position < stream.Length)
        {
            var mutationEnvelope = Serializer.DeserializeWithLengthPrefix<MutationEnvelope<TMutation>>(stream, PrefixStyle, FieldNumber);
            var mutation = Mutation.FromMutationEnvelope(mutationEnvelope);
            _store.Mutation.Add(mutation);
        }

        filePointer.ReadPosition = stream.Position;
        
        _store.Update(filePointer);
        await _store.SaveChangesAsync();
    }
}