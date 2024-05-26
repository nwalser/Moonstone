using System.Text.Json;
using Opal.Deltas;
using Opal.Domain;
using ProtoBuf;

namespace Opal;

public class Database : IDatabase
{
    private const long MaxSize = 1024 * 10;
    
    private readonly string _path;
    private readonly string _session;
    private readonly Dictionary<int, Type> _typeMap;
    private FileInfo _currentLogFile;

    private readonly Dictionary<int, Dictionary<Guid, IDocument>> _documents = new();
    private readonly Dictionary<string, long> _filePointers = new();
    private readonly FileSystemWatcher _watcher;
    private readonly Queue<FileSystemEventArgs> _changedFiles = new();
    
    
    public Database(Dictionary<int, Type> typeMap, string session, string path)
    {
        _typeMap = typeMap;
        _session = session;
        _path = path;

        _watcher = new FileSystemWatcher()
        {
            Path = _path,
            NotifyFilter = NotifyFilters.LastWrite,
            IncludeSubdirectories = true,
            EnableRaisingEvents = false,
        };
    }

    public void Open()
    {
        // get last log file
        _currentLogFile = Directory
            .EnumerateFiles(_path, $"{_session}_*.bin")
            .Select(p => new FileInfo(p))
            .Where(p => p.Length < MaxSize)
            .MinBy(f => f.Length) ?? NewLogFile();

        // setup file system watcher
        _watcher.Changed += (o, args) => _changedFiles.Enqueue(args);
        _watcher.EnableRaisingEvents = true;
        
        // rebuild current state
        RescanChanges();
    }

    private void RescanChanges()
    {
        var logFiles = Directory.EnumerateFiles(_path)
            .Select(f => new FileInfo(f));

        foreach (var logFile in logFiles)
        {
            // test if file is already known
            if (_filePointers.TryGetValue(logFile.FullName, out var filePointer) && filePointer >= logFile.Length)
                continue;
            
            using var fs = logFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);

            while (fs.Position < fs.Length)
            {
                var delta = Serializer.DeserializeWithLengthPrefix<IDelta>(fs, PrefixStyle.Base128);
                var type = _typeMap[delta.TypeId];

                switch (delta)
                {
                    case Update update:
                    {
                        _documents.TryAdd(delta.TypeId, new Dictionary<Guid, IDocument>());
                        var table = _documents[delta.TypeId];
                    
                        // do not replace if item exists and is newer as the new delta
                        if (!(table.TryGetValue(delta.RowId, out var document) && document.LastWrite >= delta.Timestamp))
                            table[delta.RowId] = (IDocument)JsonSerializer.Deserialize(update.Json, type)!;
                        
                        break;
                    }
                    case Delete delete:
                    {
                        throw new NotImplementedException();
                        break;
                    }
                }
            }

            _filePointers[logFile.FullName] = fs.Position;
        }
    }

    private FileInfo NewLogFile()
    {
        var path = Path.Join(_path, $"{_session}_{Guid.NewGuid()}.bin");
        return new FileInfo(path);
    }
    
    public void Update(IDocument document)
    {
        var update = new Update
        {
            Timestamp = DateTime.UtcNow,
            TypeId = _typeMap.Single(t => t.Value == document.GetType()).Key,
            RowId = document.Id,
            Json = JsonSerializer.Serialize(document, document.GetType()),
        };

        WriteDelta(update);
    }

    public void Remove(IDocument document)
    {
        var delete = new Delete
        {
            Timestamp = DateTime.UtcNow,
            TypeId = _typeMap.Single(t => t.Value == document.GetType()).Key,
            RowId = document.Id,
        };

        WriteDelta(delete);
    }

    private void WriteDelta(IDelta delta)
    {
        if (_currentLogFile is { Exists: true, Length: >= MaxSize })
            _currentLogFile = NewLogFile();
        
        using var fs = _currentLogFile.Open(FileMode.Append, FileAccess.Write, FileShare.Read);
        Serializer.SerializeWithLengthPrefix(fs, delta, PrefixStyle.Base128);
    }
}