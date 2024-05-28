using System.Text.Json;
using System.Threading.Channels;
using Moonstone.FileFormat.Deltas;
using Moonstone.FileFormat.Domain;
using ProtoBuf;

namespace Moonstone.FileFormat;

public class Database : IDatabase
{
    private const long MaxSize = 10 * 1024 * 1024;
    
    private readonly string _path;
    private readonly string _session;
    private readonly Dictionary<int, Type> _typeMap;
    private FileInfo _currentLogFile;

    private readonly Dictionary<Guid, IDocument> _documents = new();
    private readonly HashSet<Guid> _deleted = [];
    private readonly Dictionary<string, long> _filePointers = new();
    private readonly FileSystemWatcher _watcher;

    private readonly Channel<FileSystemEventArgs> _changedFiles = Channel.CreateUnbounded<FileSystemEventArgs>();
    private Task? _backgroundTask;
    private CancellationTokenSource _cts = new();
    
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
        _watcher.Changed += (o, args) => _changedFiles.Writer.TryWrite(args);
        _watcher.EnableRaisingEvents = true;

        // rebuild current state
        var logFiles = Directory.EnumerateFiles(_path)
            .Select(f => new FileInfo(f));

        foreach (var logFile in logFiles)
            RescanFile(logFile);
        
        // init background worker
        _cts = new CancellationTokenSource();
        _backgroundTask = Task.Run(() => BackgroundTask(_cts.Token));
    }

    private async Task BackgroundTask(CancellationToken ct = default)
    {
        while (!ct.IsCancellationRequested)
        {
            var change = await _changedFiles.Reader.ReadAsync(ct);
            var fileInfo = new FileInfo(change.FullPath);
            RescanFile(fileInfo);
        }
    }

    private void RescanFile(FileInfo file)
    {
        // test if file is already known
        if (_filePointers.TryGetValue(file.FullName, out var filePointer) && filePointer >= file.Length)
            return;
            
        using var fs = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        while (fs.Position < fs.Length)
        {
            var delta = Serializer.DeserializeWithLengthPrefix<IDelta>(fs, PrefixStyle.Base128);
            ApplyDelta(delta);
        }
        
        _filePointers[file.FullName] = fs.Position;
        Console.WriteLine($"Rescanned: {file.FullName}");
    }

    private void ApplyDelta(IDelta delta)
    {
        lock (_documents)
        {
            var type = _typeMap[delta.TypeId];

            switch (delta)
            {
                case Update update:
                {
                    if (_deleted.Contains(update.RowId))
                        break;

                    if (_documents.TryGetValue(delta.RowId, out var existingDocument) && existingDocument.LastWrite >= delta.Timestamp)
                        break;
                    
                    var document = (IDocument)JsonSerializer.Deserialize(update.Json, type)!;
                    document.LastWrite = update.Timestamp;
                    _documents[delta.RowId] = document;
                    
                    break;
                }
                case Delete delete:
                {
                    _deleted.Add(delete.RowId);
                    _documents.Remove(delete.RowId);

                    break;
                }
            }
        }
    }

    private FileInfo NewLogFile()
    {
        var path = Path.Join(_path, $"{_session}_{Guid.NewGuid()}.bin");
        return new FileInfo(path);
    }

    public void Update(IDocument document) => Update([document]);
    
    public void Update(IEnumerable<IDocument> documents)
    {
        var deltas = documents.Select(document =>
        {
            return new Update
            {
                Timestamp = DateTime.UtcNow,
                TypeId = _typeMap.Single(t => t.Value == document.GetType()).Key,
                RowId = document.Id,
                Json = JsonSerializer.Serialize(document, document.GetType()),
            };
        });

        WriteDeltas(deltas);
    }

    public IEnumerable<IDocument> Enumerate()
    {
        return _documents.Select(d => d.Value);
    }
    
    public IEnumerable<TType> Enumerate<TType>()
    {
        return _documents.Select(v => v.Value)
            .Where(t => t.GetType() == typeof(TType))
            .Cast<TType>();
    }
    
    public void Remove(IDocument document) => Remove([document]);

    public void Remove(IEnumerable<IDocument> documents)
    {
        var deltas = documents.Select(document =>
        {
            return new Delete
            {
                Timestamp = DateTime.UtcNow,
                TypeId = _typeMap.Single(t => t.Value == document.GetType()).Key,
                RowId = document.Id,
            };
        });

        WriteDeltas(deltas);
    }

    private void WriteDeltas(IEnumerable<IDelta> deltas)
    {
        var fs = _currentLogFile.Open(FileMode.Append, FileAccess.Write, FileShare.Read);

        foreach (var delta in deltas)
        {
            // switch log file
            if (fs.Length >= MaxSize)
            {
                fs.Dispose();
                _currentLogFile = NewLogFile();
                fs = _currentLogFile.Open(FileMode.Append, FileAccess.Write, FileShare.Read);
            }
        
            Serializer.SerializeWithLengthPrefix(fs, delta, PrefixStyle.Base128);
            ApplyDelta(delta);
        }
        
        fs.Dispose();
    }
}